using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using b2c_dual_signin_api.Models;
using Microsoft.AspNetCore.Http;

namespace b2c_dual_signin_api.Services
{
    public class ClientCertificateAuthHelper
    {

        public static void CheckCertificate(HttpRequest Request, AppSettings appSettings)
        {
            X509Certificate2 clientCertInRequest = Request.HttpContext.Connection.ClientCertificate;

            if (clientCertInRequest == null)
            {
                throw new Exception("Client certificate not found.");
            }

            if (!string.IsNullOrEmpty(appSettings.ClientCertificateThumbprint))
            {
                if (clientCertInRequest.Thumbprint.Trim().ToLower() != appSettings.ClientCertificateThumbprint.ToLower())
                {
                    throw new Exception($"Client certificate '{clientCertInRequest.Thumbprint.Trim().ToLower()}' is invalid.");
                }
            }
        }
    }
}