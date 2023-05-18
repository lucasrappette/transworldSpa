using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SpaFramework.App.Models.Service.WorkItems;
using SpaFramework.App.Services.WorkItems;
using SpaFramework.Web.Middleware.Exceptions;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers.WorkItems
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class WorkItemController<TWorkItem> : ControllerBase
        where TWorkItem : class, IWorkItem
    {
        private readonly ISecureWorkItemService<TWorkItem> _secureWorkItemService;

        public WorkItemController(ISecureWorkItemService<TWorkItem> secureWorkItemService)
        {
            _secureWorkItemService = secureWorkItemService;
        }

        /// <summary>
        /// Enqueues a new work item
        /// </summary>
        /// <param name="workItem">The work item to create</param>
        /// <returns></returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to create item</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create(TWorkItem workItem)
        {
            await _secureWorkItemService.EnqueueWorkItem(HttpContext.User, workItem, null, null, null);

            return Ok();
        }
    }
}
