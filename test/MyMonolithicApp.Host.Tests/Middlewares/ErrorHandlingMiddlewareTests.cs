﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Moq;
using MyMonolithicApp.Domain.Exceptions;
using MyMonolithicApp.Host.Middlewares;
using Newtonsoft.Json;
using Xunit;
using Severity = MyMonolithicApp.Domain.Exceptions.Severity;

namespace MyMonolithicApp.Host.Tests.Middlewares
{
    public class ErrorHandlingMiddlewareTests
    {
        private readonly IFixture _fixture = new Fixture();
        private readonly Mock<RequestDelegate> _next = new();
        private readonly ErrorHandlingMiddleware _sut;

        public ErrorHandlingMiddlewareTests()
        {
            _sut = new ErrorHandlingMiddleware(_next.Object);
        }

        [Fact]
        public async Task Invoke_ShouldPassContextToNextDelegate()
        {
            // Arrange
            var context = new DefaultHttpContext();

            // Act
            await _sut.Invoke(context);

            // Assert
            _next.Verify(next => next(context));
        }

        [Fact]
        public async Task Invoke_GivenExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHttpContext();
            _next
                .Setup(d => d(context))
                .Throws<Exception>();

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Unexpected, "Exception", "Exception of type 'System.Exception' was thrown."));
        }

        [Fact]
        public async Task Invoke_GivenValidationExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHttpContext();
            var propertyName = _fixture.Create<string>();
            var errorMessage = _fixture.Create<string>();
            var errorCode = _fixture.Create<string>();
            _next
                .Setup(d => d(context))
                .Throws(new ValidationException(new[]
                {
                    new FluentValidation.Results.ValidationFailure(propertyName, errorMessage)
                    {
                        ErrorCode = errorCode
                    }
                }));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Correctable, errorCode, errorMessage));
        }

        [Fact]
        public async Task Invoke_GivenServiceClientExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHttpContext();
            var expectedMessage = _fixture.Create<string>();
            var expectedStatusCode = _fixture.Create<HttpStatusCode>();
            _next
                .Setup(d => d(context))
                .Throws(new ServiceClientException(expectedMessage, expectedStatusCode));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
            GetResponse(context)
                .Errors
                .First()
                .Should()
                .BeEquivalentTo(new Error(Severity.Unexpected, nameof(ServiceClientException), expectedMessage));
        }

        [Fact]
        public async Task Invoke_GivenAggregateExceptionIsThrown_ShouldWriteToResponse()
        {
            // Arrange
            var context = GetHttpContext();
            var expectedStatusCode = HttpStatusCode.InternalServerError;
            var expectedErrors = new Exception[]
            {
                new Exception("Error 1"),
                new Exception("Error 2")
            };
            _next
                .Setup(d => d(It.IsAny<HttpContext>()))
                .Throws(new AggregateException(expectedErrors));

            // Act
            await _sut.Invoke(context);

            // Assert
            context.Response.StatusCode.Should().Be((int)expectedStatusCode);
            GetResponse(context)
                .Errors
                .Should()
                .Contain((error) => expectedErrors.Any(e => e.Message == error.Detail));
        }

        private static HttpContext GetHttpContext()
        {
            var context = new DefaultHttpContext();
            var responseBody = new MemoryStream();
            context.Response.Body = responseBody;
            return context;
        }

        private static ErrorResponse GetResponse(HttpContext context)
        {
            var responseBody = context.Response.Body;
            responseBody.Seek(0, SeekOrigin.Begin);
            var response = new StreamReader(responseBody).ReadToEnd();
            return JsonConvert.DeserializeObject<ErrorResponse>(response);
        }

        private class ErrorResponse
        {
            public IEnumerable<Error> Errors { get; set; } = null!;
        }
    }
}
