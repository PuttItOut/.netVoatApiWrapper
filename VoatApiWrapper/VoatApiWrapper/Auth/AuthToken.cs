using Newtonsoft.Json;
using System;

namespace VoatApiWrapper
{
    [Serializable]
    public class AuthToken
    {
        private DateTime _issueDate = DateTime.UtcNow;

        public int RefreshBufferInSeconds { get; set; } = 90;

        //Native token fields
        public string access_token { get; set; }

        public string refresh_token { get; set; }

        public string token_type { get; set; }

        public string userName { get; set; }

        public int expires_in { get; set; }

        public DateTime IssueDate { get { return _issueDate; } set { _issueDate = value; } }

        [JsonIgnore()]
        public DateTime ExpirationDate { get { return _issueDate.AddSeconds(expires_in - 5); } }

        [JsonIgnore]
        public bool IsExpired
        {
            get { return ExpirationDate <= DateTime.UtcNow; }
        }

        [JsonIgnore]
        public bool IsValid
        {
            get { return (!String.IsNullOrEmpty(access_token) && !IsExpired); }
        }
    }
}
