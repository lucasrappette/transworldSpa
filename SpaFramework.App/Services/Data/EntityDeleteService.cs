using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityDeleteService<TDataModel, TIdType> : EntityWriteService<TDataModel, TIdType>, IEntityWriteService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, new()
    {
        public EntityDeleteService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<TDataModel> validator, ILogger<EntityWriteService<TDataModel, TIdType>> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        #region CRUD Operations

        public async Task<TDataModel> Delete(ClaimsPrincipal user, TIdType id)
        {
            return await Delete(user, id, new Dictionary<string, object>());
        }

        public async Task<TDataModel> Delete(ClaimsPrincipal user, TIdType id, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            TDataModel dataModel = await GetItemById(applicationUser, id, null);

            if (!await CanDelete(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            await OnDeleting(user, dataModel);

            _dbContext.Set<TDataModel>().Remove(dataModel);
            await _dbContext.SaveChangesAsync();

            await OnDeleted(user, dataModel);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Deleted", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        #endregion

        #region Basic Operation Permissions

        protected virtual async Task<bool> CanDelete(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }

        #endregion

        protected virtual async Task OnDeleting(ClaimsPrincipal user, TDataModel dataModel) { }

        protected virtual async Task OnDeleted(ClaimsPrincipal user, TDataModel dataModel) { }

        /// <summary>
        /// Deletes linked items -- that is, items in a many-to-many relationship with this entity that can be created as children of this entity. Call this in OnDeleting for each linked table.
        /// </summary>
        /// <typeparam name="TLinkedItem"></typeparam>
        /// <typeparam name="TLinkedItemIdType"></typeparam>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <param name="existingLinkedItemsGetter">A function that returns all the linked items for a given parent ID</param>
        /// <returns></returns>
        protected async Task DeleteLinkedItems<TLinkedItem, TLinkedItemIdType>(ClaimsPrincipal user, TDataModel dataModel, Func<ApplicationDbContext, TIdType, IQueryable<TLinkedItem>> existingLinkedItemsGetter)
            where TLinkedItem : IHasId<TLinkedItemIdType>
        {
            List<TLinkedItem> existingItems = await existingLinkedItemsGetter(_dbContext, dataModel.GetId()).ToListAsync();

            foreach (var item in existingItems)
                _dbContext.Entry(item).State = EntityState.Deleted;
        }
    }
}
