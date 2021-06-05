using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using MELT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using MyMonolithicApp.Host.Middlewares;
using Xunit;

namespace MyMonolithicApp.Host.Tests.Middlewares
{
    public class LoggingMiddlewareTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<RequestDelegate> _next = new();
        private readonly ITestLoggerFactory _loggerFactory = TestLoggerFactory.Create();
        private readonly LoggingMiddleware _sut;

        public LoggingMiddlewareTests()
        {
            _sut = new LoggingMiddleware(_next.Object, _loggerFactory.CreateLogger<LoggingMiddleware>());
        }

        [Fact]
        public async Task Invoke_ShouldPassContextToNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            _next.Verify(next => next(context), Times.Once);
        }

        [Fact]
        public async Task Invoke_GivenConversationIdHeader_ShouldCreateLoggingScopeWithThatHeaderValue()
        {
            // Arrange
            var conversationId = _fixture.Create<string>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Add("ConversationId", conversationId);

            // Act
            await _sut.Invoke(context);

            // Assert
            _loggerFactory.Sink.Scopes.Should()
                .HaveCount(1)
                .And
                .SatisfyRespectively(scope =>
                {
                    scope.Properties.Should().Contain(new KeyValuePair<string, object>("conversationid", conversationId));
                });
        }

        [Fact]
        public async Task Invoke_GivenIpAddressInRequest_ShouldCreateLoggingScopeWithThatIpValue()
        {
            // Arrange
            var ip = 16777343; // 127.0.0.1
            var context = new DefaultHttpContext();
            context.Connection.RemoteIpAddress = new System.Net.IPAddress(ip);

            // Act
            await _sut.Invoke(context);

            // Assert
            _loggerFactory.Sink.Scopes.Should()
                .HaveCount(1)
                .And
                .SatisfyRespectively(scope =>
                {
                    scope.Properties.Should().Contain(new KeyValuePair<string, object>("clientip", "127.0.0.1"));
                });
        }
    }
}
