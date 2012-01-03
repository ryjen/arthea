namespace ArtheaEngine.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.Data;
    using System.Linq;
    using System.Text;

    using NLog;

    public class Note : IEntity<long>, IPersistent
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="text">The text.</param>
        public Note(string to, string from, string subject, string text)
        {
            To = to;
            From = from;
            Subject = subject;
            Text = text;
            When = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        public Note()
        {
            Text = string.Empty;
            When = DateTime.Now;
            Replies = new ObservableCollection<Note>();
            Replies.CollectionChanged += ArtheaHelper.CollectionChangedHandler<Note>((n, a) =>
            {
                n.Original = a ? this : null;
            });
        }

        #endregion Constructors

        #region Properties

        public Forum Forum
        {
            get; set;
        }

        public string From
        {
            get; set;
        }

        public long Id
        {
            get; set;
        }

        public Note Original
        {
            get; set;
        }

        public ObservableCollection<Note> Replies
        {
            get; set;
        }

        public string Subject
        {
            get; set;
        }

        public string Text
        {
            get; set;
        }

        public string To
        {
            get; set;
        }

        public DateTime When
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        public static void ReadAll(Forum Forum)
        {
            using (var conn = ArtheaHelper.NewConnection())
            {
                try
                {
                    var cmd = conn.CreateCommand("select_forum_notes");
                    cmd.AddParameter("@id", DbType.Int32, Forum.Id);

                    var reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        var note = new Note();

                        note.MapRow(reader);

                        Forum.Notes.Add(note);
                    }

                    foreach (var note in Forum.Notes)
                    {
                        note.ReadReplies(conn);
                    }
                }
                catch (Exception ex)
                {
                    LogManager.GetCurrentClassLogger().Error("could not read all notes: " + ex);
                }
            }
        }

        public bool Delete(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("delete_note");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@Forum", DbType.Int32, Forum.Id);

            return cmd.ExecuteNonQuery() == 1;
        }

        public bool Load(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("read_note");

            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@Forum", DbType.Int32, Forum.Id);

            var reader = cmd.ExecuteReader();

            if (!reader.Read()) return false;

            var i = MapRow(reader);

            if (!reader.IsDBNull(i))
            {
                var forum = Forum.Lookup(reader.GetInt32(i));
                if (forum != null)
                    forum.Notes.Add(this);
            }
            i++;

            if (!reader.IsDBNull(i))
                Original = Forum.FindNote(reader.GetInt64(i));
            return true;
        }

        public int MapRow(IDataRecord reader)
        {
            var i = 0;

            Id = reader.GetInt64(i++);
            Text = reader.GetString(i++);
            To = reader.GetString(i++);
            From = reader.GetString(i++);
            Subject = reader.GetString(i++);
            When = reader.GetDateTime(i++);

            return i;
        }

        public bool Save(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("update_note");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@text", DbType.String, Text);
            cmd.AddParameter("@to", DbType.String, To);
            cmd.AddParameter("@from", DbType.String, From);
            cmd.AddParameter("@subject", DbType.String, Subject);
            cmd.AddParameter("@when", DbType.DateTime, When);
            if (Forum != null)
                cmd.AddParameter("@forum", DbType.Int32, Forum.Id);
            else
                cmd.AddParameter("@forum", DbType.Int32, DBNull.Value);
            if (Original != null)
                cmd.AddParameter("@original", DbType.Int64, Original.Id);
            else
                cmd.AddParameter("@original", DbType.Int64, DBNull.Value);

            var res = cmd.ExecuteNonQuery() > 0;

            if (Id == 0 && res)
                Id = Convert.ToInt64(conn.LastInsertId());

            return res;
        }

        private void ReadReplies(IDbConnection conn)
        {
            var cmd = conn.CreateCommand("select_note_replies");
            cmd.AddParameter("@id", DbType.Int64, Id);
            cmd.AddParameter("@forum", DbType.Int32, Forum.Id);

            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var reply = new Note();

                reply.MapRow(reader);

                /*
                reply.Id = reader.GetInt64(0);
                reply.Text = reader.GetString(1);
                reply.To = reader.GetString(2);
                reply.From = reader.GetString(3);
                reply.Subject = reader.GetString(4);
                reply.When = reader.GetDateTime(5);*/

                Replies.Add(reply);

                reply.ReadReplies(conn);
            }
        }


        /// <summary>
        /// Determines whether a specific note can be read on this Forum.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="player">The player.</param>
        /// <returns>
        /// 	<c>true</c> if this the specified note can be read on this Forum; otherwise, <c>false</c>.
        /// </returns>
        public bool ReadableBy(Character player)
        {
            return (Forum.ReadPermission.HasFlag(ForumPermissions.Personal) ||
                    From == player.Name ||
                    To.Contains(player.Name));
        }

        #endregion Methods
    }
}