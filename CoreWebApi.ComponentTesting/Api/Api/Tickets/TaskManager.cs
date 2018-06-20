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
        static HttpClient client = new HttpClient();

        public TaskManager(TasksConfiguration configuration)
        {
            _baseUrl = configuration.BaseUrl;
        }
        internal async Task<Uri> Create(Ticket ticket)
        {
            var request = Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                Request = ticket.Text
            });

            HttpResponseMessage response = await client.PostAsync(
                $"{_baseUrl}/api/tasks", new StringContent(request));
            response.EnsureSuccessStatusCode();
            
            return response.Headers.Location;
        }
    }
}