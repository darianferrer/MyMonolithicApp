using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using MyMonolithicApp.Domain.Exceptions;
using Severity = MyMonolithicApp.Domain.Exceptions.Severity;

namespace MyMonolithicApp.Host.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var (errors, statusCode) = HandleError(ex);
                await WriteErrorResponseAsync(context, errors, statusCode);
            }
        }

        private static (IEnumerable<Error> errors, int statusCode) HandleError(Exception ex)
        {
            ICollection<Error> errors;
            int statusCode;

            if (ex.GetBaseException() is ExceptionBase baseException)
            {
                statusCode = (int)baseException.StatusCode;
                errors = baseException.Errors.ToList();
            }
            else
            {
                (errors, statusCode) = ex switch
                {
                    ValidationException validationException => (
                        validationException.Errors.Select(e => new Error(Severity.Correctable, e.ErrorCode, e.ErrorMessage)).ToList(),
                        StatusCodes.Status400BadRequest
                    ),
                    AggregateException aggregateException => (
                        aggregateException.InnerExceptions.Select(HandleError).SelectMany(e => e.errors).ToList(),
                        StatusCodes.Status500InternalServerError
                    ),
                    _ => (
                        GetInnerExceptionIfExists(ex),
                        StatusCodes.Status500InternalServerError
                    )
                };
            }

            return (errors, statusCode);
        }

        private static ICollection<Error> GetInnerExceptionIfExists(Exception ex)
        {
            var errors = new List<Error>
            {
                new Error(Severity.Unexpected, ex.GetType().Name, ex.Message),
            };
            if (ex.InnerException != null)
            {
                var (innerErrors, _) = HandleError(ex.InnerException);
                errors.AddRange(innerErrors);
            }
            return errors;
        }

        private static Task WriteErrorResponseAsync(HttpContext context,
            IEnumerable<Error> errors,
            int statusCode = StatusCodes.Status500InternalServerError)
        {
            var response = context.Response;
            response.StatusCode = statusCode;
            response.ContentType = System.Net.Mime.MediaTypeNames.Application.Json;
            return response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
            {
                Errors = errors.ToList(),
            }));
        }
    }
}
