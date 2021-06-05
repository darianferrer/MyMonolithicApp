using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.Stores
{
    public class CustomRoleStore : IRoleStore<RoleEntity>
    {
        private readonly ApplicationDbContext _context;

        private DbSet<RoleEntity> RolesSet { get => _context.Set<RoleEntity>(); }

        public CustomRoleStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            _context.Add(role);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            _context.Remove(role);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "concurrencyError",
                    Description = "Failed to delete role",
                });
            }
            return IdentityResult.Success;
        }

        public async Task<RoleEntity> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await RolesSet.FindAsync(new object[] { Guid.Parse(roleId) }, cancellationToken);
        }

        public async Task<RoleEntity> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await RolesSet.FirstOrDefaultAsync(r => r.NormalisedName == normalizedRoleName, cancellationToken);
        }

        public Task<string> GetNormalizedRoleNameAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return role == null
                ? throw new ArgumentNullException(nameof(role))
                : Task.FromResult(role.NormalisedName);
        }

        public Task<string> GetRoleIdAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return role == null
                ? throw new ArgumentNullException(nameof(role))
                : Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return role == null
                ? throw new ArgumentNullException(nameof(role))
                : Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(RoleEntity role, string normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.NormalisedName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetRoleNameAsync(RoleEntity role, string roleName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            role.Name = roleName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(RoleEntity role, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            _context.Attach(role);
            role.ConcurrencyStamp = Guid.NewGuid().ToString();
            _context.Update(role);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "concurrencyError",
                    Description = "Failed to update role",
                });
            }
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
