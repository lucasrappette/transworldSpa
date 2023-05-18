using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Utilities;
using SpaFramework.App.Models.Data.Accounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.Worker.Processors
{
    public class WorkItemData
    {
        private readonly IConfiguration _configuration;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public WorkItemData(IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        public async Task<ClaimsPrincipal> GetClaimsPrincipal()
        {
            return await UserUtilities.GetDefaultWorkItemClaimsPrincipal(_configuration, _signInManager, _userManager);
        }
    }
}
