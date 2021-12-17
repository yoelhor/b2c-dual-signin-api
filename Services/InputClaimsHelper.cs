using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using b2c_dual_signin_api.Models;
using Microsoft.AspNetCore.Http;

namespace b2c_dual_signin_api.Services
{
    public class InputClaimsHelper
    {

        public static async Task<InputClaimsModel> GetClaims(HttpRequest Request, string[] RequiredClaims)
        {
            string input = null;

            // If not data came in, then return
            if (Request.Body == null)
            {
                throw new Exception("Request content is null");
            }

            // Read the input claims from the request body
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                input = await reader.ReadToEndAsync();
            }

            // Check the input content value
            if (string.IsNullOrEmpty(input))
            {
                throw new Exception("Request content is empty");
            }

            // Convert the input string into InputClaimsModel object
            InputClaimsModel inputClaims = InputClaimsModel.Parse(input);

            if (inputClaims == null)
            {
                throw new Exception("Can not deserialize input claims");
            }


            foreach (PropertyInfo property in inputClaims.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object val = property.GetValue(inputClaims, null);


                // Check if the password parameter is presented
                if (RequiredClaims.Contains(property.Name) && (val is null || val.ToString().Length == 0))
                {
                    throw new Exception($"Error: the required `{property.Name}` attribute is null or empty!");
                }
            }

            return inputClaims;
        }
    }
}