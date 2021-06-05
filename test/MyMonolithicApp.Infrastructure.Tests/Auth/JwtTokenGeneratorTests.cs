using System.IdentityModel.Tokens.Jwt;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using MyMonolithicApp.Domain.Users;
using MyMonolithicApp.Infrastructure.Auth;
using Xunit;

namespace MyMonolithicApp.Infrastructure.Tests.Auth
{
    public class JwtTokenGeneratorTests
    {
        private readonly JwtTokenGenerator _sut;
        private readonly Mock<SecurityTokenHandler> _tokenHandler = new();
        private readonly IFixture _fixture = new Fixture();
        private readonly JwtIssuerOptions _options;

        public JwtTokenGeneratorTests()
        {
            _options = _fixture.Build<JwtIssuerOptions>()
                .Without(x => x.SigningCredentials)
                .Create();
            _sut = new JwtTokenGenerator(Options.Create(_options), _tokenHandler.Object);
        }

        [Fact]
        public void CreateToken_ShouldReturnToken()
        {
            // Arrange
            var user = _fixture.Create<User>();
            var token = _fixture.Create<string>();

            _tokenHandler.Setup(x => x.WriteToken(It.IsAny<JwtSecurityToken>()))
                .Returns(token);

            // Act
            var result = _sut.CreateToken(user);

            // Assert 
            result.Should().Be(token);
        }
    }
}
