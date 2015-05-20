using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper
{
    //This is just a stub out currently. :(

    public class VoatApiProxy : BaseApiProxy {

        public ApiResponse PostDiscussion(string subverse, string title, string content) {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}", subverse), new { title = title, content = content });
        }

        public ApiResponse PostLink(string subverse, string title, string url) {
            return Request(HttpMethod.Post, String.Format("api/v1/v/{0}", subverse), new { title = title, url = url });
        }

        public ApiResponse GetSubmissionsBySubverse(string subverse, object searchOptions) {
            return Request(HttpMethod.Get, String.Format("api/v1/v/{0}", subverse), null, searchOptions);
        }

        public ApiResponse GetSubmissionsAll(object searchOptions) {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_all"), null, searchOptions);
        }

        public ApiResponse GetSubmissionsDefault(object searchOptions) {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_default"), null, searchOptions);
        }

        public ApiResponse GetSubmissionsFront(object searchOptions) {
            return Request(HttpMethod.Get, String.Format("api/v1/v/_front"), null, searchOptions);
        }
        
        public ApiResponse GetUserProfile(string userName) {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/info", userName));
        }
        
        public ApiResponse GetUserComments(string userName) {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/comments", userName));
        }
        
        public ApiResponse GetUserSubmissions(string userName) {
            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/submissions", userName));
        }
    
    }
}
