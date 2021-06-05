using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MyMonolithicApp.Host.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        private const string _conversationId = "ConversationId";

        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var state = new Dictionary<string, object>();

            var headers = context.Request.Headers;

            if (headers.TryGetValue(_conversationId, out var conversationId))
            {
                state.Add(_conversationId.ToLower(), conversationId);
            }

            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            if (remoteIp != null)
            {
                state.Add("clientip", remoteIp);
            }

            using (_logger.BeginScope(state))
            {
                await _next(context);
            }
        }
    }
}
