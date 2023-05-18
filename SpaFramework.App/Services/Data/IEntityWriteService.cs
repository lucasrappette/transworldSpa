using SpaFramework.App.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
namespace SpaFramework.App.Services.Data
{
    public interface IEntityWriteService<TDataModel, TIdType> : IEntityReadService<TDataModel, TIdType>
        where TDataModel : IEntity
    {
        Task<TDataModel> Create(ClaimsPrincipal user, TDataModel dataModel);
        Task<TDataModel> Update(ClaimsPrincipal user, TDataModel dataModel);
        Task<TDataModel> Upsert(ClaimsPrincipal user, TDataModel dataModel);
        Task<TDataModel> GetNew(ClaimsPrincipal user, TDataModel templateDataModel);
    }
}
