using System;
using System.Reflection;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MyMonolithicApp.Infrastructure.Auth;
using MyMonolithicApp.Infrastructure.Data;
using MyMonolithicApp.Infrastructure.Data.Entities;
using PermissionBasedAuthorisation;
using Xunit;

namespace MyMonolithicApp.Infrastructure.Tests.Auth
{
    public class PermissionVerificationServiceTests
    {
        private readonly PermissionVerificationService _sut;
        private readonly ApplicationDbContext _dbContext = MockHelpers.MockApplicationDbContext();
        private readonly IFixture _fixture = new Fixture();

        public PermissionVerificationServiceTests()
        {
            _sut = new PermissionVerificationService(_dbContext);
        }

        [Fact]
        public async Task ChallengePermissionAsync_GivenUserRoleWithPermission_ShouldReturnTrue()
        {
            // Arrange
            var roleId = _fixture.Create<Guid>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, roleId.ToString()) }, "test"));
            var requirement = GetPermissionRequirement();

            await SetupDbContextAsync(roleId, requirement.Permission);

            // Act
            var result = await _sut.ChallengePermissionAsync(claimsPrincipal, requirement);

            // Assert 
            result.Should().BeTrue();
        }

        [Fact]
        public async Task ChallengePermissionAsync_GivenUserRoleWithoutPermission_ShouldReturnFalse()
        {
            // Arrange
            var roleId = _fixture.Create<Guid>();
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(ClaimTypes.Role, roleId.ToString()) }, "test"));
            var requirement = GetPermissionRequirement();
            var otherPermission = _fixture.Create<string>();

            await SetupDbContextAsync(roleId, otherPermission);

            // Act
            var result = await _sut.ChallengePermissionAsync(claimsPrincipal, requirement);

            // Assert 
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ChallengePermissionAsync_GivenUnauthenticatedIdentity_ShouldReturnFalse()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity());
            var requirement = GetPermissionRequirement();

            // Act
            var result = await _sut.ChallengePermissionAsync(claimsPrincipal, requirement);

            // Assert 
            result.Should().BeFalse();
        }

        [Fact]
        public async Task ChallengePermissionAsync_GivenIdentityMissing_ShouldReturnFalse()
        {
            // Arrange
            var claimsPrincipal = new ClaimsPrincipal();
            var requirement = GetPermissionRequirement();

            // Act
            var result = await _sut.ChallengePermissionAsync(claimsPrincipal, requirement);

            // Assert 
            result.Should().BeFalse();
        }

        private async Task SetupDbContextAsync(Guid roleId, string otherPermission)
        {
            var permission = _fixture.Build<PermissionEntity>()
                .Without(x => x.Roles)
                .With(x => x.Code, otherPermission)
                .Create();
            var role = _fixture.Build<RoleEntity>()
                .With(x => x.Id, roleId)
                .With(x => x.Permissions, new[]
                {
                    new RolePermissionEntity
                    {
                        Permission = permission
                    }
                })
                .Create();

            _dbContext.Roles.Add(role);
            await _dbContext.SaveChangesAsync();
        }

        private PermissionRequirement GetPermissionRequirement()
        {
            return (PermissionRequirement)typeof(PermissionRequirement).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0]
                .Invoke(new object[] { _fixture.Create<string>() });
        }
    }
}
