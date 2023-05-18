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
using SpaFramework.App.Models.Data.Destinations;

namespace SpaFramework.Web.Controllers.Data.Destinations
{
    [Authorize]
    public class DestinationController : EntityWriteController<Destination, IEntityWriteService<Destination, int>, int>
    {
        public DestinationController(IConfiguration configuration, IEntityWriteService<Destination, int> service) : base(configuration, service)
        {
        }
    }
}
