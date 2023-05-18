using SpaFramework.App.Models.Data.Accounts;
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
    /// This doesn't perform authentication and is only designed to be used internally
    /// </summary>
    /// <typeparam name="TWorkItem"></typeparam>
    public interface IWorkItemService<TWorkItem>
        where TWorkItem : IWorkItem
    {
        Task EnqueueWorkItem(TWorkItem workItem, int? initialDelaySeconds = null, int? requeueDelaySeconds = null, TimeSpan? timeToLive = null);
        Task EnqueueWorkItem(WorkItemMessage<TWorkItem> workItemMessage);
    }
}
