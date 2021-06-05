using System;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentAssertions.Execution;
using Moq;
using MyMonolithicApp.Application.Auth;
using MyMonolithicApp.Domain.Auth;
using MyMonolithicApp.Domain.Exceptions;
using MyMonolithicApp.Domain.Users;
using Xunit;

namespace MyMonolithicApp.Application.Tests.Auth
{
    public class LoginCommandHandlerTests
    {
        private readonly LoginCommandHandler _sut;
        private readonly Mock<IJwtTokenGenerator> _jwtTokenGenerator = new();
        private readonly Mock<ISignInService> _signInService = new();
        private readonly Mock<IUserService> _userService = new();
        private readonly IFixture _fixture = new Fixture();

        public LoginCommandHandlerTests()
        {
            _sut = new LoginCommandHandler(_jwtTokenGenerator.Object,
                _signInService.Object,
                _userService.Object);
        }

        [Fact]
        public async Task Handle_GivenValidCredentials_ShouldReturnUser()
        {
            // Arrange
            var request = _fixture.Create<LoginCommand>();
            var user = _fixture.Create<User>();
            var token = _fixture.Create<string>();

            _signInService.Setup(x => x.SignInAsync(request.Username, request.Password))
                .ReturnsAsync(true);
            _userService.Setup(x => x.FindByUsernameAsync(request.Username))
                .ReturnsAsync(user);
            _jwtTokenGenerator.Setup(x => x.CreateToken(user))
                .Returns(token);

            // Act
            var result = await _sut.Handle(request);

            // Assert 
            using var scope = new AssertionScope();
            result.IsT0.Should().BeTrue();
            result.AsT0.Should().BeEquivalentTo(new
            {
                user.Username,
                user.Email,
                Token = token,
            });
        }

        [Fact]
        public async Task Handle_GivenValidCredentialsAndUserNotFound_ShouldThrowException()
        {
            // Arrange
            var request = _fixture.Create<LoginCommand>();
            var user = _fixture.Create<User>();

            _signInService.Setup(x => x.SignInAsync(request.Username, request.Password))
                .ReturnsAsync(true);

            // Act
            Func<Task> act = () => _sut.Handle(request);

            // Assert 
            await act.Should()
                .ThrowAsync<Exception>()
                .WithMessage("Unexpected error, user not found after logged in");
        }

        [Fact]
        public async Task Handle_GivenInvalidCredentials_ShouldReturnError()
        {
            // Arrange
            var request = _fixture.Create<LoginCommand>();
            var error = _fixture.Create<Error>();

            _signInService.Setup(x => x.SignInAsync(request.Username, request.Password))
                .ReturnsAsync(error);

            // Act
            var result = await _sut.Handle(request);

            // Assert 
            using var scope = new AssertionScope();
            result.IsT0.Should().BeFalse();
            result.AsT1.Should().Be(error);
        }
    }
}
