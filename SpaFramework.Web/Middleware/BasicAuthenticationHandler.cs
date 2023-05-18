using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SpaFramework.App.DAL;
using SpaFramework.App.Models.Data.Accounts;
using SpaFramework.Core.Utilities;
using SpaFramework.Web.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SpaFramework.Web.Middleware
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager
            )
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            ApplicationUser applicationUser = null;

            // A temporary token can be generated at /api/account/getTemporaryToken
            if (Request.Query.ContainsKey("temporaryToken"))
            {
                var token = Request.Query["temporaryToken"].First();

                var tokenParts = token.Split("|").ToList();
                if (tokenParts.Count != 4)
                    return AuthenticateResult.Fail("Invalid TemporaryToken");

                long tokenTimestamp = long.Parse(tokenParts[0]);
                var tokenUserId = Guid.Parse(tokenParts[1]);
                var tokenNonce = tokenParts[2];
                var tokenSignature = tokenParts[3];

                if (DateTime.UtcNow.Subtract(new DateTime(tokenTimestamp)).Seconds > 30)
                    return AuthenticateResult.Fail("TemporaryToken has expired");

                var checkSignature = EncryptionUtilities.GetHash(tokenTimestamp + "|" + tokenUserId.ToString() + "|" + tokenNonce, _configuration.GetValue<string>("JwtKey"), false);
                if (checkSignature != tokenSignature)
                    return AuthenticateResult.Fail("Invalid TemporaryToken signature");

                applicationUser = _userManager.Users.Where(x => x.Id == tokenUserId).FirstOrDefault();
                if (applicationUser == null)
                    return AuthenticateResult.Fail("Invalid TemporaryToken user");
            }

            // Basic authentication is generally a base64-encoded "username:password" string. We need to capture a channelId here, so we instead take a string of "username/channelId:password"

            if (applicationUser == null)
            {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.NoResult();

            // Basic authentication is generally a base64-encoded "username:password" string. We need to capture a channelId here, so we instead take a string of "username/channelId:password"

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];

                var result = await _signInManager.PasswordSignInAsync(username, password, false, false);

                    if (result.Succeeded)
                        applicationUser = _userManager.Users.SingleOrDefault(r => r.UserName == username);
                }
                catch
                {
                    return AuthenticateResult.Fail("Invalid Authorization Header");
                }
            }

            if (applicationUser == null)
                return AuthenticateResult.Fail("Invalid Username or Password");

            var claims = await WebUtilities.GenerateClaims(_configuration, _userManager, applicationUser);

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }

}
