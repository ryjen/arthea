using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace ArtheaEngine.Model
{

	/// <summary>
	/// Forum permissions.
	/// </summary>
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


    /// <summary>
    /// Implementation of Forum.
    /// </summary>
	/// 
	[BsonKnownTypes(typeof(AnnounceForum), typeof(GeneralForum), typeof(PersonalForum))]
	[BsonDiscriminator(Required = true, RootClass = true)]
    public class Forum : INamedEntity, IRootLevelDocument
    {
		/// <summary>
		/// Initializes the <see cref="ArtheaEngine.Model.Forum"/> class.
		/// </summary>
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

		/// <summary>
		/// Gets or sets the description.
		/// </summary>
		/// <value>
		/// The description.
		/// </value>
		[BsonIgnoreIfNull]
        public string Description
        {
			get; protected set;
        }

		/// <summary>
		/// Gets the identifier.
		/// </summary>
		/// <value>
		/// The identifier.
		/// </value>
		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
			get; protected set;
        }

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>
		/// The name.
		/// </value>
        public string Name
        {
			get; protected set;
        }

		/// <summary>
		/// Gets or sets the notes.
		/// </summary>
		/// <value>
		/// The notes.
		/// </value>
        public ObservableCollection<Note> Notes
        {
            get; protected set;
        }

		/// <summary>
		/// Gets or sets the read permission.
		/// </summary>
		/// <value>
		/// The read permission.
		/// </value>
		[BsonDefaultValue(ForumPermissions.None)]
		[BsonIgnoreIfDefault]
        public ForumPermissions ReadPermission
        {
			get; protected set;
        }

		/// <summary>
		/// Gets or sets the write permission.
		/// </summary>
		/// <value>
		/// The write permission.
		/// </value>
		[BsonDefaultValue(ForumPermissions.None)]
		[BsonIgnoreIfDefault]
        public ForumPermissions WritePermission
        {
			get; protected set;
        }

		/// <summary>
		/// Lookup the specified id.
		/// </summary>
		/// <param name='id'>
		/// Identifier.
		/// </param>
        public static Forum Lookup(string id)
        {
			return DbManager.Lookup<Forum>(id);
        }

		/// <summary>
		/// All this instance.
		/// </summary>
		public static IEnumerable<Forum> All ()
		{
			return DbManager.Query<Forum>();

		}

		/// <summary>
		/// Save this instance.
		/// </summary>
		public bool Save ()
		{
			return DbManager.Save (this);
		}

		/// <summary>
		/// Delete this instance.
		/// </summary>
		public bool Delete()
		{
			return DbManager.Delete(this);
		}

		/// <summary>
		/// Readables the by.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if by was readabled, <c>false</c> otherwise.
		/// </returns>
		/// <param name='ch'>
		/// Ch.
		/// </param>
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
			ColumnFormatter columns = new ColumnFormatter(2, player);

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

			player.Page(columns);
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
	}

	/// <summary>
	/// Announce forum.
	/// </summary>
	public class AnnounceForum : Forum
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.AnnounceForum"/> class.
		/// </summary>
		public AnnounceForum() 
		{
			Name = "Announce";
			Description = "Forum for announcements.";
			ReadPermission = ForumPermissions.None;
			WritePermission = ForumPermissions.Admin;
		}
	}

	/// <summary>
	/// General forum.
	/// </summary>
	public class GeneralForum : Forum
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.GeneralForum"/> class.
		/// </summary>
		public GeneralForum ()
		{
			Name = "General";
			Description = "General notes";
			ReadPermission = ForumPermissions.None;
			WritePermission = ForumPermissions.None;
		}
	}

	/// <summary>
	/// Personal forum.
	/// </summary>
	public class PersonalForum : Forum
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.PersonalForum"/> class.
		/// </summary>
		public PersonalForum()
		{
			Name = "Personal";
			Description = "Forum to person to person notes.";
			ReadPermission = ForumPermissions.Personal;
			WritePermission = ForumPermissions.None;
		}
	}
}