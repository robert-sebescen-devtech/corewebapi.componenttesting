using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;
using System.Net.Http;
using Api;
using Api.Configuration;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Mongo2Go;
using MongoDB.Driver;
using nDumbster.Smtp;
using RichardSzalay.MockHttp;

namespace Tests
{
    public class ApiFixture : IDisposable
    {
        private TestServer _server;
        private MongoDbRunner _mongoDbRunner;
        
        public IMongoDatabase TestDb { get; set; }
        public MockHttpMessageHandler TasksApiBehavior { get; set; }
        public HttpMessageHandler Handler;
        public SimpleSmtpServer SmtpServer;

        public ApiFixture()
        {
            StartMongo2Go();
            StartSmtpServer();

            TasksApiBehavior = new MockHttpMessageHandler();

            StartApi();
        }

        private void StartSmtpServer()
        {
            SmtpServer = SimpleSmtpServer.Start();
        }

        private void StartApi()
        {
            var taskConfiguration = new TasksConfiguration("http://localhost:5000", TasksApiBehavior);
            var mongoConfiguration = new MongoConfiguration(_mongoDbRunner.ConnectionString, "Api");
            var mailerConfiguration = new MailerConfiguration("localhost", 25);

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddSingleton(mongoConfiguration);
                    configureServices.AddSingleton(taskConfiguration);
                    configureServices.AddSingleton(mailerConfiguration);
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
            SmtpServer.Dispose();
        }
    }
}
