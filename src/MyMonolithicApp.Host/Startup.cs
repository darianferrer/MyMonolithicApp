using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMonolithicApp.Host.Composition.AspNet;
using MyMonolithicApp.Host.Composition.MediatR;
using MyMonolithicApp.Host.Composition.Swagger;
using MyMonolithicApp.Host.Middlewares;
using MyMonolithicApp.Infrastructure;

namespace MyMonolithicApp.Host
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAspNetCoreServices();
            services.AddSwaggerServices(_configuration);
            services.AddMediatRServices();
            services.AddInfrastructure(_configuration);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint(new MigrationsEndPointOptions
                {
                    Path = "/migrate",
                });
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();

            app.UseMiddleware<LoggingMiddleware>();
            app.UseMiddleware<DiagnosticsMiddleware>();
            app.UseMiddleware<ErrorLoggingMiddleware>();
            app.UseMiddleware<ErrorHandlingMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.UseEndpoints(endpoints => endpoints.MapControllers().RequireAuthorization());
        }
    }
}
