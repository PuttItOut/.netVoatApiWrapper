using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper.Models
{
    public class Comment 
    {

        /// <summary>
        /// The raw content of this item.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Date comment was submitted.
        /// </summary>
        public DateTime CreationDate { get; set; }

        /// <summary>
        /// The formatted (MarkDown, Voat Content Processor) content of this item. This content is typically formatted into HTML output.
        /// </summary>
        public string FormattedContent { get; set; }

        /// <summary>
        /// The comment ID.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Marker for anon comment.
        /// </summary>
        public bool IsAnonymized { get; set; }

        /// <summary>
        /// Is this comment below the viewing threshold for the user.
        /// </summary>
        public bool IsCollapsed { get; set; }

        /// <summary>
        /// Marker for deleted comment.
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Marker for saved comment.
        /// </summary>
        public bool? IsSaved { get; set; }

        /// <summary>
        /// Marker for moderator distinguished comment.
        /// </summary>
        public bool IsDistinguished { get; set; }

        /// <summary>
        /// Marker for if current account owns this comment.
        /// </summary>
        public bool IsOwner { get; set; }

        /// <summary>
        /// Marker for if comment belongs to OP.
        /// </summary>
        public bool IsSubmitter { get; set; }

        /// <summary>
        /// Date comment was edited.
        /// </summary>
        public Nullable<DateTime> LastEditDate { get; set; }

        /// <summary>
        /// The parent comment ID. If null then comment is a root comment.
        /// </summary>
        public Nullable<int> ParentID { get; set; }

        /// <summary>
        /// The submission ID that this comment belongs.
        /// </summary>
        public Nullable<int> SubmissionID { get; set; }

        /// <summary>
        /// The subveres that this comment belongs.
        /// </summary>
        public string Subverse { get; set; }

        /// <summary>
        /// The user name who submitted the comment.
        /// </summary>
        public string UserName { get; set; }

        public int? Vote { get; set; }

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
