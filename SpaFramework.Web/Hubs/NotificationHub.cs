using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Web.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        private readonly ILogger<NotificationHub> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(ILogger<NotificationHub> logger, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("User {UserId} connected as {ConnectionId}", Context.UserIdentifier, Context.ConnectionId);

            await base.OnConnectedAsync();
        }
    }
}
