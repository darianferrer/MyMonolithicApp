using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Infrastructure.Data.Entities;

namespace MyMonolithicApp.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<UserEntity> Users { get; set; }
        public DbSet<RoleEntity> Roles { get; set; }
        public DbSet<PermissionEntity> Permissions { get; set; }
        public DbSet<RolePermissionEntity> RolesPermissions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(GetType().Assembly);

            SeedData(modelBuilder);
        }

        private static void SeedData(ModelBuilder modelBuilder)
        {
            var passwordHasher = new PasswordHasher<UserEntity>();
            var lookupNormalizer = new UpperInvariantLookupNormalizer();

            var canManageUsersPermission = new PermissionEntity
            {
                Code = PermissionCodes.CanManageUsers,
                Name = "Can manage users",
            };
            var adminRole = new RoleEntity
            {
                Id = Guid.NewGuid(),
                Name = "Admin",
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            adminRole.NormalisedName = lookupNormalizer.NormalizeName(adminRole.Name);
            var rolePermission = new RolePermissionEntity
            {
                PermissionCode = canManageUsersPermission.Code,
                RoleId = adminRole.Id,
            };
            var adminUser = new UserEntity
            {
                Id = Guid.NewGuid(),
                Email = "admin@test.org",
                Username = "admin",
                RoleId = adminRole.Id,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
            };
            adminUser.NormalisedUsername = lookupNormalizer.NormalizeName(adminUser.Username);
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Abcd*1234");

            modelBuilder.Entity<PermissionEntity>()
                .HasData(canManageUsersPermission);
            modelBuilder.Entity<RoleEntity>()
                .HasData(adminRole);
            modelBuilder.Entity<RolePermissionEntity>()
                .HasData(rolePermission);
            modelBuilder.Entity<UserEntity>()
                .HasData(adminUser);
        }
    }
}
