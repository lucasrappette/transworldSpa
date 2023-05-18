using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpaFramework.App.Models.Service.WorkItems.Echo;
using SpaFramework.App.Services.WorkItems;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers.WorkItems
{
    [Authorize]
    public class EchoWorkItemController : WorkItemController<EchoWorkItem>
    {
        public EchoWorkItemController(ISecureWorkItemService<EchoWorkItem> secureWorkItemService) : base(secureWorkItemService)
        {
        }
    }
}
