using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;

namespace MyMonolithicApp.Acceptance.Tests.Features
{
    public static class HttpRequestSteps
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            PropertyNameCaseInsensitive = true,
        };

        public static async Task<Scenario<TContext>> When_SendContent<TContext>(this Scenario scenario,
            string requestUri,
            HttpMethod method,
            HttpContent? content,
            Func<HttpResponseMessage, TContext> createContext)
            where TContext : IHttpRequestContext
        {
            var client = scenario.GetClient();
            var message = new HttpRequestMessage(method, requestUri)
            {
                Content = content,
            };
            var response = await client.SendAsync(message);
            return scenario.SetContext(createContext(response));
        }

        public static async Task<Scenario<TContext>> When_SendContentFromFile<TContext>(this Scenario scenario,
            string requestUri,
            HttpMethod method,
            string contentPath,
            Func<HttpResponseMessage, TContext> createContext)
            where TContext : IHttpRequestContext
        {
            var fileContent = await LoadContentFromFileAsync(contentPath);
            var content = new StringContent(fileContent, Encoding.UTF8, MediaTypeNames.Application.Json);
            return await scenario.When_SendContent(requestUri, method, content, createContext);
        }

        public static Task<Scenario<TContext>> When_SendContract<TContract, TContext>(this Scenario scenario,
            string requestUri,
            HttpMethod method,
            TContract contract,
            Func<HttpResponseMessage, TContext> createContext)
            where TContext : IHttpRequestContext
        {
            var content = new StringContent(JsonSerializer.Serialize(contract, _options),
                Encoding.UTF8,
                MediaTypeNames.Application.Json);
            return scenario.When_SendContent(requestUri, method, content, createContext);
        }

        public static Task<Scenario<TContext>> When_SendNoContent<TContext>(this Scenario scenario,
            string requestUri,
            HttpMethod method,
            Func<HttpResponseMessage, TContext> createContext)
            where TContext : IHttpRequestContext
            => scenario.When_SendContent(requestUri, method, null, createContext);

        public static void Then_ReceiveStatus<TContext>(this Scenario<TContext> scenario, HttpStatusCode statusCode)
            where TContext : IHttpRequestContext
            => scenario.Context.HttpResponseMessage.StatusCode.Should().Be(statusCode);

        public static async Task Then_ResponseMatches<TContext, TContract>(this Scenario<TContext> scenario,
            TContract expected)
            where TContext : IHttpRequestContext
        {
            var response = await GetResponseAsync<TContract>(scenario.Context.HttpResponseMessage);
            response.Should().BeEquivalentTo(expected);
        }

        private static HttpClient GetClient(this Scenario scenario) =>
            scenario.Server.CreateClientWithCustomisations(scenario.ClientCustomisations)();

        private static async Task<string> LoadContentFromFileAsync(string fileName) =>
            await File.ReadAllTextAsync(fileName);

        private static async Task<TContract> GetResponseAsync<TContract>(HttpResponseMessage message)
        {
            var responseAsString = await message.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(responseAsString)) throw new Exception("Response is not found for request");

            var response = JsonSerializer.Deserialize<TContract>(responseAsString, _options);
            return response == null
                ? throw new Exception("Response couldn't be deserialized")
                : response;
        }
    }
}
