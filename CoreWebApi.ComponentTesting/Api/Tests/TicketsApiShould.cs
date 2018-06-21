using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Api.ThirdParty;
using Api.Tickets;
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
            var taskUrl = "http://localhost:5000/api/tasks/" + Guid.NewGuid();
            _fixture.TasksApiBehavior
                .Expect(HttpMethod.Post, $"http://localhost:5000/api/tasks")
                .With(taskApiRequest => taskApiRequest.Content.As<NewTask>().Request.Equals("Something"))
                .Respond(new List<KeyValuePair<string, string>>()
                {
                    new KeyValuePair<string, string>("Location", taskUrl)
                },"application/json", "");


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

            var receivedEmail = _fixture.SmtpServer.ReceivedEmail.FirstOrDefault();
            Assert.NotNull(receivedEmail);
            Assert.Equal("noreply@localhost.com", receivedEmail.From.Address);
            Assert.Equal("administrators@company.com", receivedEmail.To.First().Address);
            Assert.Equal("New ticket created", receivedEmail.Subject);
            Assert.Equal($"Please check the created ticket at {taskUrl}", receivedEmail.Body);
        }
    }
}
