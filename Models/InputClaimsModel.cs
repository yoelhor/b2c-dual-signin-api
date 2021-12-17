using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace b2c_dual_signin_api.Models
{
    public class InputClaimsModel
    {
        // Demo: User's object id in Azure AD B2C
        public string email { get; set; }
        public string objectId { get; set; }
        public string password { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

        public static InputClaimsModel Parse(string JSON)
        {
            return JsonSerializer.Deserialize(JSON, typeof(InputClaimsModel)) as InputClaimsModel;
        }
    }
}