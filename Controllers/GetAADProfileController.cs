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
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Identity.Client;

namespace b2c_dual_signin_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetAADProfileController : ControllerBase
    {

        private readonly ILogger<GetAADProfileController> _logger;
        private readonly AppSettings _appSettings;

        public GetAADProfileController(ILogger<GetAADProfileController> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }


        [HttpPost]
        public async Task<ActionResult> Post()
        {
            InputClaimsModel inputClaims;

            try
            {
                inputClaims = await InputClaimsHelper.GetClaims(this.Request, new string[] { "signInName" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel(ex.Message, HttpStatusCode.Conflict));
            }

            // Initialize the client credential auth provider
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(_appSettings.TenantId, _appSettings.ClientId, _appSettings.ClientSecret);
            
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            try
            {
                // Get the Azure AD user by UPN (the sign-in name)
                // For more information, see https://docs.microsoft.com/en-us/graph/api/user-get?view=graph-rest-1.0&tabs=csharp
                User AadUser = null;

                try
                {
                    AadUser = await graphClient.Users[inputClaims.signInName]
                    .Request()
                    .GetAsync();
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    // If user not found return without error
                    if (ex.StatusCode == HttpStatusCode.NotFound)
                    {
                        return Ok();
                    }
                }

                // Get the Azure AD user's MFA phone number
                // For more information, see https://docs.microsoft.com/en-us/graph/api/authentication-list-phonemethods?view=graph-rest-beta&tabs=csharp
                var phoneMethods = await graphClient.Users[inputClaims.signInName].Authentication.PhoneMethods
                .Request()
                .GetAsync();

                // Get the first phone number from the list
                string phoneNumber = null;
                if (phoneMethods != null && phoneMethods.Count >= 1)
                {
                    phoneNumber = phoneMethods[0].PhoneNumber;
                }

                // Return the claims
                AADProfileOutputClaims outputClaims = new AADProfileOutputClaims()
                {
                    remoteAadObjectID = AadUser.Id,
                    givenName = AadUser.GivenName,
                    surName = AadUser.Surname,
                    displayName = AadUser.DisplayName,
                    AADAccountStatus = AadUser.JobTitle,
                    strongAuthenticationPhoneNumber = phoneNumber
                };

                return Ok(outputClaims);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }


    }
}
