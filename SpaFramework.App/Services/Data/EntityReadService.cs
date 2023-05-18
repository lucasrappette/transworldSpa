using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.DynamicLinq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityReadService<TDataModel, TIdType> : EntityListService<TDataModel>, IEntityReadService<TDataModel, TIdType>
        where TDataModel : class, IEntity, IHasId<TIdType>
    {
        public EntityReadService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<EntityReadService<TDataModel, TIdType>> logger)
            : base (dbContext, configuration, userManager, logger)
        {
        }

        #region CRUD Operations

        /// <summary>
        /// Returns a single entity based on its ID
        /// </summary>
        /// <param name="user">The user making the request. Permissions and row-level security are based on this user</param>
        /// <param name="id"></param>
        /// <param name="includes">A string indicating navigation properties (table joins) to include. This is intended for use from controllers, where a string value is passed for includes. In managed code, use the includeFunc instead</param>
        /// <param name="noTracking">If set to true, EF won't track the entity. If the entity won't be changed, set this to true</param>
        /// <param name="includesFunc">A func that lets you apply additional "Include" clauses to the IQueryable. Use this to bring back related data you need</param>
        /// <returns></returns>
        public async Task<TDataModel> GetOne(ClaimsPrincipal user, TIdType id, string includes, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            TDataModel dataModel = await GetItemById(applicationUser, id, includes, includesFunc);

            if (noTracking)
                _dbContext.Entry(dataModel).State = EntityState.Detached;

            return dataModel;
        }

        /// <summary>
        /// Returns a single entity based on a filter
        /// </summary>
        /// <param name="user">The user making the request. Permissions and row-level security are based on this user</param>
        /// <param name="noTracking">If set to true, EF won't track the entity. If the entity won't be changed, set this to true</param>
        /// <param name="filterFunc"></param>
        /// <param name="includesFunc"></param>
        /// <returns></returns>
        public async Task<TDataModel> GetOneByFilter(ClaimsPrincipal user, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            IQueryable<TDataModel> queryable = await GetDataSet(applicationUser);
            queryable = await ApplyIncludes(applicationUser, queryable, null, includesFunc);

            if (filterFunc != null)
                queryable = filterFunc(queryable);

            TDataModel dataModel = await queryable.SingleOrDefaultAsync();

            if (dataModel != null && noTracking)
                _dbContext.Entry(dataModel).State = EntityState.Detached;

            return dataModel;
        }

        #endregion

        /// <summary>
        /// Gets an item by ID, ensuring the user has access to it
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="id"></param>
        /// <param name="includes"></param>
        /// <param name="includesFunc"></param>
        /// <returns></returns>
        protected async Task<TDataModel> GetItemById(ApplicationUser applicationUser, TIdType id, string includes, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            IQueryable<TDataModel> queryable = await GetDataSet(applicationUser);
            queryable = await ApplyIncludes(applicationUser, queryable, includes, includesFunc);
            queryable = await ApplyIdFilter(queryable, id);
            return await queryable.SingleOrDefaultAsync();
        }

        protected abstract Task<IQueryable<TDataModel>> ApplyIdFilter(IQueryable<TDataModel> queryable, TIdType id);
    }
}
