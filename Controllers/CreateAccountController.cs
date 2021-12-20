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
    public class CreateAccountController : ControllerBase
    {

        private readonly ILogger<CreateAccountController> _logger;
        private readonly AppSettings _appSettings;

        public CreateAccountController(ILogger<CreateAccountController> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }


        [HttpPost]
        public async Task<ActionResult> Post()
        {
            // Check client certificate
            try
            {
                ClientCertificateAuthHelper.CheckCertificate(this.Request, _appSettings);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel(ex.Message, HttpStatusCode.Conflict));
            }

            InputClaimsModel inputClaims;

            try
            {
                inputClaims = await InputClaimsHelper.GetClaims(this.Request, new string[] { "displayName", "userPrincipalName", "password" });
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel(ex.Message, HttpStatusCode.Conflict));
            }

            // Convert empty string phone number to null
            if (inputClaims.strongAuthenticationPhoneNumber == "") inputClaims.strongAuthenticationPhoneNumber = null;

            // Initialize the client credential auth provider
            var scopes = new[] { "https://graph.microsoft.com/.default" };
            var clientSecretCredential = new ClientSecretCredential(_appSettings.TenantId, _appSettings.ClientId, _appSettings.ClientSecret); 
            var graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            try
            {
                var user = new User
                {
                    AccountEnabled = true,
                    DisplayName = inputClaims.displayName,
                    MailNickname = "migrated-from-b2c",
                    UserPrincipalName = inputClaims.userPrincipalName,
                    PasswordPolicies = "DisablePasswordExpiration,DisableStrongPassword",
                    PasswordProfile = new PasswordProfile
                    {
                        ForceChangePasswordNextSignIn = false,
                        Password = inputClaims.password,
                    }
                };

                // Set the givenName
                if (!string.IsNullOrEmpty(inputClaims.givenName))
                    user.GivenName = inputClaims.givenName;

                // Set the surname
                if (!string.IsNullOrEmpty(inputClaims.surname))
                    user.GivenName = inputClaims.surname;

                // Create an account
                // For more information, see https://docs.microsoft.com/en-us/graph/api/user-post-users?view=graph-rest-1.0&tabs=http
                var newUser = await graphClient.Users
                .Request()
                .AddAsync(user);

                // Set the MFA phone number
                if (inputClaims.strongAuthenticationPhoneNumber != null)
                {
                    var phoneAuthenticationMethod = new PhoneAuthenticationMethod
                    {
                        PhoneNumber = inputClaims.strongAuthenticationPhoneNumber,
                        PhoneType = AuthenticationPhoneType.Mobile
                    };

                    await graphClient.Users[newUser.Id].Authentication.PhoneMethods
                        .Request()
                        .AddAsync(phoneAuthenticationMethod);
                }

                // Return the claims
                AADProfileOutputClaims outputClaims = new AADProfileOutputClaims()
                {
                    remoteAadObjectID = newUser.Id,
                    givenName = newUser.GivenName,
                    surName = newUser.Surname,
                    displayName = newUser.DisplayName,
                    AADAccountStatus = newUser.JobTitle,
                    strongAuthenticationPhoneNumber = inputClaims.strongAuthenticationPhoneNumber
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
