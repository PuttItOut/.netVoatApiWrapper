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

        private OAuthTicket _ticket = null;

        private ApiAuthenticator() { 
            /*no-op*/
        }
        public static ApiAuthenticator Instance {
            get {
                if (_instance == null) {
                    _instance = new ApiAuthenticator();
                }
                return _instance;
            }
        }
        
        public ApiResponse Login(string userName, string password) {

            if (!ApiInfo.IsValid) {
                throw new InvalidProgramException("The ApiInfo object does not have valid data.");
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


            using (var responseStream = new StreamReader(response.GetResponseStream())) {
                if (response.StatusCode == HttpStatusCode.OK) {
                    _ticket = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(responseStream.ReadToEnd(), _ticket);
                } else {
                    
                    OAuthErrorInfo error = Newtonsoft.Json.JsonConvert.DeserializeObject<OAuthErrorInfo>(responseStream.ReadToEnd());
                    if (error != null) {
                        var r = new ApiResponse();
                        r.Success = false;
                        r.Error = new ApiResponse.ErrorInfo() { Type = error.Error, Message = error.Description };
                        return r;
                    }
                }
            
            }
            return new ApiResponse() { Success = true };
        }
        
        public void Logout() {
            _ticket = null;
        }

        public bool IsAuthenticated {
            get {
                return _ticket != null && DateTime.Now.AddTicks(_ticket.expires_in) > DateTime.Now;
            }
        }
        public string Token {
            get {
                return _ticket.access_token;
            }
        }

        private class OAuthTicket { 
            public string access_token {get; set;} 
            public string token_type  {get; set;} 
            public string userName  {get; set;} 
            public int expires_in  {get; set;}        
        }

    }
}
