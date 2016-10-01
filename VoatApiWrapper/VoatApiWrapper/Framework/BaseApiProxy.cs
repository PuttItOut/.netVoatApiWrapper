using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VoatApiWrapper.Framework;
using VoatApiWrapper.Models;

namespace VoatApiWrapper
{
    public abstract class BaseApiProxy
    {
        private int _retryWaitTime = 1100;
        private AutoResetEvent _lockHandle = new AutoResetEvent(true);
        private bool _retryOnThrottleLimit = true;
        private int _maxThrottleRetryCount = 1;
        private JsonSerializerSettings _serializerSettings;
        private ApiAuthenticator _authenticator;

        public BaseApiProxy()
        {
            _serializerSettings = new JsonSerializerSettings();
            _serializerSettings.DateFormatString = "{0:s}";
        }

        public bool EnableMultiThreading { get; set; }

        public int MaxThrottleRetryCount
        {
            get { return _maxThrottleRetryCount; }
            set { if (value >= 0) { _maxThrottleRetryCount = value; } }
        }

        public bool RetryOnThrottleLimit
        {
            get { return _retryOnThrottleLimit; }
            set { _retryOnThrottleLimit = value; }
        }

        public int WaitTimeOnThrottleLimit
        {
            get { return _retryWaitTime; }
            set { if (value >= 0) { _retryWaitTime = value; } }
        }

        protected ApiAuthenticator Authenticator
        {
            get {
                if (_authenticator != null)
                {
                    return _authenticator;
                }
                //FailSafe
                return ApiAuthenticator.Instance;
            }
            set {
                _authenticator = value;
            }
        }
        /// <summary>
        /// This method loops
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="callback"></param>
        protected void RequestCallBack<T>(ApiResponseCallback<T> callback) 
        {
            Task task = new Task(async () =>
            {
                while (!callback.CancellationToken.IsCancellationRequested)
                {
                    var response = Request<T>(callback.Method, callback.Endpoint, callback.Body, callback.QueryStringPairs);
                    //((dynamic)callback.Handler).Invoke(this, response);
                    callback.Handler(this, response);
                    await Task.Delay(callback.Delay).ConfigureAwait(false);
                }
            }, callback.CancellationToken);
            callback.Task = task;
            task.Start();
        }

        protected ApiResponse Request(HttpMethod method, string endpoint, object body = null, object queryStringPairs = null)
        {
            return Request<dynamic>(method, endpoint, body, queryStringPairs);
        }

        protected ApiResponse<T> Request<T>(HttpMethod method, string endpoint, object body = null, object queryStringPairs = null)
        {
            //prepare body
            string jsonBody = null;
            if (method != HttpMethod.Get && body != null)
            {
                jsonBody = Newtonsoft.Json.JsonConvert.SerializeObject(body, _serializerSettings);
            }

            //prepare querystrings
            string queryString = null;
            if (queryStringPairs != null)
            {
                List<string> keyValuePairs = new List<string>();
                var properties = queryStringPairs.GetType().GetProperties();

                foreach (var property in properties)
                {
                    var val = property.GetValue(queryStringPairs);
                    var stringVal = val.ToString();
                    if (val is DateTime)
                    {
                        stringVal = ((DateTime)val).ToString("s");
                    }

                    keyValuePairs.Add(String.Format("{0}={1}", property.Name, stringVal));
                }
                queryString = String.Join("&", keyValuePairs);
            }

            bool enterLock = !EnableMultiThreading;

            try
            {
                if (enterLock)
                {
                    _lockHandle.WaitOne();
                }

                return PrivateRequest<T>(method, endpoint, jsonBody, queryString);
            }
            finally
            {
                if (enterLock)
                {
                    _lockHandle.Set();
                }
            }
        }

        private ApiResponse<T> PrivateRequest<T>(HttpMethod method, string endpoint, string body, string queryString, int requestCount = 0)
        {
            if (!ApiInfo.IsValid)
            {
                throw new InvalidOperationException("ApiInfo object has invalid state.");
            }

            string finalEndpoint = Path.Combine(ApiInfo.Endpoint, endpoint) + (!String.IsNullOrEmpty(queryString) ? "?" + queryString : "");

            HttpWebRequest req = WebRequest.CreateHttp(finalEndpoint);
            req.Method = method.Method;
            req.ContentType = "application/json";
            req.Headers.Add("Voat-ApiKey", ApiInfo.PublicKey);

            Authenticator.AuthenticateRequest(req);

            if (method != HttpMethod.Get)
            {
                using (var sw = new StreamWriter(req.GetRequestStream()))
                {
                    if (!String.IsNullOrEmpty(body))
                    {
                        sw.Write(body);
                    }
                    else
                    {
                        sw.Write("");
                    }
                }
            }

            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }

            if (response == null)
            {
                return Helper.NoServerResponse<T>();
            }

            string responseString = Helper.ReadStream(response.GetResponseStream());
            ApiResponse<T> apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse<T>>(responseString);
            apiResponse.StatusCode = response.StatusCode;

            //reissue if ApiThrottleLimit exception
            if (!apiResponse.Success && apiResponse.Error != null)
            {
                if (apiResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    //token expired
                    var refreshResponse = ApiAuthenticator.Instance.Refresh();
                    if (refreshResponse.Success)
                    {
                        return PrivateRequest<T>(method, endpoint, body, queryString, (requestCount + 1));
                    }
                }
                else if (apiResponse.Error.Type == "ApiThrottleLimit" && RetryOnThrottleLimit && requestCount < MaxThrottleRetryCount)
                {
                    Console.WriteLine("[{1}] ApiThrottleLimit. Waiting {0}", WaitTimeOnThrottleLimit, System.Threading.Thread.CurrentThread.ManagedThreadId.ToString());
                    System.Threading.Thread.Sleep(WaitTimeOnThrottleLimit);
                    return PrivateRequest<T>(method, endpoint, body, queryString, (requestCount + 1));
                }
                return apiResponse;
            }
            else
            {
                return apiResponse;
            }
        }
    }
}
