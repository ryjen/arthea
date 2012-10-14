using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System;
using System.Collections.ObjectModel;


namespace ArtheaEngine.Model
{

    public class Note : IEntity
    {
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

		/// <summary>
		/// Initializes a new instance of the <see cref="ArtheaEngine.Model.Note"/> class.
		/// </summary>
		/// <param name='to'>
		/// To.
		/// </param>
		/// <param name='from'>
		/// From.
		/// </param>
		/// <param name='subject'>
		/// Subject.
		/// </param>
		/// <param name='text'>
		/// Text.
		/// </param>
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

		/// <summary>
		/// Gets or sets the forum.
		/// </summary>
		/// <value>
		/// The forum.
		/// </value>
		[BsonIgnore]
        public Forum Forum
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets from reference.
		/// </summary>
		/// <value>
		/// From reference.
		/// </value>
		[BsonRequired]
		[BsonElement("From")]
		public MongoDBRef FromRef {
			get; set;
        }

		/// <summary>
		/// Gets from.
		/// </summary>
		/// <value>
		/// From.
		/// </value>
		[BsonIgnore]
		public IEntity From {
			get {
				return FromRef == null ? null : DbManager.Instance.FetchDBRefAs<IEntity>(FromRef);
			}
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
            get; set;
        }

		/// <summary>
		/// Gets or sets the original reference.
		/// </summary>
		/// <value>
		/// The original reference.
		/// </value>
		[BsonIgnoreIfNull]
		[BsonElement("Original")]
		public string OriginalRef {
			get { return Original != null ? null : Original.Id; }
			set { Original = Forum.Notes.Lookup (value); }
		}

		/// <summary>
		/// Gets or sets the original.
		/// </summary>
		/// <value>
		/// The original.
		/// </value>
		[BsonIgnore]
        public Note Original
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the replies.
		/// </summary>
		/// <value>
		/// The replies.
		/// </value>
        public ObservableCollection<Note> Replies
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the subject.
		/// </summary>
		/// <value>
		/// The subject.
		/// </value>
		[BsonRequired]
        public string Subject
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		/// <value>
		/// The text.
		/// </value>
		[BsonRequired]
        public string Text
        {
            get; set;
        }

		/// <summary>
		/// Gets or sets to reference.
		/// </summary>
		/// <value>
		/// To reference.
		/// </value>
		[BsonRequired]
		[BsonElement("To")]
		public MongoDBRef ToRef {
			get; set;
        }

		/// <summary>
		/// Gets to.
		/// </summary>
		/// <value>
		/// To.
		/// </value>
		[BsonIgnore]
		public IEntity To {
			get {
				return ToRef == null ? null : DbManager.Instance.FetchDBRefAs<IEntity> (ToRef);
			}
		}

		/// <summary>
		/// Gets or sets the when.
		/// </summary>
		/// <value>
		/// The when.
		/// </value>
		[BsonRequired]
        public DateTime When
        {
            get; set;
        }

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
    }
}