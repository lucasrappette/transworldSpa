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
using SpaFramework.App.Models.Data.Dealers;

namespace SpaFramework.Web.Controllers.Data.Dealers
{
    [Authorize]
    public class DealerController : EntityWriteController<Dealer, IEntityWriteService<Dealer, Guid>, Guid>
    {
        public DealerController(IConfiguration configuration, IEntityWriteService<Dealer, Guid> service) : base(configuration, service)
        {
        }
    }
}
