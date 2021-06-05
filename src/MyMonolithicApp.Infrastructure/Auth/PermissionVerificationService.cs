using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyMonolithicApp.Infrastructure.Data;
using PermissionBasedAuthorisation;

namespace MyMonolithicApp.Infrastructure.Auth
{
    public class PermissionVerificationService : IPermissionVerificationService
    {
        private readonly ApplicationDbContext _dbContext;

        public PermissionVerificationService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> ChallengePermissionAsync(ClaimsPrincipal claimsPrincipal,
            PermissionRequirement requirement)
        {
            if (claimsPrincipal.Identity?.IsAuthenticated == true)
            {
                var roleIdString = claimsPrincipal.FindFirstValue(ClaimTypes.Role);
                var roleId = Guid.Parse(roleIdString);

                return await _dbContext.RolesPermissions
                    .AnyAsync(rp => rp.RoleId == roleId && rp.PermissionCode == requirement.Permission);
            }
            return false;
        }
    }
}
