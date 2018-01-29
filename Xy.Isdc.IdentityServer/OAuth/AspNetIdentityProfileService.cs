using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Xy.Isdc.IdentityServer.Models;

namespace Xy.Isdc.IdentityServer.OAuth
{
    public class AspNetIdentityProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AspNetIdentityProfileService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subject = context.Subject;
            if (subject == null) throw new ArgumentNullException(nameof(context.Subject));

            var subjectId = subject.GetSubjectId();

            var user = await _userManager.FindByIdAsync(subjectId);
            if (user == null)
                throw new ArgumentException("Invalid subject identifier");

            var claims = await GetClaimsFromUser(user);

            var siteIdClaim = claims.SingleOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress");
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.Email, user.Email));
            context.IssuedClaims.Add(new Claim("siteid", siteIdClaim.Value));
            context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, "User"));

            var roleClaims = claims.Where(x => x.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            foreach (var roleClaim in roleClaims)
            {
                context.IssuedClaims.Add(new Claim(JwtClaimTypes.Role, roleClaim.Value));
            }
        }

        public async Task IsActiveAsync(IsActiveContext context)
        {
            var subject = context.Subject;
            if (subject == null) throw new ArgumentNullException(nameof(context.Subject));

            var subjectId = subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(subjectId);

            context.IsActive = false;

            if (user != null)
            {
                if (_userManager.SupportsUserSecurityStamp)
                {
                    var security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if (security_stamp != null)
                    {
                        var db_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        if (db_security_stamp != security_stamp)
                            return;
                    }
                }

                context.IsActive =
                    !user.LockoutEnabled ||
                    !user.LockoutEnd.HasValue ||
                    user.LockoutEnd <= DateTime.Now;
            }
        }

        private async Task<IEnumerable<Claim>> GetClaimsFromUser(ApplicationUser user)
        {
            var claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Subject, user.Id),
            new Claim(JwtClaimTypes.PreferredUserName, user.UserName)
        };

            if (_userManager.SupportsUserEmail)
            {
                claims.AddRange(new[]
                {
                new Claim(JwtClaimTypes.Email, user.Email),
                new Claim(JwtClaimTypes.EmailVerified, user.EmailConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
            });
            }

            if (_userManager.SupportsUserPhoneNumber && !string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                claims.AddRange(new[]
                {
                new Claim(JwtClaimTypes.PhoneNumber, user.PhoneNumber),
                new Claim(JwtClaimTypes.PhoneNumberVerified, user.PhoneNumberConfirmed ? "true" : "false", ClaimValueTypes.Boolean)
            });
            }

            if (_userManager.SupportsUserClaim)
            {
                claims.AddRange(await _userManager.GetClaimsAsync(user));
            }

            if (_userManager.SupportsUserRole)
            {
                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(JwtClaimTypes.Role, role)));
            }

            return claims;
        }
    }
}
