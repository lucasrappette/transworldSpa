using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Exceptions;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Generics;
using SpaFramework.App.Utilities;
using SpaFramework.App.Utilities.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityWriteService<TDataModel, TIdType> : EntityReadService<TDataModel, TIdType>, IEntityWriteService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>, new()
    {
        protected readonly IValidator<TDataModel> _validator;

        public EntityWriteService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<TDataModel> validator, ILogger<EntityWriteService<TDataModel, TIdType>> logger) : base(dbContext, configuration, userManager, logger)
        {
            _validator = validator;
        }

        #region CRUD Operations

        /// <summary>
        /// Creates a new row in the database table for the entity
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public async Task<TDataModel> Create(ClaimsPrincipal user, TDataModel dataModel)
        {
            return await Create(user, dataModel, new Dictionary<string, object>());
        }

        protected async Task<TDataModel> Create(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            dataModel = await StripNavigationProperties(dataModel);

            if (!await CanCreate(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            if (dataModel is IHasLastModification)
            {
                ((IHasLastModification)dataModel).LastModification = DateTime.UtcNow;
                ((IHasLastModification)dataModel).LastModificationApplicationUserId = applicationUser.Id;
            }

            await OnCreating(user, dataModel, extraData);

            _validator.ValidateAndThrow(dataModel);

            _dbContext.Set<TDataModel>().Add(dataModel);
            await _dbContext.SaveChangesAsync();

            await OnCreated(user, dataModel, extraData);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Created", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        /// <summary>
        /// Updates an entity in the database
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public async Task<TDataModel> Update(ClaimsPrincipal user, TDataModel dataModel)
        {
            return await Update(user, dataModel, new Dictionary<string, object>());
        }

        protected async Task<TDataModel> Update(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            dataModel = await StripNavigationProperties(dataModel);

            if (!await CanUpdate(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            TDataModel oldDataModel = await GetItemById(applicationUser, dataModel.GetId(), null);
            _dbContext.Entry(oldDataModel).State = EntityState.Detached;

            if (dataModel is IHasLastModification)
            {
                ((IHasLastModification)dataModel).LastModification = DateTime.UtcNow;
                ((IHasLastModification)dataModel).LastModificationApplicationUserId = applicationUser.Id;
            }

            await OnUpdating(user, dataModel, oldDataModel, extraData);

            _validator.ValidateAndThrow(dataModel);

            _dbContext.Entry(dataModel).State = EntityState.Modified;
            await _dbContext.SaveChangesAsync();

            await OnUpdated(user, dataModel, oldDataModel, extraData);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Updated", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        /// <summary>
        /// Creates or updates an entity based on its primary key
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public async Task<TDataModel> Upsert(ClaimsPrincipal user, TDataModel dataModel)
        {
            return await Upsert(user, dataModel, new Dictionary<string, object>());
        }

        protected async Task<TDataModel> Upsert(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            dataModel = await StripNavigationProperties(dataModel);

            if (!await CanUpsert(applicationUser, dataModel, extraData))
                throw new ForbiddenException();

            if (dataModel is IHasLastModification)
            {
                ((IHasLastModification)dataModel).LastModification = DateTime.UtcNow;
                ((IHasLastModification)dataModel).LastModificationApplicationUserId = applicationUser.Id;
            }

            await OnUpserting(user, dataModel, extraData);

            _validator.ValidateAndThrow(dataModel);

            await _dbContext.Set<TDataModel>()
                .Upsert(dataModel)
                .RunAsync();

            await OnUpserted(user, dataModel, extraData);

            _logger.LogInformation("Entity {EntityType} {EntityAction}: {EntityValue} ({EntityId}) by {UserName} ({UserId})", typeof(TDataModel).Name, "Upserted", (dataModel is ILoggableName ? ((ILoggableName)dataModel).LoggableName : dataModel.ToString()), dataModel.GetId(), user.GetUserName(), user.GetUserId());

            return dataModel;
        }

        /// <summary>
        /// Creates or updates multiple entities based on their primary key
        /// </summary>
        /// <param name="user"></param>
        /// <param name="dataModels"></param>
        /// <returns></returns>
        public async Task<List<TDataModel>> UpsertRange(ClaimsPrincipal user, List<TDataModel> dataModels)
        {
            return await UpsertRange(user, dataModels, new Dictionary<string, object>());
        }

        protected async Task<List<TDataModel>> UpsertRange(ClaimsPrincipal user, List<TDataModel> dataModels, Dictionary<string, object> extraData)
        {
            var applicationUser = await GetApplicationUser(user);

            List<TDataModel> rangeDataModels = new List<TDataModel>();

            foreach (TDataModel dataModel in dataModels)
            {
                TDataModel rangeDataModel = await StripNavigationProperties(dataModel);

                if (!await CanUpsert(applicationUser, rangeDataModel, extraData))
                    throw new ForbiddenException();

                if (rangeDataModel is IHasLastModification)
                {
                    ((IHasLastModification)rangeDataModel).LastModification = DateTime.UtcNow;
                    ((IHasLastModification)rangeDataModel).LastModificationApplicationUserId = applicationUser.Id;
                }

                rangeDataModels.Add(rangeDataModel);
            }

            await OnUpsertingRange(user, rangeDataModels, extraData);

            foreach (TDataModel dataModel in rangeDataModels)
                _validator.ValidateAndThrow(dataModel);

            await _dbContext.Set<TDataModel>()
                .UpsertRange(rangeDataModels)
                .RunAsync();

            await OnUpsertedRange(user, rangeDataModels, extraData);

            return rangeDataModels;
        }

        /// <summary>
        /// Returns a new entity with default values but does not save the entity to the database
        /// 
        /// A template data model can optionally be passed in. The way this is used depends on the particular model. For instance, this might contain the ID of a relationship
        /// that should be pre-populated. Note, though, that the template data model can (and often will be) null.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="templateDataModel"></param>
        /// <returns></returns>
        public async Task<TDataModel> GetNew(ClaimsPrincipal user, TDataModel templateDataModel)
        {
            TDataModel dataModel = new TDataModel();

            await OnGettingNew(user, dataModel, templateDataModel);

            return dataModel;
        }

        #endregion

        #region Basic Operation Permissions

        /// <summary>
        /// Returns the list of roles that have write permissions. If null, everyone will have permission. If empty, no one will have permission. Otherwise, only those in this list will have permission.
        /// 
        /// Note that, if this service implements IDonorAccessService, a user always has access to their own donor data, regardless of their other permissions.
        /// 
        /// Note too that outlet-level access is also applied. That is, unless ApplicationUser.AllOutlets is true, only the outlets that the user has access to will be returned.
        /// </summary>
        protected virtual List<string> WriteRoles => null;

        /// <summary>
        /// Returns whether or not a user can perform non-read (create, update, delete) operations on a model. By default, CanCreate, CanUpdate, and CanDelete call this method, but each of them
        /// may provide their own custom implementation that won't call this method. The data model passed in should already contain all the data needed to make a determination on the user's
        /// ability to write the data.
        /// 
        /// The default implementation limits write access in the following way:
        /// * If WriteRoles is null, everyone can write all items
        /// * If WriteRoles is an empty list, no one has write any items
        /// * If the user is not in any of the WriteRoles
        ///   * If this service implements IDonorAccessWriteService and the user is the donor, the user has access to their own donor data
        ///   * Otherwise, the user has no access to any items
        /// * If the user is in one of the WriteRoles
        ///   * If this service implements IOutletAccessWriteService, the user is given access to items associated with their outlet(s). If the user has AllOutlets set, the user can access data from all outlets
        ///   * Otherwise, the user has write access to all items
        /// 
        /// This can be overwritten in derived classes to enforce their own security model
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="dataModel"></param>
        /// <param name="extraData"></param>
        /// <returns></returns>
        protected virtual async Task<bool> CanWrite(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData)
        {
            if (WriteRoles == null)
                return true;

            if (applicationUser == null)
                return false;

            var roles = await _userManager.GetRolesAsync(applicationUser);

            // If the user doesn't match any roles, they don't have access
            if (!roles.Any(r => WriteRoles.Contains(r)))
                return false;

            // The user has a matching role, and this isn't outlet-specific data, so allow write
            return true;
        }
        
        protected virtual async Task<bool> CanCreate(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }
        protected virtual async Task<bool> CanUpdate(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }
        protected virtual async Task<bool> CanUpsert(ApplicationUser applicationUser, TDataModel dataModel, Dictionary<string, object> extraData) { return await CanWrite(applicationUser, dataModel, extraData); }

        #endregion

        protected virtual async Task OnCreating(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpdating(ClaimsPrincipal user, TDataModel dataModel, TDataModel oldDataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpserting(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpsertingRange(ClaimsPrincipal user, List<TDataModel> dataModels, Dictionary<string, object> extraData)
        {
            foreach (var dataModel in dataModels)
                await OnUpserting(user, dataModel, extraData);
        }

        protected virtual async Task OnCreated(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpdated(ClaimsPrincipal user, TDataModel dataModel, TDataModel oldDataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpserted(ClaimsPrincipal user, TDataModel dataModel, Dictionary<string, object> extraData) { }
        protected virtual async Task OnUpsertedRange(ClaimsPrincipal user, List<TDataModel> dataModels, Dictionary<string, object> extraData)
        {
            foreach (var dataModel in dataModels)
                await OnUpserted(user, dataModel, extraData);
        }

        protected virtual async Task OnGettingNew(ClaimsPrincipal user, TDataModel dataModel, TDataModel templateDataModel) { }

        protected virtual async Task<TDataModel> StripNavigationProperties(TDataModel dataModel)
        {
            return SerializationUtilities.CloneModel<TDataModel>(dataModel);
        }

        /// <summary>
        /// Creates linked items -- that is, items in a many-to-many relationship with this entity that can be created as children of this entity. Call this in OnCreating for each linked table.
        /// </summary>
        /// <typeparam name="TLinkedItem"></typeparam>
        /// <typeparam name="TLinkedItemIdType"></typeparam>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <param name="dataModelItems">The list of linked items on the dataModel</param>
        /// <param name="linkedItemParentIdSetter">An action that, for a given linked item, sets the parent ID to this entity</param>
        /// <param name="newIdSetter">A function that sets a new id</param>
        /// <returns></returns>
        protected async Task<List<TLinkedItem>> CreateLinkedItems<TLinkedItem, TLinkedItemIdType>(ClaimsPrincipal user, TDataModel dataModel, List<TLinkedItem> dataModelItems, Action<TLinkedItem, TIdType> linkedItemParentIdSetter, Action<TLinkedItem> newIdSetter = null)
            where TLinkedItem : IHasId<TLinkedItemIdType>
        {
            List<TLinkedItem> touchedLinkedItems = new List<TLinkedItem>();

            if (dataModelItems != null)
            {
                var applicationUser = await GetApplicationUser(user);

                foreach (var item in dataModelItems)
                {
                    linkedItemParentIdSetter(item, dataModel.GetId());

                    if (newIdSetter != null)
                        newIdSetter(item);

                    if (item is IHasLastModification)
                    {
                        ((IHasLastModification)item).LastModification = DateTime.UtcNow;
                        ((IHasLastModification)item).LastModificationApplicationUserId = applicationUser.Id;
                    }

                    if (item is ILinkedItem)
                        ((ILinkedItem)item).IsActive = true;

                    touchedLinkedItems.Add(item);

                    _dbContext.Entry(item).State = EntityState.Added;
                }
            }

            return touchedLinkedItems;
        }

        /// <summary>
        /// Updates linked items -- that is, items in a many-to-many relationship with this entity that can be created as children of this entity. Call this in OnUpdating for each linked table.
        /// </summary>
        /// <typeparam name="TLinkedItem"></typeparam>
        /// <typeparam name="TLinkedItemIdType"></typeparam>
        /// <param name="user"></param>
        /// <param name="dataModel"></param>
        /// <param name="dataModelItems">The list of linked items on the dataModel</param>
        /// <param name="linkedItemParentIdSetter">An action that, for a given linked item, sets the parent ID to this entity</param>
        /// <param name="existingLinkedItemsGetter">A function that returns all the linked items for a given parent ID</param>
        /// <param name="linkedItemUpdater">A function used to update many-to-many relationships</param>
        /// <param name="newIdSetter">A function used to set a new id</param>
        /// <returns></returns>
        protected async Task<List<TLinkedItem>> UpdateLinkedItems<TLinkedItem, TLinkedItemIdType>(ClaimsPrincipal user, TDataModel dataModel, List<TLinkedItem> dataModelItems, Action<TLinkedItem, TIdType> linkedItemParentIdSetter, Func<ApplicationDbContext, TIdType, IQueryable<TLinkedItem>> existingLinkedItemsGetter, Action<TLinkedItem, TLinkedItem> linkedItemUpdater = null, Action<TLinkedItem> newIdSetter = null)
            where TLinkedItem : class, IHasId<TLinkedItemIdType>
        {
            List<TLinkedItemIdType> deletedIds = new List<TLinkedItemIdType>();
            List<TLinkedItem> touchedLinkedItems = new List<TLinkedItem>();

            if (dataModelItems != null)
            {
                var applicationUser = await GetApplicationUser(user);

                List<TLinkedItem> existingItems = await existingLinkedItemsGetter(_dbContext, dataModel.GetId()).ToListAsync();

                foreach (var item in existingItems.Where(x => !dataModelItems.Any(y => y.GetId().Equals(x.GetId()))))
                {
                    if (item is IHasLastModification)
                    {
                        ((IHasLastModification)item).LastModification = DateTime.UtcNow;
                        ((IHasLastModification)item).LastModificationApplicationUserId = applicationUser.Id;
                    }

                    if (item is ILinkedItem)
                        ((ILinkedItem)item).IsActive = false;

                    touchedLinkedItems.Add(item);

                    deletedIds.Add(item.GetId());
                    _dbContext.Entry(item).State = EntityState.Deleted;
                }

                foreach (var item in dataModelItems.Where(x => !existingItems.Any(y => y.GetId().Equals(x.GetId()))))
                {
                    linkedItemParentIdSetter(item, dataModel.GetId());

                    if (newIdSetter != null)
                        newIdSetter(item);

                    if (item is IHasLastModification)
                    {
                        ((IHasLastModification)item).LastModification = DateTime.UtcNow;
                        ((IHasLastModification)item).LastModificationApplicationUserId = applicationUser.Id;
                    }

                    if (item is ILinkedItem)
                        ((ILinkedItem)item).IsActive = true;

                    touchedLinkedItems.Add(item);

                    _dbContext.Entry(item).State = EntityState.Added;
                }

                List<TLinkedItem> ignoreItems = dataModelItems.Where(x => existingItems.Any(y => y.GetId().Equals(x.GetId()))).ToList();
                foreach (var item in ignoreItems)
                {
                    dataModelItems.Remove(item);

                    var existingItem = existingItems.Where(x => x.GetId().Equals(item.GetId())).First();

                    if (linkedItemUpdater != null)
                        linkedItemUpdater(item, existingItem);
                }
            }

            return touchedLinkedItems;
        }
    }
}
