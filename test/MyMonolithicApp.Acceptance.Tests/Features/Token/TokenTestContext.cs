using System.Net.Http;
using MyMonolithicApp.Acceptance.Tests.Contracts;

namespace MyMonolithicApp.Acceptance.Tests.Features.Token
{
    public record TokenSuccessResponseTestContext(string Username, string Email, HttpResponseMessage HttpResponseMessage)
        : HttpRequestContext(HttpResponseMessage);

    public record TokenFailResponseTestContext(ErrorResponse ErrorResponse, HttpResponseMessage HttpResponseMessage)
        : HttpRequestContext(HttpResponseMessage);

    public record TokenSubmitTestContext(string Username, string Password) : ITestContext;
}
