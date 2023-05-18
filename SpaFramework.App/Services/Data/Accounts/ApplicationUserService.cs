using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;
using FluentValidation;
using System.Security.Claims;
using SpaFramework.App.Utilities;
using SpaFramework.App.Utilities.Serialization;

namespace SpaFramework.App.Services.Data.Accounts
{
    public class ApplicationUserService : EntityWriteService<ApplicationUser, Guid>
    {
        public ApplicationUserService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<ApplicationUser> validator, ILogger<EntityWriteService<ApplicationUser, Guid>> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        protected override async Task<IQueryable<ApplicationUser>> ApplyIdFilter(IQueryable<ApplicationUser> queryable, Guid id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override async Task<IQueryable<ApplicationUser>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<ApplicationUser> queryable)
        {
            // This overrides the base implementation of ApplyReadSecurity to allow users to read their own user

            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return queryable;

            return queryable.Where(x => x.Id == applicationUser.Id);
        }

        protected override async Task<bool> CanWrite(ApplicationUser applicationUser, ApplicationUser dataModel, Dictionary<string, object> extraData)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            if (applicationUser.Id == dataModel.Id)
                return true;

            return false;
        }

        protected override async Task<bool> CanInclude(ApplicationUser applicationUser, string include)
        {
            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }
		
		protected override async Task<ApplicationUser> StripNavigationProperties(ApplicationUser dataModel)
        {
            return SerializationUtilities.CloneModel<ApplicationUser>(dataModel, explicitIncludes: new string[] { "Dealers", "Roles", "Contact", "Contact.ContactBusinessUnits" });
        }

        protected override async Task OnCreating(ClaimsPrincipal user, ApplicationUser dataModel, Dictionary<string, object> extraData)
        {

            var applicationUser = await GetApplicationUser(user);

            if (string.IsNullOrEmpty(dataModel.UserName))
                dataModel.UserName = dataModel.Email;

            dataModel.NormalizedUserName = dataModel.UserName.ToUpper();

            if (!string.IsNullOrEmpty(dataModel.Email))
                dataModel.NormalizedEmail = dataModel.Email.ToUpper();

            dataModel.SecurityStamp = string.Empty;

            if (string.IsNullOrEmpty(dataModel.PasswordHash))
            {
                dataModel.PasswordHash = "AQAAAAEAACcQAAAAEPho8KMQakeHgisftWAFFd8l688LJwa9sCeueDvcN4N1XU0f6v9aToxk5T/H+5AyOQ==";
                dataModel.SecurityStamp = "QRHAJELJZG7BO2GUQIQXKBIGBDICW4ZD";
            }

            await CreateLinkedItems<ApplicationUserRole, Guid>(user, dataModel, dataModel.Roles, 
                (linkedItem, parentId) => { linkedItem.UserId = parentId; },
                item => { item.Id = Guid.NewGuid(); });

            await base.OnCreating(user, dataModel, extraData);
        }

        protected override async Task OnUpdating(ClaimsPrincipal user, ApplicationUser dataModel, ApplicationUser oldDataModel, Dictionary<string, object> extraData)
        {

            await UpdateLinkedItems<ApplicationUserRole, Guid>(user, dataModel, dataModel.Roles, 
                (linkedItem, parentId) => { linkedItem.UserId = parentId; }, 
                (dbContext, parentId) => { return dbContext.ApplicationUserRoles.Where(x => x.UserId == parentId); },
                null,
                item => { item.Id = Guid.NewGuid(); });

            dataModel.PasswordHash = oldDataModel.PasswordHash;
            dataModel.SecurityStamp = oldDataModel.SecurityStamp;
            dataModel.EmailConfirmed = oldDataModel.EmailConfirmed;

            if (!string.IsNullOrEmpty(dataModel.Email))
                dataModel.NormalizedEmail = dataModel.Email.ToUpper();

            dataModel.NormalizedUserName = dataModel.UserName.ToUpper();

            await base.OnUpdating(user, dataModel, oldDataModel, extraData);
        }

        protected override async Task OnUpdated(ClaimsPrincipal user, ApplicationUser dataModel, ApplicationUser oldDataModel, Dictionary<string, object> extraData)
        {
            await base.OnUpdated(user, dataModel, oldDataModel, extraData);
        }
    }
}
