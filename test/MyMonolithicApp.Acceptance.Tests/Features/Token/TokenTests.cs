using System;
using System.Threading.Tasks;
using MyMonolithicApp.Acceptance.Tests.Server;
using Xunit;

namespace MyMonolithicApp.Acceptance.Tests.Features.Token
{
    [Collection(nameof(TestApplicationServer))]
    public class TokenTests : IDisposable
    {
        private readonly Scenario _startingScenario;

        public TokenTests(TestApplicationServer server)
        {
            _startingScenario = new Scenario(server);
        }

        [Fact]
        public async Task GivenUserInDatabase_WhenSubmitValidCredentials_ThenReceiveToken()
        {
            await _startingScenario.Given_UserInDatabase()
                .When_SubmitValidCredentials()
                .Then_ReceiveToken();
        }

        [Fact]
        public async Task GivenUserInDatabase_WhenSubmitInvalidCredentials_ThenReceiveBadRequest()
        {
            await _startingScenario.Given_UserInDatabase()
                .When_SubmitInvalidCredentials()
                .Then_ReceiveBadRequest();
        }

        public void Dispose()
        {
            _startingScenario.CleanDatabase();
            _startingScenario.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
