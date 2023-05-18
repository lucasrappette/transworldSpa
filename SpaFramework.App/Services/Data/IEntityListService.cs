using SpaFramework.App.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data
{
    public interface IEntityListService<TDataModel>
        where TDataModel : IEntity
    {
        Task<List<TDataModel>> GetAll(ClaimsPrincipal user, int offset, int limit, string order, string includes, string filter, bool noTracking, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null);
        Task<long> GetAllCount(ClaimsPrincipal user, string filter, int maxCount, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> filterFunc = null);
    }
}
