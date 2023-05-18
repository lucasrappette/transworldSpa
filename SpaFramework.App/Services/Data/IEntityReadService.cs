using SpaFramework.App.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace SpaFramework.App.Services.Data
{
    public interface IEntityReadService<TDataModel, TIdType> : IEntityListService<TDataModel>
        where TDataModel : IEntity
    {
        Task<TDataModel> GetOne(ClaimsPrincipal user, TIdType id, string includes, bool detach = false, Func<IQueryable<TDataModel>, IQueryable<TDataModel>> includesFunc = null);
    }
}
