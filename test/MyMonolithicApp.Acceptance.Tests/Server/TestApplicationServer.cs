using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MyMonolithicApp.Host;
using MyMonolithicApp.Infrastructure.Data;
using AppHost = Microsoft.Extensions.Hosting.Host;

namespace MyMonolithicApp.Acceptance.Tests.Server
{
    public class TestApplicationServer : WebApplicationFactory<Startup>
    {
        private readonly IConfiguration _config;
        private const string _environment = "Development";
        private ICompositeService? _compositeService;

        public TestApplicationServer()
        {
            _config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{_environment}.json")
                .AddJsonFile("appsettings.test.json", true)
                .Build();

            SetupDatabaseContainer();
        }

        public Func<HttpClient> CreateClientWithCustomisations(IEnumerable<Action<HttpClient>> customisations) =>
            () =>
            {
                var client = CreateClient();
                foreach (var customisation in customisations)
                {
                    customisation(client);
                }
                return client;
            };

        public IServiceScope GetScope()
        {
            if (Server == null) // Need to create a client before start using services
            {
                CreateClient();
            }
            return Server!.Services.CreateScope();
        }

        protected override IHostBuilder CreateHostBuilder() => AppHost.CreateDefaultBuilder()
            .ConfigureAppConfiguration(builder => builder.AddConfiguration(_config))
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>())
            .UseEnvironment(_environment);

        protected override void ConfigureClient(HttpClient client)
        {
            base.ConfigureClient(client);
            client.DefaultRequestHeaders.Add("ConversationId", "dsf");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _compositeService!.Dispose();
            }
        }

        private void SetupDatabaseContainer()
        {
            Console.WriteLine("Starting Docker container with SQL database");

            var dockerFile = _config["DockerComposeFile"];
            _compositeService = new Builder()
                .UseContainer()
                .WaitForPort("1533/tcp", 25000)
                .UseCompose()
                .FromFile(dockerFile)
                .RemoveOrphans()
                .Build()
                .Start();

            Console.WriteLine("Starting migrating SQL database to latest version");

            var connectionString = _config["ConnectionStrings:DefaultConnection"];
            var serviceProvider = new ServiceCollection()
                .AddDbContextPool<ApplicationDbContext>(options => options.UseSqlServer(
                    _config.GetConnectionString("DefaultConnection"),
                    x => x.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)))
                .BuildServiceProvider(false);

            var retries = 20;
            while (retries > 0)
            {
                using var scope = serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    dbContext.Database.Migrate();
                    Console.WriteLine("Migration succeed!");
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Migration failed, trying again in 1s. Error: {ex.Message}");
                    Thread.Sleep(1000);
                    retries--;
                }
            }
        }
    }
}
