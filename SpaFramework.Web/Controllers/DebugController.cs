using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Notifications;
using SpaFramework.Web.Hubs;
using System;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    // Ideally, we'd specify the "SuperAdmin" role in the Authorize attribute -- but for whatever reason, that doesn't seem to work in this controller
    [Authorize]
    public class DebugController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubContext<NotificationHub, INotificationClient> _notificationHubContext;

        public DebugController(IConfiguration configuration, UserManager<ApplicationUser> userManager, IHubContext<NotificationHub, INotificationClient> notificationHubContext)
        {
            _configuration = configuration;
            _userManager = userManager;
            _notificationHubContext = notificationHubContext;
        }

        private async Task PreCheck()
        {
            if (!_configuration.GetValue<bool>("AllowDebugController", false))
                throw new Exception("Debug Controller is disabled");
        }

        /// <summary>
        /// Sends an alert to all Clients
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SendGlobalAlert(AlertDTO alert)
        {
            await PreCheck();

            await _notificationHubContext.Clients.All.OnAlert(alert);

            return Ok();
        }

        /// <summary>
        /// Sends an alert to a single Client
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> SendAlert(AlertDTO alert, string userId)
        {
            await PreCheck();

            await _notificationHubContext.Clients.User(userId).OnAlert(alert);

            return Ok();
        }

    }
}
