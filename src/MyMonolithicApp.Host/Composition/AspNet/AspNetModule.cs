using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using MyMonolithicApp.Application.Auth;

namespace MyMonolithicApp.Host.Composition.AspNet
{
    public static class AspNetModule
    {
        public static IServiceCollection AddAspNetCoreServices(this IServiceCollection services)
        {
            services
                .AddCors(options => options.AddDefaultPolicy(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()))
                .AddControllers()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddFluentValidation(fv =>
                {
                    fv.DisableDataAnnotationsValidation = true;
                    fv.RegisterValidatorsFromAssemblyContaining<LoginCommandValidator>();
                })
                .AddJsonOptions(opts =>
                {
                    opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            return services;
        }
    }
}
