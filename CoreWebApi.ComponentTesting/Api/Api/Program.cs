using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Api.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args)
        {
            var mongoConfiguration = new MongoConfiguration("mongodb://localhost:27017", "Api");
            var tasksConfiguration = new TasksConfiguration("http://localhost:34127");
            var mailerConfiguration = new MailerConfiguration("smtp.domain.com", 25);

            return WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .ConfigureServices(configureServices =>
                {
                    configureServices.AddSingleton(mongoConfiguration);
                    configureServices.AddSingleton(tasksConfiguration);
                    configureServices.AddSingleton(mailerConfiguration);
                })
                .Build();
        }
    }
}
