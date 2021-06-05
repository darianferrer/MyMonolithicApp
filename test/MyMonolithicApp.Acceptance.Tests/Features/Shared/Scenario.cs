using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using MyMonolithicApp.Acceptance.Tests.Server;

namespace MyMonolithicApp.Acceptance.Tests.Features
{
    public class Scenario : IDisposable
    {
        private bool _disposed;
        private readonly Lazy<IServiceScope> _scope;

        protected Guid ScenarioId { get; private set; }
        protected internal TestApplicationServer Server { get; private set; }
        protected internal IList<Action<HttpClient>> ClientCustomisations { get; private set; }

        public Scenario(TestApplicationServer server)
        {
            Server = server;
            ScenarioId = Guid.NewGuid();
            _scope = new Lazy<IServiceScope>(() => server.GetScope());
            ClientCustomisations = new List<Action<HttpClient>>
            {
                client => client.DefaultRequestHeaders.Add("x-scenario", ScenarioId.ToString())
            };
        }

        protected Scenario(Scenario scenario)
        {
            Server = scenario.Server;
            ScenarioId = scenario.ScenarioId;
            _scope = scenario._scope;
            ClientCustomisations = scenario.ClientCustomisations;
        }

        public Scenario<TContext> SetContext<TContext>(TContext context) where TContext : ITestContext =>
            new(this, context);

        public TService GetService<TService>() where TService : class =>
            _scope.Value.ServiceProvider.GetRequiredService<TService>();

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _scope.Value.Dispose();
            }
        }
    }

    public class Scenario<TContext> : Scenario
        where TContext : ITestContext
    {
        public TContext Context { get; }

        public Scenario(Scenario scenario, TContext context) : base(scenario)
        {
            Context = context;
        }
    }
}
