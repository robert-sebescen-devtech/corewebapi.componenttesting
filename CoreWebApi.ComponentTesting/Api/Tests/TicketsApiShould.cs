using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api;
using Api.Configuration;
using Api.Tickets;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
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

            var responseData = await response.Content.ReadAsStringAsync();
            var createdTicket = Newtonsoft.Json.JsonConvert.DeserializeObject<Ticket>(responseData);

            var dbTicket = _fixture.TestDb.GetCollection<Ticket>("tickets").FindSync(ticket => ticket.Id.Equals(createdTicket.Id)).FirstOrDefault();
            Assert.NotNull(dbTicket);
            Assert.Equal("Normal", dbTicket.Type);
            Assert.Equal("Something", dbTicket.Text);
        }
    }

    public class ApiFixture : IDisposable
    {
        private TestServer _server;
        private MongoDbRunner mongoDbRunner;
        
        public IMongoDatabase TestDb { get; set; }
        public HttpMessageHandler Handler;

        public ApiFixture()
        {
            StartMongo2Go();
            var taskConfiguration = new TasksConfiguration("http://localhost:5000");
            var mongoConfiguration = new MongoConfiguration(mongoDbRunner.ConnectionString, "Api");

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddSingleton(mongoConfiguration);
                    configureServices.AddSingleton(taskConfiguration);
                });
            _server = new TestServer(builder);
            Handler = _server.CreateHandler();
        }

        private void StartMongo2Go()
        {
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "mongodb/data/");
            mongoDbRunner = MongoDbRunner.Start(dataDirectory);

            var mongoClient = new MongoClient(mongoDbRunner.ConnectionString);
            TestDb = mongoClient.GetDatabase("Api");
        }

        public void Dispose()
        {
            Handler.Dispose();
            _server.Dispose();
            mongoDbRunner.Dispose();
        }
    }
}
