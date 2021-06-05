using System;
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
    public class ErrorLoggingMiddlewareTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<RequestDelegate> _next = new();
        private readonly ITestLoggerFactory _loggerFactory = TestLoggerFactory.Create();
        private readonly ErrorLoggingMiddleware _sut;

        public ErrorLoggingMiddlewareTests()
        {
            _sut = new ErrorLoggingMiddleware(_next.Object,
                _loggerFactory.CreateLogger<ErrorLoggingMiddleware>());
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
        public async Task Invoke_GivenExceptionIsThrown_ShouldLogException()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var error = new Exception(_fixture.Create<string>());
            _next
                .Setup(next => next(context))
                .Throws(error);

            // Act
            await _sut.Invoke(context);

            // Assert
            _loggerFactory.Sink.LogEntries.Should()
                .HaveCount(1)
                .And
                .SatisfyRespectively(log =>
                {
                    log.Message.Should().Be(error.Message);
                    log.LogLevel.Should().Be(LogLevel.Error);
                    log.Exception.Should().Be(error);
                });
        }
    }
}
