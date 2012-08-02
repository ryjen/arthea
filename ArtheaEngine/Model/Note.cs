using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;

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

    public class Note : IEntity
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        /// <param name="to">To.</param>
        /// <param name="from">From.</param>
        /// <param name="subject">The subject.</param>
        /// <param name="text">The text.</param>
        public Note(MongoDBRef to, MongoDBRef from, string subject, string text)
        {
			Id = ObjectId.GenerateNewId().ToString();
            ToRef = to;
            FromRef = from;
            Subject = subject;
            Text = text;
            When = DateTime.Now;
        }

		public Note(IEntity to, IEntity from, string subject, string text)
			: this(new MongoDBRef(to.GetType().FullName, to.Id), 
			       new MongoDBRef(from.GetType().FullName, from.Id),
			       subject, text)
		{
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="Note"/> class.
        /// </summary>
        public Note()
        {
			Id = ObjectId.GenerateNewId().ToString();
            Text = string.Empty;
            When = DateTime.Now;
            Replies = new ObservableCollection<Note>();
            Replies.AddChangedHandler((n, a) =>
            {
                n.Original = a ? this : null;
            });
        }

        #endregion Constructors

        #region Properties

		[BsonIgnore]
        public Forum Forum
        {
            get; set;
        }

		[BsonRequired]
		[BsonElement("From")]
		public MongoDBRef FromRef {
			get; set;
        }

		[BsonIgnore]
		public IEntity From {
			get {
				return FromRef == null ? null : DbManager.Instance.FetchDBRefAs<IEntity>(FromRef);
			}
		}

		[BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id
        {
            get; set;
        }

		[BsonIgnoreIfNull]
		[BsonElement("Original")]
		public string OriginalRef {
			get { return Original != null ? null : Original.Id; }
			set { Original = Forum.Notes.Lookup (value); }
		}

		[BsonIgnore]
        public Note Original
        {
            get; set;
        }

        public ObservableCollection<Note> Replies
        {
            get; set;
        }

		[BsonRequired]
        public string Subject
        {
            get; set;
        }

		[BsonRequired]
        public string Text
        {
            get; set;
        }

		[BsonRequired]
		[BsonElement("To")]
		public MongoDBRef ToRef {
			get; set;
        }


		[BsonIgnore]
		public IEntity To {
			get {
				return ToRef == null ? null : DbManager.Instance.FetchDBRefAs<IEntity> (ToRef);
			}
		}

		[BsonRequired]
        public DateTime When
        {
            get; set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determines whether a specific note can be read on this Forum.
        /// </summary>
        /// <param name="note">The note.</param>
        /// <param name="player">The player.</param>
        /// <returns>
        /// 	<c>true</c> if this the specified note can be read on this Forum; otherwise, <c>false</c>.
        /// </returns>
        public bool ReadableBy(IEntity entity)
        {
            return (!Forum.ReadPermission.HasFlag(ForumPermissions.Personal) && (
                    (FromRef != null && FromRef.Id == entity.Id) ||
                    (ToRef != null && ToRef.Id == entity.Id)));
        }

        #endregion Methods
    }
}