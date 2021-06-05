using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MyMonolithicApp.Host.AppSettings;
using MyMonolithicApp.Host.Middlewares;
using Xunit;

namespace MyMonolithicApp.Host.Tests.Middlewares
{
    public class DiagnosticsMiddlewareTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly MetaSettings _appMetadata;
        private readonly Mock<RequestDelegate> _next = new();
        private readonly DiagnosticsMiddleware _sut;

        public DiagnosticsMiddlewareTests()
        {
            _appMetadata = _fixture.Build<MetaSettings>()
                .Without(x => x.Contact)
                .Create();
            _sut = new DiagnosticsMiddleware(_next.Object, _appMetadata);
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
        public async Task Invoke_GivenRequestUriIsEqualToDiagnosticsPath_ShouldReturnAppMetadata()
        {
            // Arrange
            var context = GetHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            var responseBody = context.Response.Body;
            responseBody.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(responseBody).ReadToEnd();
            var appMetadata = JsonSerializer.Deserialize<MetaSettings>(response);
            appMetadata.Should().NotBeNull().And.BeEquivalentTo(_appMetadata);
        }

        [Fact]
        public async Task Invoke_GivenRequestUriIsEqualToDiagnosticsPath_ShouldNotPassContextToNextDelegate()
        {
            // Arrange
            var context = GetHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            _next.Verify(next => next(context), Times.Never);
        }

        private static HttpContext GetHttpContext()
        {
            var context = new DefaultHttpContext();
            context.Request.Path = "/version";
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            return context;
        }
    }
}
