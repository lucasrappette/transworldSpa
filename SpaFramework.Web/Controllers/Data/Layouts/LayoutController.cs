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
using SpaFramework.App.Models.Data.Layouts;

namespace SpaFramework.Web.Controllers.Data.Layouts
{
    [Authorize]
    public class LayoutController : EntityWriteController<Layout, IEntityWriteService<Layout, int>, int>
    {
        public LayoutController(IConfiguration configuration, IEntityWriteService<Layout, int> service) : base(configuration, service)
        {
        }
    }
}
