using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MyMonolithicApp.Host;

Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>())
    .Build()
    .Run();
