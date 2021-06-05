using System;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MyMonolithicApp.Host.AppSettings;

namespace MyMonolithicApp.Host.Composition.Swagger
{
    public static class SwaggerModule
    {
        private static readonly MetaSettings _metadata = new();

        public static IServiceCollection AddSwaggerServices(this IServiceCollection services, IConfiguration config)
        {
            config.GetSection("Meta").Bind(_metadata);

            services.AddSingleton(_metadata);
            services.AddSwaggerGen(config =>
            {
                config.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please insert JWT with Bearer into field",
                    Type = SecuritySchemeType.Http,
                    Scheme = JwtBearerDefaults.AuthenticationScheme.ToLower(),
                });

                config.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = JwtBearerDefaults.AuthenticationScheme }
                        },
                        Array.Empty<string>()
                    }
                });

                config.SwaggerDoc(_metadata.Version, new OpenApiInfo
                {
                    Title = _metadata.Name,
                    Description = _metadata.Description,
                    Contact = _metadata.Contact,
                    Version = _metadata.Version,
                });
                config.OperationFilter<RequiredHeadersOperationFilter>();
            });
            return services;
        }

        public static void UseSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger(action => action.RouteTemplate = "docs/{documentName}/swagger.json");
            app.UseSwaggerUI(action =>
            {
                action.SwaggerEndpoint
                (
                    $"/docs/{_metadata.Version}/swagger.json",
                    $"{_metadata.Name} v{_metadata.Version}"
                );
                action.RoutePrefix = "docs";
            });
        }
    }
}
