using AutoMapper;
using SpaFramework.Web.Middleware.Exceptions;
using SpaFramework.App.Models.Data.Content;
using SpaFramework.App.Models.Service.Content;
using SpaFramework.App;
using SpaFramework.App.Services.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Services.Data.Content;
using SpaFramework.App.Utilities.Serialization;
using SpaFramework.Core.Models;

namespace SpaFramework.Web.Controllers.Data.Content
{
    [Authorize]
    public class ContentBlockController : EntityWriteController<ContentBlock, ContentBlockService, Guid>
    {
        public ContentBlockController(IConfiguration configuration, ContentBlockService service) : base(configuration, service)
        {
        }

        /// <summary>
        /// Gets a content block by its slug
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [AllowAnonymous]
        [Route("slug/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorDetails), StatusCodes.Status403Forbidden)]
        public async Task<ActionResult> GetBySlug(string slug)
        {
            var contentData = await _writeService.GetContentData(slug, new Dictionary<string, string>(), false);
            var returnValue = DataModelConverter.ConvertToDTO(ModelContexts.WebApi, contentData, new ConvertToDTOOptions());

            return Ok(returnValue);
        }
    }
}
