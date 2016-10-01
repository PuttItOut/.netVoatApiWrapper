using System;
using System.IO;
using System.Net;
using System.Threading;
using VoatApiWrapper.Models;

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
                    _tokenStore = new DisabledTokenStore();
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
            _tokenStore = new DisabledTokenStore();
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


        /// <summary>
        /// Will attempt to load token from instance token store. Returns true if token is found and valid, else false.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool TryLogin(string userName)
        {
            AuthToken storedToken = TokenStore.Find(userName);
            if (storedToken != null)
            {
                if (storedToken.IsValid)
                {
                    _token = storedToken;
                    return true;
                }
                else
                {
                    TokenStore.Purge(userName);
                }
            }
            return false;
        }
        private ApiResponse IssueTokenRequest(string userName, string body, bool autoRefresh = false)
        {
            HttpWebRequest req = WebRequest.CreateHttp(Path.Combine(ApiInfo.Endpoint, "oauth/token"));
            req.Headers.Add("Voat-ApiKey", ApiInfo.PublicKey);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            using (var content = new StreamWriter(req.GetRequestStream()))
            {
                content.Write(body);
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
                return (ApiResponse)Helper.NoServerResponse<object>();
            }

            string responseString = Helper.ReadStream(response.GetResponseStream());

            if (response.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine($"Auth for {userName} OK");
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
        public ApiResponse Login(string userName, string password, bool autoRefresh)
        {
            if (!ApiInfo.IsValid)
            {
                throw new InvalidProgramException("The ApiInfo object does not have valid data.");
            }

            if (TryLogin(userName))
            {
                return new ApiResponse() { Success = true };
            }

            //Clear current ticket if present
            Logout();

            return IssueTokenRequest(
                userName,
                $"grant_type=password&username={userName}&password={password}&client_id={ApiInfo.PublicKey}&client_secret={ApiInfo.PrivateKey}", 
                autoRefresh);
        }

        public ApiResponse Refresh(AuthToken storedToken, bool autoRefresh = false)
        {
            if (storedToken != null)
            {
                if (storedToken.IsValid)
                {
                    return IssueTokenRequest(
                        storedToken.userName,
                        $"grant_type=refresh_token&client_id={ApiInfo.PublicKey}&client_secret={ApiInfo.PrivateKey}&refresh_token={storedToken.refresh_token}",
                        autoRefresh);
                }
            }
            return new ApiResponse() { Success = false, Error = new ApiResponse.ErrorInfo() { Type = "ExpiredToken", Message = "Token can not be refreshed" } };
        }
        /// <summary>
        /// Clears token from instance of ApiAuthenticator but leaves token in store
        /// </summary>
        public void Logout()
        {
            _token = null;
        }
        /// <summary>
        /// Purges Token in store and clears token from instance of ApiAuthenticator
        /// </summary>
        /// <param name="userName"></param>
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
            if (!String.IsNullOrEmpty(ApiInfo.PrivateKey))
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
