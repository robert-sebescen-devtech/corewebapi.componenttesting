using System;
using System.Net.Http;
using System.Threading.Tasks;
using Api.Configuration;
using Api.ThirdParty;

namespace Api.Tickets
{
    public class TaskManager
    {
        private readonly string _baseUrl;
        static HttpClient _client;

        public TaskManager(TasksConfiguration configuration)
        {
            _baseUrl = configuration.BaseUrl;
            _client = configuration.Client ?? new HttpClient();
        }
        internal async Task<Uri> Create(string taskDescription)
        {
            var request = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                TaskDescription = taskDescription
            });

            HttpResponseMessage response = await _client.PostAsync(
                $"{_baseUrl}/api/tasks", new StringContent(request));
            response.EnsureSuccessStatusCode();
            
            return response.Headers.Location;
        }
    }
}