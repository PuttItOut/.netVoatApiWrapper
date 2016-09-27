using System;
using System.Net.Http;
using VoatApiWrapper.Framework;
using VoatApiWrapper.Models;

namespace VoatApiWrapper
{
    //This isn't a complete coverage
    public class VoatApiProxy : BaseApiProxy
    {
        public VoatApiProxy(ApiAuthenticator authenticator)
        {
            base.Authenticator = authenticator;
        }

        #region Submission

        public ApiResponse SubmitDiscussion(string subverse, string title, string content)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}", subverse), new { title = title, content = content });
        }

        public ApiResponse SubmitLink(string subverse, string title, string url)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}", subverse), new { title = title, url = url });
        }

        public ApiResponse EditLinkSubmission(int submissionID, string title, string url)
        {
            return Request(HttpMethod.Put, String.Format("api/v1/submissions/{0}", submissionID.ToString()), new { title = title, url = url });
        }

        public ApiResponse EditDiscussionSubmission(int submissionID, string title, string content)
        {
            return Request(HttpMethod.Put, String.Format("api/v1/submissions/{0}", submissionID.ToString()), new { title = title, content = content });
        }

        public ApiResponse DeleteSubmission(int submissionID)
        {
            return Request(HttpMethod.Delete, String.Format("api/v1/submissions/{0}", submissionID.ToString()));
        }

        #endregion Submission

        #region Subverses

        public ApiResponse GetSubverseInfo(string subverse)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/{0}/info", subverse));
        }

        public ApiResponse BlockSubverse(string subverse)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}/block", subverse));
        }

        public ApiResponse UnblockSubverse(string subverse)
        {
            return Request(HttpMethod.Delete, String.Format("api/v1/v/{0}/block", subverse));
        }

        #endregion Subverses

        #region Get Submissions

        public ApiResponse GetSubmissionsBySubverse(string subverse, object searchOptions)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/{0}", subverse), null, searchOptions);
        }

        public ApiResponse GetSubmissionsAll(object searchOptions)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_all"), null, searchOptions);
        }

        public ApiResponse GetSubmissionsDefault(object searchOptions)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_default"), null, searchOptions);
        }

        public ApiResponse GetSubmissionsFront(object searchOptions)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_front"), null, searchOptions);
        }

        public ApiResponse GetSubmission(int submissionID)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/submissions/{0}", submissionID.ToString()));
        }

        #endregion Get Submissions

        #region Comments

        public ApiResponse GetComments(string subverse, int submissionID)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/v/{0}/{1}/comments", subverse, submissionID.ToString()));
        }

        public ApiResponse GetComment(int commentID)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/comments/{0}", commentID.ToString()));
        }

        public ApiResponse PostComment(string subverse, int submissionID, string comment)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}/{1}/comment", subverse, submissionID.ToString()), new { value = comment });
        }

        public ApiResponse PostCommentReply(int commentID, string comment)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/comments/{0}", commentID.ToString()), new { value = comment });
        }

        public ApiResponse EditComment(int commentID, string comment)
        {
            return Request(HttpMethod.Put, String.Format("api/v1/comments/{0}", commentID.ToString()), new { value = comment });
        }

        public ApiResponse DeleteComment(int commentID)
        {
            return Request(HttpMethod.Delete, String.Format("api/v1/comments/{0}", commentID.ToString()));
        }

        #endregion Comments

        #region Voting

        public ApiResponse VoteComment(int commentID, Vote vote, bool revokeOnRevote = true)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/vote/{0}/{1}/{2}", "comment", commentID.ToString(), (int)vote), null, new { revokeOnRevote = revokeOnRevote });
        }

        public ApiResponse VoteSubmission(int submissionID, Vote vote, bool revokeOnRevote = true)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/vote/{0}/{1}/{2}", "submission", submissionID.ToString(), (int)vote), null, new { revokeOnRevote = revokeOnRevote });
        }

        #endregion Voting

        #region Saving

        public ApiResponse SaveSubmission(int submissionID)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/submissions/{0}/save", submissionID.ToString()));
        }

        public ApiResponse UnsaveSubmission(int submissionID)
        {
            return Request(HttpMethod.Delete, String.Format("api/v1/submissions/{0}/save", submissionID.ToString()));
        }

        public ApiResponse SaveComment(int commentID)
        {
            return Request(HttpMethod.Post, String.Format("api/v1/comments/{0}/save", commentID.ToString()));
        }

        public ApiResponse UnsaveComment(int commentID)
        {
            return Request(HttpMethod.Delete, String.Format("api/v1/comments/{0}/save", commentID.ToString()));
        }

        #endregion Saving

        #region User

        public ApiResponse GetUserProfile(string userName)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/info", userName));
        }

        public ApiResponse GetUserComments(string userName, object search)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/comments", userName), null, search);
        }

        public ApiResponse GetUserSubmissions(string userName, object search)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/submissions", userName), null, search);
        }

        #endregion User

        #region Stream

        public ApiResponse GetSubmissionStream(string subverse = null)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/stream/submissions{0}", String.IsNullOrEmpty(subverse) ? "" : $"/v/{subverse}"));
        }
        public void PollSubmissionStream(Callback<ApiResponse> registration, string subverse = null)
        {
            registration.Method = HttpMethod.Get;
            registration.Endpoint = String.Format("api/v1/stream/submissions{0}", String.IsNullOrEmpty(subverse) ? "" : $"/v/{subverse}");
            RequestCallBack(registration);
        }

        public ApiResponse GetCommentStream(string subverse = null)
        {
            return Request(HttpMethod.Get, String.Format("api/v1/stream/comments{0}", String.IsNullOrEmpty(subverse) ? "" : $"/v/{subverse}"));
        }
        public void PollCommentStream(Callback<ApiResponse> registration, string subverse = null)
        {
            registration.Method = HttpMethod.Get;
            registration.Endpoint = String.Format("api/v1/stream/comments{0}", String.IsNullOrEmpty(subverse) ? "" : $"/v/{subverse}");
            RequestCallBack(registration);
        }
        #endregion Stream

        #region Misc

        public ApiResponse ServerStatus()
        {
            return Request(HttpMethod.Get, "api/v1/status");
        }

        public ApiResponse ServerTime()
        {
            return Request(HttpMethod.Get, "api/v1/time");
        }

        #endregion Misc

    }
}
