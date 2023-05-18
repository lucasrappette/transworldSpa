using AutoMapper;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.DTO.Accounts;
using SpaFramework.App;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SpaFramework.App.Services.Data;

namespace SpaFramework.Web.Controllers.Data.Accounts
{
    public class ApplicationUserController : EntityWriteController<ApplicationUser, IEntityWriteService<ApplicationUser, Guid>, Guid>
    {
        public ApplicationUserController(IConfiguration configuration, IEntityWriteService<ApplicationUser, Guid> service) : base(configuration, service)
        {
        }
    }
}
