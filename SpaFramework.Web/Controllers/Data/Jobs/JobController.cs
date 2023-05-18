using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.App.Services.Data;
using System;

namespace SpaFramework.Web.Controllers.Data.Jobs
{
    [Authorize]
    public class JobController : EntityWriteController<Job, IEntityWriteService<Job, Guid>, Guid>
    {
        public JobController(IConfiguration configuration, IEntityWriteService<Job, Guid> service) : base(configuration, service)
        {
        }
    }
}
