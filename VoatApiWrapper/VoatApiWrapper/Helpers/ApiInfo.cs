using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper {


    public static class ApiInfo {

        private static string _endpoint = null;

        
        public static string BaseEndpoint {
            get{
                return _endpoint;
            }
            set{
                if (value != null) { 
                    _endpoint = value + (!value.EndsWith("/") ? "/" : "");
                }
            }
        }
        public static string ApiPublicKey = null;
        public static string ApiPrivateKey = null;

        public static bool IsValid {
            get { return !String.IsNullOrEmpty(BaseEndpoint) && !String.IsNullOrEmpty(ApiPublicKey); }
        }

    }

}
