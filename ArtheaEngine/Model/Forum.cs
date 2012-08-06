using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    using NLog;

    #region Enumerations

    [Flags]
    public enum ForumPermissions
    {
        /// <summary>
        /// No permissions, all can read/write
        /// </summary>
        None,
        /// <summary>
        /// Only person in to or from fields
        /// </summary>
        Personal,
        /// <summary>
        /// admin can read/write
        /// </summary>
        Admin
    }

    #endregion Enumerations


    /// <summary>
    /// Implementation of Forum.
    /// </summary>
	/// 
	[BsonKnownTypes(typeof(AnnounceForum), typeof(GeneralForum), typeof(PersonalForum))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public class Forum : INamedEntity, IRootLevelDocument
    {
        #region Constructors
	
		static Forum() {
			DbManager.Synchronize(new Forum[] {
				new AnnounceForum(), 
				new GeneralForum(), 
				new PersonalForum()
			});
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Forum"/> class.
        /// </summary>
        public Forum()
        {
			Id = ObjectId.GenerateNewId().ToString();
            Notes = new ObservableCollection<Note>();
            Notes.AddChangedHandler<Note>((n, a) =>
            {
                n.Forum = a ? this : null;
            });
        }

        #endregion Constructors

        #region Properties

		[BsonIgnoreIfNull]
        public string Description
        {
			get; protected set;
        }

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
			get; protected set;
        }

        public string Name
        {
			get; protected set;
        }

        public ObservableCollection<Note> Notes
        {
            get; protected set;
        }

		[BsonDefaultValue(ForumPermissions.None)]
		[BsonIgnoreIfDefault]
        public ForumPermissions ReadPermission
        {
			get; protected set;
        }
		
		[BsonDefaultValue(ForumPermissions.None)]
		[BsonIgnoreIfDefault]
        public ForumPermissions WritePermission
        {
			get; protected set;
        }

        #endregion Properties

        #region Methods

        public static Forum Lookup(string id)
        {
			return DbManager.Lookup<Forum>(id);
        }

		public static IEnumerable<Forum> All ()
		{
			return DbManager.Query<Forum>();

		}

		public bool Save ()
		{
			return DbManager.Save (this);
		}

		public bool Delete()
		{
			return DbManager.Delete(this);
		}

        public bool ReadableBy(Character ch)
        {
            if (ReadPermission.HasFlag(ForumPermissions.Admin) && ch.Level < Game.Immortal)
                return false;

            return true;
        }

        /// <summary>
        /// Returns the number of unread notes for a player on this Forum.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns>the number of unread notes</returns>
        public int CountUnreadNotes(Player player)
        {
            if (ReadPermission.HasFlag(ForumPermissions.Admin) && (player.Account.Flags.HasFlag(AccountFlag.Admin)))
                return 0;

            int count = 0;

            foreach (Note n in Notes)
            {
                if (player.HasReadNote(n) ||
                    !n.ReadableBy(player))
                {
                    continue;
                }

                ++count;
            }

            return count;
        }

        /// <summary>
        /// Gets the next unread.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <returns></returns>
        public Note GetNextUnreadNote(Player player)
        {
            if (ReadPermission.HasFlag(ForumPermissions.Admin) && !player.Account.Flags.HasFlag(AccountFlag.Admin))
                return null;

            foreach (Note n in Notes)
            {
                if (player.HasReadNote(n) ||
                    !n.ReadableBy(player))
                {
                    continue;
                }

                return n;
            }

            return null;
        }

        /// <summary>
        /// Shows the note list to.
        /// </summary>
        /// <param name="player">The player.</param>
        public void ShowNoteListTo(Player player)
        {
            StringCollection columns = new StringCollection();

            var count = 0;

            foreach(var note in Notes)
            {
                if (!note.ReadableBy(player))
                    continue;

                columns.Add(string.Format("{0}{1,-2}> {2} ({3})",
                                          player.HasReadNote(note) ? "~R*~x" : " ",
                                          (count + 1), note.Subject, note.From));
                count++;
            }

            Columns.Show(player, 2, columns);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"></see> that represents the current <see cref="T:System.Object"></see>.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion Methods
	}

	public class AnnounceForum : Forum
	{
		public AnnounceForum() 
		{
			Name = "Announce";
			Description = "Forum for announcements.";
			ReadPermission = ForumPermissions.None;
			WritePermission = ForumPermissions.Admin;
		}
	}

	public class GeneralForum : Forum
	{
		public GeneralForum ()
		{
			Name = "General";
			Description = "General notes";
			ReadPermission = ForumPermissions.None;
			WritePermission = ForumPermissions.None;
		}
	}

	public class PersonalForum : Forum
	{
		public PersonalForum()
		{
			Name = "Personal";
			Description = "Forum to person to person notes.";
			ReadPermission = ForumPermissions.Personal;
			WritePermission = ForumPermissions.None;
		}
	}
}