using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {


    public abstract class BaseApiProxy {
       
        protected ApiResponse Request(HttpMethod method, string endpoint, object body) {

            if (!ApiInfo.IsValid) {
                throw new InvalidOperationException("ApiInfo object has invalid state.");
            }

            HttpWebRequest req = WebRequest.CreateHttp(Path.Combine(ApiInfo.BaseEndpoint, endpoint));
            req.Method = method.Method;
            req.ContentType = "application/json";
            req.Headers.Add("Voat-ApiKey", ApiInfo.ApiPublicKey);
            
            if (ApiAuthenticator.Instance.IsAuthenticated) {
                req.Headers.Add("Authorization", String.Format("Bearer {0}", ApiAuthenticator.Instance.Token));
            }

            if (method != HttpMethod.Get && body != null) {
                using (var sw = new StreamWriter(req.GetRequestStream())) {
                    sw.Write(Newtonsoft.Json.JsonConvert.SerializeObject(body));
                }
            }

            HttpWebResponse resp = null;
            try {
                resp = (HttpWebResponse)req.GetResponse();
            } catch (WebException ex) {
                resp = (HttpWebResponse)ex.Response;
            }

            var response = Newtonsoft.Json.JsonConvert.DeserializeObject<ApiResponse>(Helper.ReadStream(resp.GetResponseStream()));
            response.StatusCode = resp.StatusCode;

            return response;

        }

    }
}
