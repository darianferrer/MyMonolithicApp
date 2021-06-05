using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using MyMonolithicApp.Application.Auth;
using Xunit;

namespace MyMonolithicApp.Application.Tests.Auth
{
    public class LoginCommandValidatorTests
    {
        private readonly LoginCommandValidator _sut = new();
        private readonly IFixture _fixture = new Fixture();

        [Fact]
        public void Validate_GivenValidLoginCommand_ShouldReturnTrue()
        {
            // Arrange
            var cmd = new LoginCommand(_fixture.Create<string>(), _fixture.Create<string>());

            // Act
            var result = _sut.Validate(cmd);

            // Assert 
            result.IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void Validate_GivenInvalidUsername_ShouldReturnError(string username)
        {
            // Arrange
            var cmd = new LoginCommand(username, _fixture.Create<string>());

            // Act
            var result = _sut.Validate(cmd);

            // Assert 
            using var scope = new AssertionScope();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.AllBeEquivalentTo(new
            {
                ErrorMessage = "login.emptyUserName",
            });
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("    ")]
        public void Validate_GivenInvalidPassword_ShouldReturnError(string password)
        {
            // Arrange
            var cmd = new LoginCommand(_fixture.Create<string>(), password);

            // Act
            var result = _sut.Validate(cmd);

            // Assert 
            using var scope = new AssertionScope();
            result.IsValid.Should().BeFalse();
            result.Errors.Should().HaveCount(1).And.AllBeEquivalentTo(new
            {
                ErrorMessage = "login.emptyPassword",
            });
        }
    }
}
