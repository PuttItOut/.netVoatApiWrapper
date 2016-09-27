using System;
using System.IO;
using System.Net;
using System.Threading;

namespace VoatApiWrapper
{
    public class ApiAuthenticator
    {
        private static ApiAuthenticator _instance;

        private AuthToken _token = null;

        private ITokenStore _tokenStore = null;

        private ITokenStore TokenStore
        {
            get
            {
                if (_tokenStore == null)
                {
                    _tokenStore = new DummyTokenStore();
                }
                return _tokenStore;
            }
        }

        public string UserName
        {
            get
            {
                return _token == null ? null : _token.userName;
            }
        }

        /// <summary>
        /// Create an ApiAuthenicator object with a custom TokenStore implementation
        /// </summary>
        /// <param name="tokenStore"></param>
        public ApiAuthenticator(ITokenStore tokenStore)
        {
            _tokenStore = tokenStore;
        }

        /// <summary>
        /// This constructor assigns a Dummy Token Store object so that API Authentication tokens are not stored.
        /// </summary>
        public ApiAuthenticator()
        {
            _tokenStore = new DummyTokenStore();
        }

        /// <summary>
        /// All Api requests will use this static property to resolve and attempt authentication. If you use a custom ApiAuthenticator(ITokenStore) constructor, assign it to this property so the ApiProxy can utilize it.
        /// </summary>
        public static ApiAuthenticator Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApiAuthenticator(new IsolatedStorageTokenStore());
                }
                return _instance;
            }

            set
            {
                _instance = value;
            }
        }

        protected void IssueRefreshCallback(AuthToken authToken)
        {
            var obj = this; //javascript paranoid

            //Don't look at me, just want the quickest way to do this.
            Thread thread = new Thread(() =>
            {
                var sleepSpan = authToken.ExpirationDate.Subtract(DateTime.UtcNow);
                sleepSpan = sleepSpan.Subtract(TimeSpan.FromSeconds(authToken.RefreshBufferInSeconds));
                Thread.Sleep((int)sleepSpan.TotalMilliseconds);
                obj.Refresh(authToken, true);
            });
            thread.IsBackground = true;
            thread.Name = "Refresh Token : " + authToken.userName;
            thread.Start();
        }

        public ApiResponse Refresh(bool autoRefresh = false)
        {
            return Refresh(TokenStore.Find(UserName));
        }

        public ApiResponse Refresh(AuthToken storedToken, bool autoRefresh = false)
        {
            if (storedToken != null)
            {
                if (storedToken.IsValid)
                {
                    HttpWebRequest req = WebRequest.CreateHttp(Path.Combine(ApiInfo.BaseEndpoint, "oauth/token"));
                    req.Headers.Add("Voat-ApiKey", ApiInfo.ApiPublicKey);
                    req.ContentType = "application/x-www-form-urlencoded";
                    req.Method = "POST";

                    using (var content = new StreamWriter(req.GetRequestStream()))
                    {
                        //content.Write($"grant_type=refresh_token&refesh_token={storedToken.refresh_token}");
                        string payload = $"grant_type=refresh_token&client_id={ApiInfo.ApiPublicKey}&client_secret={ApiInfo.ApiPrivateKey}&refresh_token={storedToken.refresh_token}";
                        content.Write(payload);
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
                        return Helper.NoServerResponse;
                    }

                    string responseString = Helper.ReadStream(response.GetResponseStream());

                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        Console.WriteLine("Refresh succeeded");
                        _token = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthToken>(responseString);
                        TokenStore.Store(UserName, _token);
                        if (autoRefresh)
                        {
                            IssueRefreshCallback(_token);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Refresh failed");
                        OAuthErrorInfo error = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthErrorInfo>(responseString);
                        if (error != null)
                        {
                            var r = new ApiResponse();
                            r.Success = false;
                            r.Error = new ApiResponse.ErrorInfo() { Type = error.Error, Message = error.Description };
                            return r;
                        }
                    }
                    return new ApiResponse() { Success = true };
                }
            }
            return new ApiResponse() { Success = false, Error = new ApiResponse.ErrorInfo() { Type = "ExpiredToken", Message = "Token can not be refreshed" } };
        }

        public ApiResponse Login(string userName, string password, bool autoRefresh)
        {
            if (!ApiInfo.IsValid)
            {
                throw new InvalidProgramException("The ApiInfo object does not have valid data.");
            }

            //Check if we have a token
            AuthToken storedToken = TokenStore.Find(userName);
            if (storedToken != null)
            {
                if (storedToken.IsValid)
                {
                    _token = storedToken;
                    return new ApiResponse() { Success = true };
                }
                else
                {
                    TokenStore.Purge(userName);
                }
            }

            //Clear current ticket if present
            Logout();

            HttpWebRequest req = WebRequest.CreateHttp(Path.Combine(ApiInfo.BaseEndpoint, "oauth/token"));
            req.Headers.Add("Voat-ApiKey", ApiInfo.ApiPublicKey);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            using (var content = new StreamWriter(req.GetRequestStream()))
            {
                content.Write($"grant_type=password&username={userName}&password={password}&client_id={ApiInfo.ApiPublicKey}&client_secret={ApiInfo.ApiPrivateKey}");
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
                return Helper.NoServerResponse;
            }

            string responseString = Helper.ReadStream(response.GetResponseStream());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                _token = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthToken>(responseString);
                TokenStore.Store(userName, _token);
                if (autoRefresh)
                {
                    IssueRefreshCallback(_token);
                }
            }
            else
            {
                OAuthErrorInfo error = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthErrorInfo>(responseString);
                if (error != null)
                {
                    var r = new ApiResponse();
                    r.Success = false;
                    r.Error = new ApiResponse.ErrorInfo() { Type = error.Error, Message = error.Description };
                    return r;
                }
            }
            return new ApiResponse() { Success = true };
        }

        public void Logout()
        {
            _token = null;
        }

        public void Logout(string userName)
        {
            TokenStore.Purge(userName);
            Logout();
        }

        public void AuthenticateRequest(HttpWebRequest request)
        {
            if (IsAuthenticated)
            {
                request.Headers.Add("Authorization", String.Format("Bearer {0}", Token));
            }
        }

        public void SignRequest(HttpWebRequest request)
        {
            if (!String.IsNullOrEmpty(ApiInfo.ApiPrivateKey))
            {
                request.Headers.Add("Voat-HMAC", "TODO");
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return _token != null ? _token.IsValid : false;
            }
        }

        public string Token
        {
            get
            {
                return _token.access_token;
            }
        }
    }
}
