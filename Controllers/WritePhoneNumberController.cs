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
    public class WritePhoneNumberController : ControllerBase
    {

        private readonly ILogger<WritePhoneNumberController> _logger;
        private readonly AppSettings _appSettings;

        public WritePhoneNumberController(ILogger<WritePhoneNumberController> logger, IOptions<AppSettings> appSettings)
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
                inputClaims = await InputClaimsHelper.GetClaims(this.Request, new string[] { "strongAuthenticationPhoneNumber", "objectId" });
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
                try
                {
                    var AadUser = await graphClient.Users[inputClaims.objectId]
                   .Request()
                   .GetAsync();
                }
                catch (Microsoft.Graph.ServiceException ex)
                {
                    return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel("User not found", HttpStatusCode.Conflict));

                }


                var phoneAuthenticationMethod = new PhoneAuthenticationMethod
                {
                    PhoneNumber = inputClaims.strongAuthenticationPhoneNumber,
                    PhoneType = AuthenticationPhoneType.Mobile
                };

                await graphClient.Users[inputClaims.objectId].Authentication.PhoneMethods
                    .Request()
                    .AddAsync(phoneAuthenticationMethod);


                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.Conflict, new B2CResponseModel($"General error (REST API): {ex.Message}", HttpStatusCode.Conflict));
            }
        }


    }
}
