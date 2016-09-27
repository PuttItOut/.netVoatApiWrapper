using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoatApiWrapper.Models
{

    public class OAuthErrorInfo
    {
        public OAuthErrorInfo() : base()
        {
        }

        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string Description { get; set; }
    }
}
