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
    public class InfoController : ControllerBase
    {

        private readonly ILogger<InfoController> _logger;
        private readonly AppSettings _appSettings;

        public InfoController(ILogger<InfoController> logger, IOptions<AppSettings> appSettings)
        {
            _logger = logger;
            _appSettings = appSettings.Value;
        }


        [HttpGet]
        public  ActionResult Get()
        {
            return Ok(new {
                Tenant = _appSettings.TenantId
            });
        }

    }
}
