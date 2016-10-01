using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper.Models
{
    public class Submission
    {
        /// <summary>
        /// The submission ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// The subverse to which this submission belongs.
        /// </summary>
        public string Subverse { get; set; }

        /// <summary>
        /// The type of submission. Values: 1 for Self Posts, 2 for Link Posts
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// The user name who submitted the post.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// The submission title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The url for the submission if present.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// The thumbnail for submission.
        /// </summary>
        public string ThumbnailUrl { get; set; }

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        public string FormattedContent { get; set; }

        /// <summary>
        /// The number of comments submission current contains.
        /// </summary>
        public int CommentCount { get; set; }

        /// <summary>
        /// The date the submission was made.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// Date submission was edited.
        /// </summary>
        public Nullable<DateTime> LastEditDate { get; set; }

        /// <summary>
        /// Is this submission anon
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// Is this submission deleted
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// The view count of the submission.
        /// </summary>
        public int Views { get; set; }

        /// <summary>
        /// Status of vote for context user
        /// </summary>
        public int? Vote { get; set; }

        /// <summary>
        /// Total votes
        /// </summary>
        public int Sum { get; set; }

        /// <summary>
        /// UpCount count
        /// </summary>
        public int UpCount { get; set; }

        /// <summary>
        /// DownCount count
        /// </summary>
        public int DownCount { get; set; }

    }
}
