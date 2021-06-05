using System.Net.Http;

namespace MyMonolithicApp.Acceptance.Tests.Features
{
    public interface IHttpRequestContext : ITestContext
    {
        HttpResponseMessage HttpResponseMessage { get; }
    }

    public record HttpRequestContext(HttpResponseMessage HttpResponseMessage) : IHttpRequestContext;
}
