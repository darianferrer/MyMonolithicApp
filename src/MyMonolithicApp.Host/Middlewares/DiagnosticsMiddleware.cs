using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MyMonolithicApp.Host.AppSettings;

namespace MyMonolithicApp.Host.Middlewares
{
    public class DiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly MetaSettings _metadata;
        private const string _endpoint = "/version";

        public DiagnosticsMiddleware(RequestDelegate next, MetaSettings metadata)
        {
            _next = next;
            _metadata = metadata;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments(_endpoint, System.StringComparison.CurrentCultureIgnoreCase))
            {
                var content = System.Text.Json.JsonSerializer.Serialize(_metadata);
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync(content);
            }
            else
            {
                await _next(context);
            }
        }
    }
}
