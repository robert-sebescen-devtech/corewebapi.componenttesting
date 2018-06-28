using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Api.Configuration
{
    public class TasksConfiguration
    {
        public string BaseUrl { get; }
        public HttpClient Client { get; }

        public TasksConfiguration(string baseUrl, HttpClient client = null)
        {
            BaseUrl = baseUrl;
            Client = client;
        }
    }
}
