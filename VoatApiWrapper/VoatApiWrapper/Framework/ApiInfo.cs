using System;

namespace VoatApiWrapper
{
    public static class ApiInfo
    {
        private static string _endpoint = null;

        public static string Endpoint
        {
            get
            {
                return _endpoint;
            }

            set
            {
                if (value != null)
                {
                    _endpoint = value + (!value.EndsWith("/") ? "/" : "");
                }
            }
        }

        public static string PublicKey = null;
        public static string PrivateKey = null;

        public static bool IsValid
        {
            get { return !String.IsNullOrEmpty(Endpoint) && !String.IsNullOrEmpty(PublicKey); }
        }
    }
}
