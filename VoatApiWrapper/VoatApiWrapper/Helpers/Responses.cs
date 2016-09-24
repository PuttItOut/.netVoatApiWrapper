using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {

   

    //This structure is stolen from the Voat v1 API source code. Just want to let everyone know that a theft has occured. That's right, I stole my own code and I'm turning myself in.
    public class ApiResponse {



        /// <summary>
        /// An absolute value indicating whether operation succeeded or failed. If this value is false the error object will be populated with details.
        /// </summary>
        [JsonProperty("statusCode")]
        public HttpStatusCode StatusCode { get; set; }


        /// <summary>
        /// An absolute value indicating whether operation succeeded or failed. If this value is false the error object will be populated with details.
        /// </summary>
        [JsonProperty("success", Order = 1)]
        public bool Success { get; set; }

        /// <summary>
        /// This field contains the JSON Payload. It's dynamic.
        /// </summary>
        [JsonProperty("data", Order = 1)]
        public dynamic Data { get; set; }


        /// <summary>
        /// If present the operation failed. Details are presented here.
        /// </summary>
        [JsonProperty("error", Order = 2, NullValueHandling = NullValueHandling.Ignore)]
        public ErrorInfo Error { get; set; }


        public class ErrorInfo {
            /// <summary>
            /// The error type code
            /// </summary>
            [JsonProperty("type")]
            public string Type { get; set; }

            /// <summary>
            /// Error description information
            /// </summary>
            [JsonProperty("message")]
            public string Message { get; set; }
        }

        [JsonIgnore]
        public bool HasData {
            get {
                return Data != null && Data.Count > 0;
            }
        }
    
    }

    public class OAuthErrorInfo {

        public OAuthErrorInfo() : base() { }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }

    }
}
