using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Identity;
using Moq;
using MyMonolithicApp.Domain.Exceptions;
using MyMonolithicApp.Infrastructure.Auth;
using MyMonolithicApp.Infrastructure.Data.Entities;
using Xunit;

namespace MyMonolithicApp.Infrastructure.Tests.Auth
{
    public class SignInServiceTests
    {
        private readonly SignInService _sut;
        private readonly Mock<UserManager<UserEntity>> _userManager = MockHelpers.MockUserManager<UserEntity>();
        private readonly Mock<SignInManager<UserEntity>> _signInManager;
        private readonly IFixture _fixture = new Fixture();

        private static readonly Error _invalidCredentials = new(Severity.Correctable,
            "login.invalidCredentials",
            "Credentials are not valid");

        public SignInServiceTests()
        {
            _signInManager = MockHelpers.MockSignInManager(_userManager.Object);

            _sut = new SignInService(_signInManager.Object, _userManager.Object);
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        [Fact]
        public async Task SignInAsync_GivenValidCredentials_ShouldReturnTrue()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var password = _fixture.Create<string>();
            var user = _fixture.Create<UserEntity>();

            _userManager.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);
            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
                .ReturnsAsync(SignInResult.Success);

            // Act
            var result = await _sut.SignInAsync(username, password);

            // Assert 
            using var scope = new AssertionScope();
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().BeTrue();
        }

        [Fact]
        public async Task SignInAsync_GivenInvalidPassword_ShouldReturnError()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var password = _fixture.Create<string>();
            var user = _fixture.Create<UserEntity>();

            _userManager.Setup(x => x.FindByNameAsync(username))
                .ReturnsAsync(user);
            _signInManager.Setup(x => x.CheckPasswordSignInAsync(user, password, false))
                .ReturnsAsync(SignInResult.Failed);

            // Act
            var result = await _sut.SignInAsync(username, password);

            // Assert 
            using var scope = new AssertionScope();
            result.IsT0.Should().BeFalse();
            result.AsT1.Should().BeEquivalentTo(_invalidCredentials);
        }

        [Fact]
        public async Task SignInAsync_GivenUserNotFound_ShouldReturnError()
        {
            // Arrange
            var username = _fixture.Create<string>();
            var password = _fixture.Create<string>();

            // Act
            var result = await _sut.SignInAsync(username, password);

            // Assert 
            using var scope = new AssertionScope();
            result.IsT0.Should().BeFalse();
            result.AsT1.Should().BeEquivalentTo(_invalidCredentials);
        }
    }
}
