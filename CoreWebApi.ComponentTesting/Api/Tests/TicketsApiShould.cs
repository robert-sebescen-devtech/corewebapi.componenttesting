using Microsoft.AspNetCore.Hosting;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api;
using Api.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Tests
{
    public class TicketsApiShould : IClassFixture<ApiFixture>
    {
        private readonly ApiFixture _fixture;
        static HttpClient client;

        public TicketsApiShould(ApiFixture fixture)
        {
            _fixture = fixture;
            client = new HttpClient(_fixture.Handler);
            client.BaseAddress = new Uri("http://localhost:5000");
        }

        [Fact]
        public async Task CreateTicketAndTask()
        {
            var request = @"
            {
                ""type"":""Normal"",
                ""text"":""Something"",
            }
            ";

            HttpResponseMessage response = await client.PostAsync(
                "api/tickets", new StringContent(request, Encoding.Unicode, "application/json"));
            response.EnsureSuccessStatusCode();
        }
    }

    public class ApiFixture : IDisposable
    {
        private TestServer _server;

        public HttpMessageHandler Handler;

        public ApiFixture()
        {
            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddSingleton(new MongoConfiguration
                    {
                        ConnectionString = "mongodb://localhost:27017",
                        Database = "Api"
                    });
                    configureServices.AddSingleton(new TasksConfiguration
                    {
                        BaseUrl = "http://localhost:5000"
                    });
                });
            _server = new TestServer(builder);
            Handler = _server.CreateHandler();
        }

        public void Dispose()
        {
            Handler.Dispose();
            _server.Dispose();
        }
    }
}
