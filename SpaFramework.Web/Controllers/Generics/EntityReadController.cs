using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Services.Data;
using SpaFramework.Web.Middleware.Exceptions;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityReadController<TDataModel, TService, TIdType> : EntityListController<TDataModel, TService>
        where TDataModel : class, IEntity, IHasId<TIdType>
        where TService : IEntityReadService<TDataModel, TIdType>
    {
        public EntityReadController(IConfiguration configuration, TService readService) 
            : base(configuration, readService)
        {
        }

        /// <summary>
        /// Returns a single item
        /// </summary>
        /// <param name="id">The ID of the item to return</param>
        /// <returns></returns>
        [HttpGet("{*id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetOne(TIdType id, string includes = null, string context = null)
        {
            id = GetOneId(id);

            TDataModel dataModel = await _readService.GetOne(HttpContext.User, id, includes);

            if (dataModel == null)
                return NotFound();

            dynamic dtoModel = ConvertToDTO(dataModel, includes, context);

            return Ok(dtoModel);
        }

        protected virtual TIdType GetOneId(TIdType id) { return id; }
    }
}