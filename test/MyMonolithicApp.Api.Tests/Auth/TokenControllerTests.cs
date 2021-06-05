using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using MyMonolithicApp.Api.Auth;
using MyMonolithicApp.Application.Auth;
using MyMonolithicApp.Domain.Exceptions;
using OneOf;
using Xunit;

namespace MyMonolithicApp.Api.Tests.Auth
{
    public class TokenControllerTests
    {
        private readonly TokenController _sut;
        private readonly Mock<IMediator> _mediator = new();
        private readonly IFixture _fixture = new Fixture();

        public TokenControllerTests()
        {
            _sut = new TokenController(_mediator.Object);
        }

        [Fact]
        public async Task Token_GivenValidCredentials_ShouldReturnLoginResponse()
        {
            // Arrange
            var cmd = _fixture.Create<LoginCommand>();
            var loginResponse = _fixture.Create<LoginResponse>();

            _mediator.Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OneOf<LoginResponse, Error>.FromT0(loginResponse));

            // Act
            var result = await _sut.ConnectAsync(cmd);

            // Assert 
            result.Should()
                .BeOfType<ActionResult<LoginResponse>>()
                .Which
                .Value
                .Should()
                .Be(loginResponse);
        }

        [Fact]
        public async Task Token_GivenInvalidCredentials_ShouldReturnError()
        {
            // Arrange
            var cmd = _fixture.Create<LoginCommand>();
            var error = _fixture.Create<Error>();

            _mediator.Setup(x => x.Send(cmd, It.IsAny<CancellationToken>()))
                .ReturnsAsync(OneOf<LoginResponse, Error>.FromT1(error));

            // Act
            var result = await _sut.ConnectAsync(cmd);

            // Assert 
            result.Result.Should()
                .BeOfType<BadRequestObjectResult>()
                .Which
                .Value
                .Should()
                .BeEquivalentTo(new
                {
                    Errors = new[] { error }
                });
        }
    }
}
