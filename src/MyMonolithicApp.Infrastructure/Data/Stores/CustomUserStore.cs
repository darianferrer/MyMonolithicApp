using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data.Stores
{
    public class CustomUserStore : IUserPasswordStore<UserEntity>, IUserRoleStore<UserEntity>
    {
        private readonly ApplicationDbContext _context;

        private DbSet<UserEntity> UsersSet => _context.Set<UserEntity>();
        private IQueryable<UserEntity> Users => UsersSet.Include(x => x.Role);
        private DbSet<RoleEntity> RolesSet => _context.Set<RoleEntity>();

        public CustomUserStore(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityResult> CreateAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            UsersSet.Add(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<UserEntity> FindByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await UsersSet.FindAsync(new object[] { userId }, cancellationToken);
        }

        public async Task<UserEntity> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Users.FirstOrDefaultAsync(u => u.NormalisedUsername == normalizedUserName, cancellationToken);
        }

        public Task<string> GetNormalizedUserNameAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user == null
                ? throw new ArgumentNullException(nameof(user))
                : Task.FromResult(user.NormalisedUsername);
        }

        public Task<string> GetPasswordHashAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user == null
                ? throw new ArgumentNullException(nameof(user))
                : Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetUserIdAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user == null
                ? throw new ArgumentNullException(nameof(user))
                : Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user == null
                ? throw new ArgumentNullException(nameof(user))
                : Task.FromResult(user.Username);
        }

        public Task<bool> HasPasswordAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return user == null
                ? throw new ArgumentNullException(nameof(user))
                : Task.FromResult(user.PasswordHash != null);
        }

        public Task SetNormalizedUserNameAsync(UserEntity user, string normalizedName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.NormalisedUsername = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(UserEntity user, string passwordHash, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(UserEntity user, string userName, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.Username = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(UserEntity user, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            _context.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            _context.Update(user);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Code = "concurrencyError",
                    Description = "Failed to update user",
                });
            }
            return IdentityResult.Success;
        }

        public async Task AddToRoleAsync(UserEntity user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }
            var roleEntity = await FindRoleByNameAsync(roleName, cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException("roleNotFound");
            }
            user.RoleId = roleEntity.Id;
        }

        public Task RemoveFromRoleAsync(UserEntity user, string roleName, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IList<string>> GetRolesAsync(UserEntity user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var role = await FindRoleByIdAsync(user.RoleId, cancellationToken);
            return new List<string>
            {
                role.Name,
            };
        }

        public async Task<bool> IsInRoleAsync(UserEntity user, string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }
            var role = await FindRoleByNameAsync(roleName, cancellationToken);
            return role != null && user.RoleId == role.Id;
        }

        public async Task<IList<UserEntity>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentNullException(nameof(roleName));
            }

            var role = await FindRoleByNameAsync(roleName, cancellationToken);

            return role != null
                ? await Users.Where(u => u.RoleId == role.Id).ToListAsync(cancellationToken)
                : new List<UserEntity>();
        }

        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
        }

        private async Task<RoleEntity> FindRoleByNameAsync(string roleName, CancellationToken cancellationToken) =>
            await RolesSet.SingleOrDefaultAsync(r => r.NormalisedName == roleName, cancellationToken);

        private async Task<RoleEntity> FindRoleByIdAsync(Guid roleId, CancellationToken cancellationToken) =>
            await RolesSet.FindAsync(new object[] { roleId }, cancellationToken);
    }
}
