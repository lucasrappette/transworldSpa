using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Services.Data;
using SpaFramework.App.Utilities.Serialization;
using SpaFramework.Web.Controllers;
using SpaFramework.Web.Middleware.Exceptions;
using System.Dynamic;
using System.Threading.Tasks;

namespace SpaFramework.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public abstract class EntityWriteController<TDataModel, TService, TIdType> : EntityReadController<TDataModel, TService, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, new()
        where TService : IEntityReadService<TDataModel, TIdType>, IEntityWriteService<TDataModel, TIdType>
    {
        protected readonly TService _writeService;

        public EntityWriteController(IConfiguration configuration, TService service) : base(configuration, service)
        {
            _writeService = service;
        }

        /// <summary>
        /// Creates a new item
        /// </summary>
        /// <param name="dtoModel">The item to create</param>
        /// <returns></returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to create item</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Create([FromBody] ExpandoObject dtoModel, string context = null)
        {
            TDataModel dataModel = ConvertToDataModel(dtoModel, context);

            dataModel = await _writeService.Create(HttpContext.User, dataModel);
            object returnValue = ConvertToDTO(dataModel, "", context);

            return CreatedAtAction(nameof(GetOne), new { id = dataModel.GetId() }, returnValue);
        }

        /// <summary>
        /// Updates an item
        /// </summary>
        /// <param name="id">The ID of the item to update</param>
        /// <param name="dtoModel">The new value for the item</param>
        /// <returns>Returns the item (with an updated timestamp)</returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to update item</response>
        /// <response code="409">Concurrency conflict. The item sent in the request is no longer the most recent version of the item</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Update(TIdType id, [FromBody] ExpandoObject dtoModel, string context = null)
        {
            id = GetOneId(id);

            TDataModel dataModel = ConvertToDataModel(dtoModel, context);

            dataModel = await _writeService.Update(HttpContext.User, dataModel);

            object returnValue = ConvertToDTO(dataModel, "", context);

            return Ok(returnValue);
        }

        /// <summary>
        /// Gets a new, empty item
        /// </summary>
        /// <returns></returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to create item</response>
        [HttpGet("new")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetNew(string context = null)
        {
            TDataModel dataModel = await _writeService.GetNew(HttpContext.User, null);

            object returnValue = DataModelConverter.ConvertToDTO(context, dataModel, new ConvertToDTOOptions(explicitExcludes: GetNewExplicitExcludes));

            return Ok(returnValue);
        }

        /// <summary>
        /// Gets a new, empty item
        /// </summary>
        /// <returns></returns>
        /// <response code="400">Item validation failed</response>
        /// <response code="403">User does not have permission to create item</response>
        [HttpPost("new")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetNew([FromBody] ExpandoObject dtoModel, string context = null)
        {
            TDataModel templateDataModel = ConvertToDataModel(dtoModel, context);
            TDataModel dataModel = await _writeService.GetNew(HttpContext.User, templateDataModel);

            object returnValue = DataModelConverter.ConvertToDTO(context, dataModel, new ConvertToDTOOptions(explicitExcludes: GetNewExplicitExcludes));

            return Ok(returnValue);
        }

        ///// <summary>
        ///// Partially updates an item using JSON patch
        ///// </summary>
        ///// <param name="id">The ID of the item to update</param>
        ///// <param name="patchDocument">The JSON patch operations for the item</param>
        ///// <returns>Returns the item (with an updated timestamp)</returns>
        ///// <response code="400">Item validation failed</response>
        ///// <response code="403">User does not have permission to update item</response>
        ///// <response code="404">Item does not exist</response>
        ///// <response code="409">Concurrency conflict. The item sent in the request is no longer the most recent version of the item</response>
        //[HttpPatch("{id}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ValidationErrorDetails), StatusCodes.Status400BadRequest)]
        //[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        //[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status404NotFound)]
        //[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status409Conflict)]
        //[ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status500InternalServerError)]
        //public async Task<ActionResult> Patch(TIdType id, [FromBody] JsonPatchDocument<TDTOModel> patchDocument)
        //{
        //    id = GetOneId(id);

        //    TDataModel originalDataModel = await _readService.GetOne(HttpContext.User, id, null, true);

        //    if (originalDataModel == null)
        //        return NotFound();

        //    TDTOModel dtoModel = ConvertToDTO(originalDataModel);
        //    patchDocument.ApplyTo(dtoModel);
        //    TDataModel patchedDataModel = ConvertToDataModel(dtoModel);

        //    await _writeService.Update(HttpContext.User, patchedDataModel);

        //    TDTOModel returnValue = ConvertToDTO(patchedDataModel);

        //    return Ok(returnValue);
        //}

        /// <summary>
        /// Converts a DTO to a data model
        /// </summary>
        /// <param name="dtoModel"></param>
        /// <returns></returns>
        protected virtual dynamic ConvertToDataModel(ExpandoObject dtoModel, string context = null)
        {
            if (context == null)
                context = GetDefaultModelContext();

            return DataModelConverter.ConvertToDataModel<TDataModel>(GetModelContext(context), dtoModel);
        }

        protected virtual string GetNewExplicitExcludes { get { return "id,concurrencyCheck,lastModification"; } }
    }
}
