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
        public HttpMessageHandler Handler { get; }

        public TasksConfiguration(string baseUrl, HttpMessageHandler handler = null)
        {
            BaseUrl = baseUrl;
            Handler = handler;
        }
    }
}
