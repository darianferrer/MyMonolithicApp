using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Execution;
using MyMonolithicApp.Functional.Tests.Contracts;
using TechTalk.SpecFlow;

namespace MyMonolithicApp.Functional.Tests.StepDefinitions
{
    [Binding]
    public class TokenSteps
    {
        private const string _responseKey = "Response";
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        private readonly HttpClient _httpClient;
        private readonly ScenarioContext _scenarioContext;

        public TokenSteps(HttpClient httpClient, ScenarioContext scenarioContext)
        {
            _httpClient = httpClient;
            _scenarioContext = scenarioContext;
        }

        [Given("User exists in database")]
        public void GivenUseExistsInDatabase()
        {
            // TODO actually use a different user rather than admin
        }

        [When("I submit the user's credentials")]
        public async Task WhenISubmitTheUsersCredentials()
        {
            var contract = new LoginRequest("admin", "Abcd*1234");
            var request = new HttpRequestMessage(HttpMethod.Post, "/token/connect")
            {
                Content = new StringContent(JsonSerializer.Serialize(contract), Encoding.UTF8, MediaTypeNames.Application.Json),
            };
            var response = await _httpClient.SendAsync(request);
            _scenarioContext.Add(_responseKey, response);
        }

        [Then("I receive the token response")]
        public async Task ThenIReceiveTheTokenResponse()
        {
            var response = _scenarioContext.Get<HttpResponseMessage>(_responseKey);
            var content = await response.Content.ReadAsStringAsync();
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(content, _options);
            using var scope = new AssertionScope();
            loginResponse.Should().BeEquivalentTo(new
            {
                Username = "admin",
                Email = "admin@test.org"
            });
            loginResponse.Token.Should().NotBeNullOrWhiteSpace();
        }

        [When("I submit wrong user's credentials")]
        public void WhenISubmitWrongUsersCredentials()
        {

        }

        [Then("I receive an authentication failed error")]
        public void ThenIReceiveanAuthenticationFailedError()
        {

        }
    }
}
