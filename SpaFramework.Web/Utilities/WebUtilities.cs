using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SpaFramework.App.Models;
using SpaFramework.Core.Models;
using Microsoft.AspNetCore.Http;

namespace SpaFramework.Web.Utilities
{
    public static class WebUtilities
    {
        public static async Task<List<Claim>> GenerateClaims(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationUser applicationUser)
        {
            return await App.Utilities.UserUtilities.GenerateClaims(configuration, userManager, applicationUser);
        }

        public static async Task<string> GenerateJwtToken(IConfiguration configuration, UserManager<ApplicationUser> userManager, ApplicationUser applicationUser)
        {
            var claims = await GenerateClaims(configuration, userManager, applicationUser);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(configuration["JwtExpireDays"]));

            var token = new JwtSecurityToken(
                configuration["JwtIssuer"],
                configuration["JwtIssuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public static string GetCallbackUrlBase(HttpRequest request)
        {
            // Check for X-Forwarded-Host -- if this is running behind NGrok for local dev, we need to use that instead of the host header
            return request.Scheme + "://" + (request.Headers.ContainsKey("X-Forwarded-Host") ? request.Headers["X-Forwarded-Host"].First() : request.Host.Value) + (request.Host.Port != null ? ":" + request.Host.Port.ToString() : "");
        }
    }
}
