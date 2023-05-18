using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.App.Models.Data.Jobs;
using SpaFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Services.Data.Jobs
{
    public class JobService : EntityWriteService<Job, Guid>
    {

        public JobService(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, IValidator<Job> validator, ILogger<EntityWriteService<Job, Guid>> logger) : base(dbContext, configuration, userManager, validator, logger)
        {
        }

        protected override async Task<IQueryable<Job>> ApplyIdFilter(IQueryable<Job> queryable, Guid id)
        {
            return queryable.Where(x => x.Id == id);
        }

        protected override async Task<IQueryable<Job>> ApplyReadSecurity(ApplicationUser applicationUser, IQueryable<Job> queryable)
        {
            // Site admins can read all users
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return queryable;

            return queryable.Where(x => false);
        }

        protected override async Task<bool> CanWrite(ApplicationUser applicationUser, Job dataModel, Dictionary<string, object> extraData)
        {
            if (await _userManager.IsInRoleAsync(applicationUser, ApplicationRoleNames.SuperAdmin))
                return true;

            return false;
        }

        private async Task<bool> Increment(Guid jobId, string columnName)
        {
            bool isDone = false;

            var connectionString = _configuration.GetConnectionString("Default");
            using (var sqlConnection = new SqlConnection(connectionString))
            {
                await sqlConnection.OpenAsync();

                var sqlCommand = sqlConnection.CreateCommand();
                sqlCommand.CommandText = $"UPDATE Jobs SET {columnName} = {columnName} + 1, Updated = @Timestamp OUTPUT INSERTED.ExpectedCount, INSERTED.SuccessCount, INSERTED.FailureCount WHERE Id = @JobId";
                sqlCommand.Parameters.Add(new SqlParameter("@JobId", jobId));
                sqlCommand.Parameters.Add(new SqlParameter("@Timestamp", DateTime.UtcNow));

                var sqlDataReader = await sqlCommand.ExecuteReaderAsync();
                if (await sqlDataReader.ReadAsync())
                {
                    long expectedCount = (long)sqlDataReader["ExpectedCount"];
                    long successCount = (long)sqlDataReader["SuccessCount"];
                    long failureCount = (long)sqlDataReader["FailureCount"];

                    isDone = expectedCount == successCount + failureCount;
                }

                if (isDone)
                {
                    sqlCommand = sqlConnection.CreateCommand();
                    sqlCommand.CommandText = "UPDATE Jobs SET Updated = @Timestamp, Ended = @Timestamp WHERE Id = @JobId";
                    sqlCommand.Parameters.Add(new SqlParameter("@JobId", jobId));
                    sqlCommand.Parameters.Add(new SqlParameter("@Timestamp", DateTime.UtcNow));

                    await sqlCommand.ExecuteNonQueryAsync();
                }

                await sqlConnection.CloseAsync();
            }

            return isDone;
        }

        public async Task<bool> IncrementSuccessCount(Guid jobId)
        {
            return await Increment(jobId, "SuccessCount");
        }

        public async Task<bool> IncrementFailureCount(Guid jobId)
        {
            return await Increment(jobId, "FailureCount");
        }
    }
}
