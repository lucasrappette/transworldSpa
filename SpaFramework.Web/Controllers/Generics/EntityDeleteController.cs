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
    public abstract class EntityDeleteController<TDataModel, TService, TIdType> : EntityWriteController<TDataModel, TService, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, new()
        where TService : IEntityDeleteService<TDataModel, TIdType>
    {
        public EntityDeleteController(IConfiguration configuration, TService service) : base(configuration, service)
        {
        }

        /// <summary>
        /// Deletes an item
        /// </summary>
        /// <param name="id">The ID of the item to delete</param>
        /// <returns></returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Delete(TIdType id)
        {
            id = GetOneId(id);

            await _writeService.Delete(HttpContext.User, id);

            return NoContent();
        }
    }
}
