using System;
using System.Text.Json.Serialization;

namespace b2c_dual_signin_api.Models
{
    public class AADProfileOutputClaims
    {
        public string remoteAadObjectID { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string givenName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string surName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string displayName { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string AADAccountStatus { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string strongAuthenticationPhoneNumber { get; set; }
    }
}