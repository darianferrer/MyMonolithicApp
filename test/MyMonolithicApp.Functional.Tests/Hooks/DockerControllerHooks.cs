using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using BoDi;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Configuration;
using TechTalk.SpecFlow;

namespace MyMonolithicApp.Functional.Tests.Hooks
{
    [Binding]
    public class DockerControllerHooks
    {
        private static ICompositeService? _compositeService;
        private readonly IObjectContainer _objectContainer;
        private static readonly IConfiguration _configuration;

        static DockerControllerHooks()
        {
            _configuration = LoadConfiguration();
        }

        private static string BaseAddress { get => _configuration["Service:BaseAddress"]; }

        public DockerControllerHooks(IObjectContainer objectContainer)
        {
            _objectContainer = objectContainer;
        }

        [BeforeTestRun]
        public static void DockerComposeUp()
        {
            Console.WriteLine("Starting Docker containers WebApi and SQL database");

            var filePath = GetDockerComposeFilePath();

            _compositeService = new Builder()
                .UseContainer()
                .UseCompose()
                .FromFile(filePath)
                .RemoveOrphans()
                .WaitForHttp("webapi",
                    $"{BaseAddress}/version",
                    continuation: (response, _) => response.Code != HttpStatusCode.OK ? 2000 : 0)
                .WaitForHttp("webapi",
                    $"{BaseAddress}/migrate",
                    method: HttpMethod.Post,
                    body: GetMigrateRequestBody(),
                    contentType: "application/x-www-form-urlencoded",
                    continuation: (response, _) => response.Code != HttpStatusCode.NoContent ? 2000 : 0)
                .Build()
                .Start();
        }

        [AfterTestRun]
        public static void DockerComposeDown()
        {
            if (_compositeService != null)
            {
                _compositeService.Stop();
                _compositeService.Dispose();
            }
        }

        [BeforeScenario]
        public void AddHttpClient()
        {
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseAddress),
            };
            httpClient.DefaultRequestHeaders.Add("ConversationId", "dsf");
            _objectContainer.RegisterInstanceAs(httpClient);
        }

        private static IConfiguration LoadConfiguration()
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build();
        }

        private static string GetDockerComposeFilePath()
        {
            var dockerComposeFileName = _configuration["DockerComposeFileName"];
            var currentDirectory = Directory.GetCurrentDirectory();
            while (!Directory.EnumerateFiles(currentDirectory, "*.yml").Any(s => s.EndsWith(dockerComposeFileName)))
            {
                currentDirectory = currentDirectory.Substring(0, currentDirectory.LastIndexOf(Path.DirectorySeparatorChar));
            }
            return Path.Combine(currentDirectory, dockerComposeFileName);
        }

        private static string GetMigrateRequestBody()
        {
            var requestBody = new FormUrlEncodedContent(new Dictionary<string?, string?>
            {
                {
                    "context",
                    @"MyMonolithicApp.Infrastructure.Data.ApplicationDbContext, MyMonolithicApp.Infrastructure, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null"
                },
            });

            var stream = requestBody.ReadAsStream();
            return new StreamReader(stream).ReadToEnd();
        }
    }
}
