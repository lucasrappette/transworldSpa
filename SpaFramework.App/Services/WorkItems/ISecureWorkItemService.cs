using SpaFramework.App.Models.Service.WorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.WorkItems
{
    /// <summary>
    /// This is a public-facing version of IWorkItemService that authenticates a user before performing operations
    /// </summary>
    public interface ISecureWorkItemService<TWorkItem>
        where TWorkItem : IWorkItem
    {
        Task EnqueueWorkItem(ClaimsPrincipal user, TWorkItem workItem, int? initialDelaySeconds = null, int? requeueDelaySeconds = null, TimeSpan? timeToLive = null);
    }
}
