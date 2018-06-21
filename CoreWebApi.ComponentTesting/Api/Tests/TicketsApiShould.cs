using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Api;
using Api.Configuration;
using Api.ThirdParty;
using Api.Tickets;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using RichardSzalay.MockHttp;
using Xunit;

namespace Tests
{
    public class TicketsApiShould : IClassFixture<ApiFixture>
    {
        private readonly ApiFixture _fixture;

        public TicketsApiShould(ApiFixture fixture)
        {
            _fixture = fixture;
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
            _fixture.TasksApiBehavior
                .Expect(HttpMethod.Post, $"http://localhost:5000/api/tasks")
                .With(taskApiRequest => taskApiRequest.Content.As<NewTask>().Request.Equals("Something"))
                .Respond("application/json", "");


            HttpClient client = new HttpClient(_fixture.Handler);
            client.BaseAddress = new Uri("http://localhost:5000");
            HttpResponseMessage response = await client.PostAsync(
                "api/tickets", new StringContent(request, Encoding.Unicode, "application/json"));
            response.EnsureSuccessStatusCode();

            Ticket createdTicket = response.Content.As<Ticket>();

            var dbTicket = _fixture.TestDb.GetCollection<Ticket>("tickets").FindSync(ticket => ticket.Id.Equals(createdTicket.Id)).FirstOrDefault();
            Assert.NotNull(dbTicket);
            Assert.Equal("Normal", dbTicket.Type);
            Assert.Equal("Something", dbTicket.Text);
            _fixture.TasksApiBehavior.VerifyNoOutstandingExpectation();
        }
    }

    public class ApiFixture : IDisposable
    {
        private TestServer _server;
        private MongoDbRunner _mongoDbRunner;
        
        public IMongoDatabase TestDb { get; set; }
        public MockHttpMessageHandler TasksApiBehavior { get; set; }
        public HttpMessageHandler Handler;

        public ApiFixture()
        {
            StartMongo2Go();

            TasksApiBehavior = new MockHttpMessageHandler();

            StartApi();
        }

        private void StartApi()
        {
            var taskConfiguration = new TasksConfiguration("http://localhost:5000", TasksApiBehavior);
            var mongoConfiguration = new MongoConfiguration(_mongoDbRunner.ConnectionString, "Api");

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
            _mongoDbRunner = MongoDbRunner.Start(dataDirectory);

            var mongoClient = new MongoClient(_mongoDbRunner.ConnectionString);
            TestDb = mongoClient.GetDatabase("Api");
        }

        public void Dispose()
        {
            Handler.Dispose();
            _server.Dispose();
            _mongoDbRunner.Dispose();
        }
    }
}
