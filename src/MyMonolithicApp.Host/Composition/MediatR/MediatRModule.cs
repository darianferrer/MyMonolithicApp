using MediatR;
using Microsoft.Extensions.DependencyInjection;
using MyMonolithicApp.Application.Auth;

namespace MyMonolithicApp.Host.Composition.MediatR
{
    public static class MediatRModule
    {
        public static IServiceCollection AddMediatRServices(this IServiceCollection services)
        {
            services.AddMediatR(typeof(LoginCommand).Assembly);
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationPipelineBehavior<,>));
            return services;
        }
    }
}
