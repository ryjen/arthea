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

    public class AnnounceForum : Forum
    {
        #region Properties

        public override string Description
        {
            get { return "Forum for announcements from administrators."; }
        }

        public override int Id
        {
            get { return 1; }
        }

        public override string Name
        {
            get { return "Announce"; }
        }

        public override ForumPermissions ReadPermission
        {
            get { return ForumPermissions.None; }
        }

        public override ForumPermissions WritePermission
        {
            get { return ForumPermissions.Admin; }
        }

        #endregion Properties
    }

    /// <summary>
    /// Implementation of Forum.
    /// </summary>
    public abstract class Forum : IEntity<int>, IPersistent
    {
        #region Fields

        public static readonly HashSet<Forum> List = new HashSet<Forum>();

        #endregion Fields

        #region Constructors

        static Forum()
        {
            var assm = Assembly.GetExecutingAssembly();

            using (var conn = ArtheaHelper.NewConnection())
            {
                foreach (var t in assm.GetTypes())
                {
                    if (t.IsSubclassOf(typeof(Forum)))
                    {
                        var b = (Forum)Activator.CreateInstance(t);
                        b.Load(conn);
                        List.Add(b);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Forum"/> class.
        /// </summary>
        public Forum()
        {
            Notes = new ObservableCollection<Note>();
            Notes.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Note>((n, a) =>
                {
                    n.Forum = a ? this : null;
                });
        }

        #endregion Constructors

        #region Properties

        public abstract string Description
        {
            get;
        }

        public abstract int Id
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public ObservableCollection<Note> Notes
        {
            get; set;
        }

        public abstract ForumPermissions ReadPermission
        {
            get;
        }

        public abstract ForumPermissions WritePermission
        {
            get;
        }

        #endregion Properties

        #region Methods

        public static Note FindNote(long id)
        {
            foreach (var b in List)
            {
                var n = b.Notes.SingleOrDefault(x => x.Id == id);

                if (n != null) return n;
            }
            return null;
        }

        public static Forum Lookup(int id)
        {
            return List.FirstOrDefault(x => x.Id == id);
        }

        public bool ReadableBy(Character ch)
        {
            if (ReadPermission.HasFlag(ForumPermissions.Admin) && ch.Level < ArtheaHelper.Immortal)
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

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_forum");
            cmd.AddParameter("@id", DbType.Int32, Id);

            return cmd.ExecuteNonQuery() == 1;
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

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_forum");
            cmd.AddParameter("@id", DbType.Int32, Id);

            Note.ReadAll(this);

            return this.Load(cmd);
        }

        public int MapRow(IDataRecord reader)
        {
            return 0;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("save_forum");

            cmd.AddParameter("@id", DbType.Int32, Id);
            cmd.AddParameter("@name", DbType.String, Name);
            cmd.AddParameter("@descr", DbType.String, Description);
            cmd.AddParameter("@read_perm", DbType.String, ReadPermission.ToString());
            cmd.AddParameter("@write_perm", DbType.String, WritePermission.ToString());

            return cmd.ExecuteNonQuery() > 0;
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
}