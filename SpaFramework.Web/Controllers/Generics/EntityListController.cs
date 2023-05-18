using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Services.Data;
using SpaFramework.App.Utilities.Serialization;
using SpaFramework.Core.Models;
using SpaFramework.Web.Filters.Support;
using SpaFramework.Web.Middleware.Exceptions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityListController<TDataModel, TService> : ControllerBase
        where TDataModel : class, IEntity
        where TService : IEntityListService<TDataModel>
    {
        protected readonly IConfiguration _configuration;
        protected readonly TService _readService;

        public EntityListController(IConfiguration configuration, TService readService)
        {
            _configuration = configuration;
            _readService = readService;
        }

        /// <summary>
        /// Returns all items
        /// </summary>
        /// <param name="offset">The first offset in the result set to return</param>
        /// <param name="limit">The maximum number of results to return</param>
        /// <param name="order">The order in which to sort results</param>
        /// <param name="filter">The filters to apply to the results</param>
        /// <param name="maxCount">The highest number that will be returned for X-Total-Count. By default, there's a limit applied and the full limit isn't returned; if you pass -1 you'll get the full number of results available in X-Total-Count</param>
        /// <returns></returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [SwaggerResponseHeader(StatusCodes.Status200OK, "X-Total-Count", "int", "Returns the total number of available items (not to exceed X-Total-Count-Max)")]
        [SwaggerResponseHeader(StatusCodes.Status200OK, "X-Total-Count-Max", "int", "Returns the highest total number that could be returned. If this equals X-Total-Count, then there are probably more results available than the number returned in X-Total-Count")]
        public async Task<ActionResult<IEnumerable<dynamic>>> GetAll(int offset = 0, int limit = 10, string order = null, string includes = null, string filter = null, int maxCount = -1, string context = null)
        {
            List<TDataModel> dataModelItems = await _readService.GetAll(HttpContext.User, offset, limit, order, includes, filter, true, null);

            List<dynamic> dtoModelItems = dataModelItems
                .Select(d => ConvertToDTO(d, includes, context))
                .ToList();

            if (maxCount == -1)
                maxCount = offset + limit + (limit * 3);

            long count = 0;

            // If we're bringing back fewer items than the limit, then we know we know these are the last items and don't need to do another expensive query to count how many others there are
            if (dtoModelItems.Count < offset + limit)
                count = offset + dtoModelItems.Count;
            else
                count = await _readService.GetAllCount(HttpContext.User, filter, maxCount);

            Response.Headers.Add("X-Total-Count", count.ToString());
            Response.Headers.Add("X-Total-Count-Max", maxCount.ToString());

            return Ok(dtoModelItems);
        }

        /// <summary>
        /// Converts a data model to a DTO
        /// </summary>
        /// <param name="dataModel"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        protected virtual dynamic ConvertToDTO(TDataModel dataModel, string includes = "", string context = null)
        {
            if (context == null)
                context = GetDefaultModelContext();

            return DataModelConverter.ConvertToDTO(GetModelContext(context), dataModel, new ConvertToDTOOptions(includes));
        }

        /// <summary>
        /// Returns the default model context. If different users have different default model contexts, override this to specify the correct context
        /// </summary>
        /// <returns></returns>
        protected virtual string GetDefaultModelContext()
        {
            return ModelContexts.WebApi;
        }

        /// <summary>
        /// Ensures the current user can use the specified model context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        protected virtual string GetModelContext(string context)
        {
            if (context == null)
                context = GetDefaultModelContext();

            return context;
        }
    }
}