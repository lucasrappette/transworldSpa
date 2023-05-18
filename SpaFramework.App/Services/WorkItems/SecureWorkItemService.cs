using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Utilities;
using SpaFramework.App.Models;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Service.WorkItems;
using SpaFramework.App.Services.WorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.Core.Models;

namespace SpaFramework.App.Services.WorkItems
{
    public class SecureWorkItemService<TWorkItem> : ISecureWorkItemService<TWorkItem>
        where TWorkItem : IWorkItem
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly IWorkItemService<TWorkItem> _workItemService;
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<WorkItemService<TWorkItem>> _logger;

        public SecureWorkItemService(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager, IWorkItemService<TWorkItem> workItemService, IConfiguration configuration, ILogger<WorkItemService<TWorkItem>> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _workItemService = workItemService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Enqueues a single work item
        /// </summary>
        /// <param name="user"></param>
        /// <param name="workItem"></param>
        /// <param name="initialDelaySeconds"></param>
        /// <param name="requeueDelaySeconds"></param>
        /// <param name="timeToLive"></param>
        /// <returns></returns>
        public async Task EnqueueWorkItem(ClaimsPrincipal user, TWorkItem workItem, int? initialDelaySeconds = null, int? requeueDelaySeconds = null, TimeSpan? timeToLive = null)
        {
            var applicationUser = await GetApplicationUser(user);

            if (!(await CanEnqueue(applicationUser, workItem)))
                throw new ForbiddenException();

            await _workItemService.EnqueueWorkItem(workItem, initialDelaySeconds, requeueDelaySeconds, timeToLive);

            _logger.LogInformation("WorkItem {WorkItemType} {EntityAction} by {UserName} ({UserId})", typeof(TWorkItem).Name, "Enqueued", user.GetUserName(), user.GetUserId());
        }

        /// <summary>
        /// Returns whether or not a user is allowed to enqueue the work item. This is designed to be overridden in derived classes. By default, only SiteAdmins can enqueue work items.
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="workItem"></param>
        /// <returns></returns>
        protected virtual async Task<bool> CanEnqueue(ApplicationUser applicationUser, TWorkItem workItem)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }

        /// <summary>
        /// Returns an ApplicationUser, with necessary security attributes, for a ClaimsPrincipal
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        protected async Task<ApplicationUser> GetApplicationUser(ClaimsPrincipal user)
        {
            if (user == null)
                return null;

            string userIdString = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userIdString))
                return null;

            Guid userId;
            if (!Guid.TryParse(userIdString, out userId))
                return null;

            return await _dbContext.ApplicationUsers
                .Where(au => au.Id == userId)
                .SingleOrDefaultAsync();
        }
    }
}
