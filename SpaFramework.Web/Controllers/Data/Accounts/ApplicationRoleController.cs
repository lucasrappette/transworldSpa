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
    public class ApplicationRoleController : EntityWriteController<ApplicationRole, IEntityWriteService<ApplicationRole, Guid>, Guid>
    {
        public ApplicationRoleController(IConfiguration configuration, IEntityWriteService<ApplicationRole, Guid> service) : base(configuration, service)
        {
        }
    }
}
