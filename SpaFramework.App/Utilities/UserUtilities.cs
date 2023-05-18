using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SpaFramework.App.Models;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SpaFramework.App.Utilities
{
    public static class UserUtilities
    {
        public static async Task<List<Claim>> GenerateClaims(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationUser applicationUser)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, applicationUser.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, applicationUser.Id.ToString())
            };

            var roles = await userManager.GetRolesAsync(applicationUser);
            foreach (string role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            return claims;
        }

        public static Guid GetUserIdAsGuid(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var claim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            Guid x;
            if (Guid.TryParse(claim.Value, out x))
                return x;

            throw new Exception("UserId cannot be parsed from claim");
        }

        public static string GetUserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var claim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? claim.Value : null;
        }

        public static string GetUserName(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal == null)
                throw new ArgumentNullException(nameof(claimsPrincipal));

            var claim = claimsPrincipal.FindFirst(JwtRegisteredClaimNames.Sub);
            return claim != null ? claim.Value : null;
        }

        public static async Task<ClaimsPrincipal> GetClaimsPrincipal(long applicationUserId, string[] applicationRoleNames = null)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, applicationUserId.ToString())
            };

            if (applicationRoleNames != null)
                foreach (string applicationRoleName in applicationRoleNames)
                    claims.Add(new Claim(ClaimTypes.Role, applicationRoleName));

            return await GetClaimsPrincipal(claims);
        }

        public static async Task<ClaimsPrincipal> GetClaimsPrincipal(List<Claim> claims)
        {
            return new ClaimsPrincipal(new ClaimsIdentity(claims));
        }

        public static async Task<ClaimsPrincipal> GetDefaultWorkItemClaimsPrincipal(IConfiguration configuration, SignInManager<ApplicationUser> signInManager, UserManager<ApplicationUser> userManager)
        {
            Guid applicationUserId = configuration.GetValue<Guid>("DefaultWorkItemUserId");

            var applicationUser = await userManager.FindByIdAsync(applicationUserId.ToString());
            return await GetClaimsPrincipal(await GenerateClaims(configuration, userManager, applicationUser));
            //return await GetClaimsPrincipal(applicationUserId, new string[] { "SuperAdmin" });
        }
    }
}
