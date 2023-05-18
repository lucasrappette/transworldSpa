using FluentValidation;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaFramework.App.Models.Data.Donors;
using SpaFramework.App.Models;
using System.Collections.Generic;
using SpaFramework.Core.Models;
using SpaFramework.App.Services.WorkItems;
using SpaFramework.App.Models.Data.Dealers;

namespace SpaFramework.App.Services.Data.Dealers
{
    public class DealerService : EntityWriteService<Dealer, Guid>
    {
        public DealerService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<Dealer> validator, ILogger<DealerService> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        protected override async Task<IQueryable<Dealer>> ApplyIdFilter(IQueryable<Dealer> queryable, Guid id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override List<string> ReadRoles => new List<string> { ApplicationRoleNames.SuperAdmin, ApplicationRoleNames.ProjectManager, ApplicationRoleNames.ProjectViewer };
        protected override List<string> WriteRoles => new List<string> { ApplicationRoleNames.SuperAdmin, ApplicationRoleNames.ProjectManager };
    }
}
