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
        private MongoDbRunner _testMongoDb;
        
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
            var mongoConfiguration = new MongoConfiguration(_testMongoDb.ConnectionString, "Api");
            var tasksConfiguration = new TasksConfiguration(TasksApiBaseUrl, TasksApiBehavior.ToHttpClient());
            var mailerConfiguration = new MailerConfiguration(FakeSmtpHost, 25);

            var builder = new WebHostBuilder()
                .UseStartup<Startup>()
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddSingleton(mongoConfiguration);
                    configureServices.AddSingleton(tasksConfiguration);
                    configureServices.AddSingleton(mailerConfiguration);
                });
            _server = new TestServer(builder);
            Handler = _server.CreateHandler();
        }

        private static string FakeSmtpHost
        {
            get { return "localhost"; }
        }

        public string TasksApiBaseUrl => "http://localhost:5000";
        public Uri ApiBaseUrl => new Uri("http://localhost:5000");

        private void StartMongo2Go()
        {
            var dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "mongodb/data/");
            _testMongoDb = MongoDbRunner.Start(dataDirectory);

            var mongoClient = new MongoClient(_testMongoDb.ConnectionString);
            TestDb = mongoClient.GetDatabase("Api");
        }

        public void Dispose()
        {
            Handler.Dispose();
            _server.Dispose();
            _testMongoDb.Dispose();
            SmtpServer.Dispose();
        }
    }
}
