using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyMonolithicApp.Infrastructure.Data.Entities;
using MyMonolithicApp.Infrastructure.UserManagement;
using Xunit;

namespace MyMonolithicApp.Infrastructure.Tests.Users
{
    public class UserServiceTests
    {
        private readonly UserService _sut;
        private readonly Mock<UserManager<UserEntity>> _userManager = MockHelpers.MockUserManager<UserEntity>();
        private readonly IFixture _fixture = new Fixture();

        public UserServiceTests()
        {
            _sut = new UserService(_userManager.Object);
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task FindByUsernameAsync_GivenUserExists_ShouldReturnUser()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var user = _fixture.Create<UserEntity>();

            _userManager.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _sut.FindByUsernameAsync(username);

            // Assert 
            result.Should().BeEquivalentTo(new
            {
                user.Id,
                user.Username,
                user.Email,
                Role = new
                {
                    user.Role.Id,
                    user.Role.Name,
                },
            });
        }

        [Fact]
        public async Task FindByUsernameAsync_GivenUserDoesNotExists_ShouldReturnNull()
        {
            // Arrange
            var username = _fixture.Create<string>();

            // Act
            var result = await _sut.FindByUsernameAsync(username);

            // Assert 
            result.Should().BeNull();
        }
    }
}
