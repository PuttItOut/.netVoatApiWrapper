using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VoatApiWrapper {


    public abstract class BaseApiProxy {

        private int _retryWaitTime = 1100;
        private AutoResetEvent _lockHandle = new AutoResetEvent(true);
        private bool _retryOnThrottleLimit = true;
        private int _maxThrottleRetryCount = 1;
        private JsonSerializerSettings _serializerSettings;

        public BaseApiProxy() {
            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.DateFormatString = "{0:s}";
        }


        public bool EnableMultiThreading { get; set; }

        public int MaxThrottleRetryCount {
            get { return _maxThrottleRetryCount; }
            set { if (value >= 0) { _maxThrottleRetryCount = value; } }
        }
        public bool RetryOnThrottleLimit {
            get { return _retryOnThrottleLimit; }
            set { _retryOnThrottleLimit = value; }
        }
        public int WaitTimeOnThrottleLimit { 
            get { return _retryWaitTime; } 
            set { if (value >= 0) { _retryWaitTime = value; } } 
        }
        

        protected ApiResponse Request(HttpMethod method, string endpoint, object body = null, object queryStringPairs = null) {

            //prepare body
            string jsonBody = null;
            if (method != HttpMethod.Get && body != null) {
                jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body, _serializerSettings);
            }

            //prepare querystrings 
            string queryString = null;
            if (queryStringPairs != null) {
                List<string> keyValuePairs = new List<string>();
                var properties = queryStringPairs.GetType().GetProperties();

                foreach (var property in properties) {
                    var val = property.GetValue(queryStringPairs);
                    var stringVal = val.ToString();
                    if (val is DateTime) {
                        stringVal = ((DateTime)val).ToString("s");
                    }
                    
                    keyValuePairs.Add(String.Format("{0}={1}", property.Name, stringVal));
                }
                queryString = String.Join("&", keyValuePairs);
            }


            bool enterLock = !EnableMultiThreading;

            try {

                if (enterLock) {
                    _lockHandle.WaitOne();
                }

                return PrivateRequest(method, endpoint, jsonBody, queryString);

            } finally {
                if (enterLock) {
                    _lockHandle.Set();
                }
            }

        }

        // This is a recursive method
        private ApiResponse PrivateRequest(HttpMethod method, string endpoint, string body, string queryString, int requestCount = 0) {

            if (!ApiInfo.IsValid) {
                throw new InvalidOperationException("ApiInfo object has invalid state.");
            }
            
            string finalEndpoint = Path.Combine(ApiInfo.BaseEndpoint, endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");

            HttpWebRequest req = WebRequest.CreateHttp(finalEndpoint);
            req.Method = method.Method;
            req.ContentType = "application/json";
            req.Headers.Add("Voat-ApiKey", ApiInfo.ApiPublicKey);

            ApiAuthenticator.Instance.AuthenticateRequest(req);
            
            if (method != HttpMethod.Get){
                using (var sw = new StreamWriter(req.GetRequestStream())) {
                    if (!String.IsNullOrEmpty(body)) {
                        sw.Write(body);
                    } else {
                        sw.Write("");
                    }
                }
            }

            HttpWebResponse response = null;
            try {
                response = (HttpWebResponse)req.GetResponse();
            } catch (WebException ex) {
                response = (HttpWebResponse)ex.Response;
            }

            if (response == null) {
                return Helper.NoServerResponse;
            }

            string responseString = Helper.ReadStream(response.GetResponseStream());
            ApiResponse apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(responseString);
            apiResponse.StatusCode = response.StatusCode;

            //reissue if ApiThrottleLimit exception
            if (!apiResponse.Success && apiResponse.Error != null) {
                if (apiResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //token expired
                    var refreshResponse = ApiAuthenticator.Instance.Refresh();
                    if (refreshResponse.Success)
                    {
                        return PrivateRequest(method, endpoint, body, queryString, (requestCount + 1));
                    }
                } else if (apiResponse.Error.Type == "ApiThrottleLimit" && RetryOnThrottleLimit && requestCount < MaxThrottleRetryCount) {
                    Console.WriteLine("[{1}] ApiThrottleLimit. Waiting {0}", WaitTimeOnThrottleLimit, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
                    System.Threading.Thread.Sleep(WaitTimeOnThrottleLimit);
                    return PrivateRequest(method, endpoint, body, queryString, (requestCount + 1));
                }
                return apiResponse;
            } else {
                return apiResponse;
            }

        }
    }
}
