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

        public ApiResponse GetUserProfile(string userName) {

            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/info", userName), null);

        }
        public ApiResponse GetUserComments(string userName) {

            return Request(HttpMethod.Get, String.Format("api/v1/u/{0}/comments", userName), null);

        }
    
    }
}
