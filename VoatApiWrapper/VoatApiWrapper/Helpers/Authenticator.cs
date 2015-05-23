using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {


    public class ApiAuthenticator {

        private static ApiAuthenticator _instance;

        private AuthToken _token = null;

        private ITokenStore _tokenStore = null;

        private ITokenStore TokenStore {
            get {
                if (_tokenStore == null) {
                    _tokenStore = new DummyTokenStore();
                }
                return _tokenStore; 
            }
        }

        /// <summary>
        /// Create an ApiAuthenicator object with a custom TokenStore implementation
        /// </summary>
        /// <param name="tokenStore"></param>
        public ApiAuthenticator(ITokenStore tokenStore) {
            _tokenStore = tokenStore;
        }

        /// <summary>
        /// This constructor assigns a Dummy Token Store object so that API Authentication tokens are not stored.
        /// </summary>
        public ApiAuthenticator() {
            _tokenStore = new DummyTokenStore();
        }

        /// <summary>
        /// All Api requests will use this static property to resolve and attempt authentication. If you use a custom ApiAuthenticator(ITokenStore) constructor, assign it to this property so the ApiProxy can utilize it.
        /// </summary>
        public static ApiAuthenticator Instance {
            get {
                if (_instance == null) {
                    _instance = new ApiAuthenticator(new IsolatedStorageTokenStore());
                }
                return _instance;
            }
            set {
                _instance = value;
            }
        }
        
        public ApiResponse Login(string userName, string password) {

            if (!ApiInfo.IsValid) {
                throw new InvalidProgramException("The ApiInfo object does not have valid data.");
            }

            //Check if we have a token
            AuthToken storedToken = TokenStore.Find(userName);
            if (storedToken != null){
                if (storedToken.IsValid) {
                    _token = storedToken;
                    return new ApiResponse() { Success = true };
                } else {
                    TokenStore.Purge(userName);
                }
            }

            //Clear current ticket if present
            Logout();

            HttpWebRequest req = WebRequest.CreateHttp(Path.Combine(ApiInfo.BaseEndpoint, "api/token"));
            req.Headers.Add("Voat-ApiKey", ApiInfo.ApiPublicKey);
            req.ContentType = "application/x-www-form-urlencoded";
            req.Method = "POST";

            using (var content = new StreamWriter(req.GetRequestStream())) {
                content.Write(String.Format("grant_type=password&username={0}&password={1}", userName, password));
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

            if (response.StatusCode == HttpStatusCode.OK) {
                _token = Newtonsoft.Json.JsonConvert.DeserializeObject<AuthToken>(responseString);
                TokenStore.Store(userName, _token);
            } else {
                OAuthErrorInfo error = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthErrorInfo>(responseString);
                if (error != null) {
                    var r = new ApiResponse();
                    r.Success = false;
                    r.Error = new ApiResponse.ErrorInfo() { Type = error.Error, Message = error.Description };
                    return r;
                }
            }
            return new ApiResponse() { Success = true };
        }
        
        public void Logout() {
            _token = null;
        }

        public void AuthenticateRequest(HttpWebRequest request) {
            if (IsAuthenticated) {
                request.Headers.Add("Authorization", String.Format("Bearer {0}", Token));
            }
        }

        public bool IsAuthenticated {
            get {
                return _token.IsValid;
            }
        }
        public string Token {
            get {
                return _token.access_token;
            }
        }


    }


    [Serializable]
    public class AuthToken {

        private DateTime _issueDate = DateTime.UtcNow;
        
        //Native token fields
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string userName { get; set; }
        public int expires_in { get; set; }
        

        [JsonProperty("issue_date")]
        public DateTime IssueDate { get { return _issueDate; } set { _issueDate = value; } }
        
        [JsonIgnore]
        public bool IsExpired {
            get { return _issueDate.AddSeconds(expires_in) <= DateTime.UtcNow; }
        }

        [JsonIgnore]
        public bool IsValid {
            get { return (!String.IsNullOrEmpty(access_token) && !IsExpired); }
        }

    }
}
