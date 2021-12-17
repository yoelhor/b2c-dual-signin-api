using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Azure.Identity;
using b2c_dual_signin_api.Models;
using b2c_dual_signin_api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace b2c_dual_signin_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UpdateAADPasswordController : ControllerBase
    {

        private readonly ILogger<UpdateAADPasswordController> _logger;

        public UpdateAADPasswordController(ILogger<UpdateAADPasswordController> logger)
        {
            _logger = logger;
        }


        [HttpPost]
        public async Task<ActionResult> Post()
        {
            InputClaimsModel inputClaims;

            try
            {
                inputClaims = await InputClaimsHelper.GetClaims(this.Request, new string[] { "objectId", "password" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel(ex.Message, HttpStatusCode.Conflict));
            }

            // Initialize the client credential auth provider
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(AppSettings.AAD.TenantId, AppSettings.AAD.ClientId, AppSettings.AAD.ClientSecret);
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var user = new User
            {
                PasswordPolicies = "DisablePasswordExpiration,DisableStrongPassword",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = inputClaims.password,
                }
            };

            try
            {
                // Update user by object ID
                await graphClient.Users[inputClaims.objectId]
                   .Request()
                   .UpdateAsync(user);

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }

        
    }
}
