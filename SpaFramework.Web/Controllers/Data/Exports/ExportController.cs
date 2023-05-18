using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Donors;
using SpaFramework.App;
using SpaFramework.App.Services.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Exports;

namespace SpaFramework.Web.Controllers.Data.Exports
{
    [Authorize]
    public class ExportController : EntityWriteController<Export, IEntityWriteService<Export, int>, int>
    {
        public ExportController(IConfiguration configuration, IEntityWriteService<Export, int> service) : base(configuration, service)
        {
        }
    }
}
