using Microsoft.EntityFrameworkCore.DynamicLinq;
using System.Linq.Dynamic.Core;
using SpaFramework.App.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using SpaFramework.App.DAL;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using SpaFramework.App.Models.Data.Accounts;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SpaFramework.App.Exceptions;

namespace SpaFramework.App.Services.Data
{
    public abstract class EntityListService<TDataModel> : IEntityListService<TDataModel>
        where TDataModel : class, IEntity
    {
        protected readonly ApplicationDbContext _dbContext;
        protected readonly IConfiguration _configuration;
        protected readonly UserManager<ApplicationUser> _userManager;
        protected readonly ILogger<EntityListService<TDataModel>> _logger;

        protected static ParsingConfig _parsingConfig;
        protected static ParsingConfig GetParsingConfig()
        {
            if (_parsingConfig == null)
            {
                _parsingConfig = new ParsingConfig();
                _parsingConfig.CustomTypeProvider = new CustomDynamicLinqProvider();
            }

            return _parsingConfig;
        }

        public EntityListService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, ILogger<EntityListService<TDataModel>> logger)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _userManager = userManager;
            _logger = logger;
        }

        #region CRUD Operations

        /// <summary>
        /// Returns all entities
        /// </summary>
        /// <param name="user">The user making the request. Permissions and row-level security are based on this user</param>
        /// <param name="offset">The index (zero-based) of the first entity to read</param>
        /// <param name="limit">The number of entities to read. Use with offset for paging results</param>
        /// <param name="order">A string indicating the field to search on and the order, e.g. "Name desc"</param>
        /// <param name="includes">A string indicating navigation properties (table joins) to include. This is intended for use from controllers, where a string value is passed for includes. In managed code, use the includeFunc instead</param>
        /// <param name="filter">A string indicating any filter to apply to the data. This is intended for use from controllers, where a string value is passed for includes. In managed code, use the filterFunc instead</param>
        /// <param name="noTracking">If set to true, EF won't track the entity. If the entity won't be changed, set this to true</param>
        /// <param name="filterFunc">A func that lets you apply additional "Where" clauses to the IQueryable used for retreiving data. Use this to add a filter to the data</param>
        /// <param name="includesFunc">A func that lets you apply additional "Include" clauses to the IQueryable. Use this to bring back related data you need</param>
        /// <returns></returns>
        public async Task<List<TDataModel>> GetAll(ClaimsPrincipal user, int offset, int limit, string order, string includes, string filter, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var queryable = await GetAllQueryable(user, offset, limit, order, includes, filter, noTracking, filterFunc, includesFunc);

            List<TDataModel> dataModelItems = await queryable.ToListAsync();

            return dataModelItems;
        }

        /// <summary>
        /// Returns all entities as an AsyncEnumerable list
        /// </summary>
        /// <param name="user">The user making the request. Permissions and row-level security are based on this user</param>
        /// <param name="offset">The index (zero-based) of the first entity to read</param>
        /// <param name="limit">The number of entities to read. Use with offset for paging results</param>
        /// <param name="order">A string indicating the field to search on and the order, e.g. "Name desc"</param>
        /// <param name="includes">A string indicating navigation properties (table joins) to include. This is intended for use from controllers, where a string value is passed for includes. In managed code, use the includeFunc instead</param>
        /// <param name="filter">A string indicating any filter to apply to the data. This is intended for use from controllers, where a string value is passed for includes. In managed code, use the filterFunc instead</param>
        /// <param name="noTracking">If set to true, EF won't track the entity. If the entity won't be changed, set this to true</param>
        /// <param name="filterFunc">A func that lets you apply additional "Where" clauses to the IQueryable used for retreiving data. Use this to add a filter to the data</param>
        /// <param name="includesFunc">A func that lets you apply additional "Include" clauses to the IQueryable. Use this to bring back related data you need</param>
        /// <returns></returns>
        public async Task<IAsyncEnumerable<TDataModel>> GetAllAsAsyncEnumerable(ClaimsPrincipal user, int offset, int limit, string order, string includes, string filter, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var queryable = await GetAllQueryable(user, offset, limit, order, includes, filter, noTracking, filterFunc, includesFunc);

            IAsyncEnumerable<TDataModel> dataModelEnumerable = queryable.AsAsyncEnumerable();

            return dataModelEnumerable;
        }

        protected async Task<IQueryable<TDataModel>> GetAllQueryable(ClaimsPrincipal user, int offset, int limit, string order, string includes, string filter, bool noTracking = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            IQueryable<TDataModel> queryable = (await GetDataSet(applicationUser));

            queryable = await ApplyIncludes(applicationUser, queryable, includes, includesFunc);

            if (!string.IsNullOrEmpty(filter))
                queryable = queryable.Where(GetParsingConfig(), filter);

            if (filterFunc != null)
                queryable = filterFunc(queryable);

            if (!string.IsNullOrEmpty(order))
                queryable = queryable.OrderBy(order);

            queryable = queryable
                .Skip(offset)
                .Take(limit);

            if (noTracking)
                queryable = queryable.AsNoTracking();

            return queryable;
        }
        
        /// <summary>
                 /// Returns a count of entities that match a given query
                 /// </summary>
                 /// <param name="user"></param>
                 /// <param name="filter"></param>
                 /// <param name="maxCount"></param>
                 /// <param name="filterFunc"></param>
                 /// <returns></returns>
        public async Task<long> GetAllCount(ClaimsPrincipal user, string filter, int maxCount, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null)
        {
            var applicationUser = await GetApplicationUser(user);

            IQueryable<TDataModel> queryable = (await GetDataSet(applicationUser));

            if (!string.IsNullOrEmpty(filter))
                queryable = queryable.Where(GetParsingConfig(), filter);

            if (filterFunc != null)
                queryable = filterFunc(queryable);

            if (maxCount > 0)
                queryable = queryable.Take(maxCount);

            long count = await queryable.LongCountAsync();

            return count;
        }

        #endregion

        #region Basic Operation Permissions

        /// <summary>
        /// Returns the list of roles that have read permissions. If null, everyone will have permission. If empty, no one will have permission. Otherwise, only those in this list will have permission.
        /// 
        /// Note that, if this service implements IDonorAccessService, a user always has access to their own donor data, regardless of their other permissions.
        /// 
        /// Note too that outlet-level filtering is also applied. That is, unless ApplicationUser.AllOutlets is true, only the outlets that the user has access to will be returned.
        /// </summary>
        protected virtual List<string> ReadRoles => null;

        /// <summary>
        /// Returns a queryable with any security-related read filtering applied.
        /// 
        /// The default implementation limits read access in the following way:
        /// * If ReadRoles is null, everyone has access to all items
        /// * If ReadRoles is an empty list, no one has access to any items
        /// * If the user is in one of the ReadRoles
        ///   * If this service implements IClientAccessListService, the user is given access to items associated with their Client. If the user has AllClients set, this returns all Clients
        ///   * Otherwise, the user has access to all items
        /// 
        /// This can be overwritten in derived classes to enforce their own security model
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="queryable"></param>
        /// <returns></returns>
        protected virtual async Task<IQueryable<TDataModel>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<TDataModel> queryable)
        {
            if (ReadRoles == null)
                return queryable;

            if (applicationUser == null)
            {
                _logger.LogWarning("Cannot read entity because there is no user: DataModelType: {DataModelType}", typeof(TDataModel).Name);
                return queryable.Where(x => false);
            }

            var roles = await _userManager.GetRolesAsync(applicationUser);

            // If the user doesn't match any roles, they don't have access
            if (!roles.Any(r => ReadRoles.Contains(r)))
            {
                _logger.LogWarning("Cannot read entity because the user does not have permissions: DataModelType: {DataModelType}, ApplicationUserId: {ApplicationUserId}", typeof(TDataModel).Name, applicationUser.Id);
                return queryable.Where(x => false);
            }

            // The user has a matching role, and this isn't outlet-specific data, so return it all
            return queryable;
        }

        #endregion

        /// <summary>
        /// Returns an ApplicationUser, with necessary security attributes, for a ClaimsPrincipal
        /// </summary>
        /// <param name="user"></param>
        /// <param name="includes"></param>
        /// <returns></returns>
        protected async Task<ApplicationUser> GetApplicationUser(ClaimsPrincipal user, Func<IQueryable<ApplicationUser>, IQueryable<ApplicationUser>> includes = null)
        {
            if (user == null)
                return null;

            string userIdString = _userManager.GetUserId(user);
            if (string.IsNullOrEmpty(userIdString))
                return null;

            Guid userId;
            if (!Guid.TryParse(userIdString, out userId))
                return null;

            IQueryable<ApplicationUser> queryable = _dbContext.ApplicationUsers.AsNoTracking();


            if (includes != null)
                queryable = includes(queryable);

            return await queryable
                .Where(au => au.Id == userId)
                .SingleOrDefaultAsync();
        }

        /// <summary>
        /// Gets the underlying dataset for the data model with read security applied
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <returns></returns>
        protected virtual async Task<IQueryable<TDataModel>> GetDataSet(ApplicationUser applicationUser)
        {
            IQueryable<TDataModel> queryable = _dbContext.Set<TDataModel>();

            if (typeof(IHasDeleted).IsAssignableFrom(typeof(TDataModel)))
                queryable = queryable.Where(x => !((IHasDeleted)x).Deleted);

            return await ApplyReadSecurity(applicationUser, queryable);
        }

        protected virtual async Task<IQueryable<TDataModel>> ApplyIncludes(ApplicationUser applicationUser, IQueryable<TDataModel> queryable, string includes, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null)
        {
            if (!string.IsNullOrEmpty(includes))
            {
                foreach (string include in includes.Split(","))
                {
                    // Break this apart at the dots, and ensure that each character after a dot is upper case -- this converts it from camelCase to PascalCase. There's probably a regex that could do this more efficiently. But ugh, regex.
                    string[] parts = include.Split(".");
                    string pascalCasedInclude = string.Join(".", parts.Select(x => x.Substring(0, 1).ToUpper() + x.Substring(1)));
                    if (!await CanInclude(applicationUser, pascalCasedInclude))
                        throw new ForbiddenException("User does not have permissions to include " + pascalCasedInclude);
                    queryable = queryable.Include(pascalCasedInclude);
                }
            }

            if (includesFunc != null)
                queryable = includesFunc(queryable);

            return queryable;
        }

        /// <summary>
        /// Ensures a user has permission to include the specified entity. Override this to limit access to related entities
        /// </summary>
        /// <param name="applicationUser"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        protected virtual async Task<bool> CanInclude(ApplicationUser applicationUser, string include)
        {
            return true;
        }
    }
}
