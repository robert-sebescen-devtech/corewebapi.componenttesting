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
            // ARRANGE
            var request = @"
            {
                ""type"":""Normal"",
                ""text"":""Something"",
            }
            ";
            var newTaskId = Guid.NewGuid();
            var createdTaskUri = $"{TasksApiBaseUrl}/api/tasks/{newTaskId}";

            // SETUP
            _fixture.TasksApiBehavior
                .Expect(HttpMethod.Post, $"{_fixture.TasksApiBaseUrl}/api/tasks")
                .With(taskApiRequest => taskApiRequest
                        .Content.As<NewTask>()
                        .TaskDescription.Equals("Something"))
                .Respond(
                    headers: new List<KeyValuePair<string, string>>()
                    {
                        new KeyValuePair<string, string>("Location", createdTaskUri)
                    },
                    mediaType: "application/json", 
                    content: ""
                );

            // ACT
            HttpClient client = new HttpClient(TestHostMessageHandler)
            {
                BaseAddress = TestHostBaseUri
            };
            HttpResponseMessage response = await client.PostAsync(
                "api/tickets", 
                new StringContent(
                    content: request, 
                    encoding: Encoding.Unicode, 
                    mediaType: "application/json"));
            
            // ASSERT
            response.EnsureSuccessStatusCode();
            Ticket createdTicket = response.Content.As<Ticket>();

            // ASSERT DB
            var dbTicket = _fixture
                .TestDb
                .GetCollection<Ticket>("tickets")
                .FindSync(ticket => ticket.Id.Equals(createdTicket.Id))
                .SingleOrDefault();
            Assert.NotNull(dbTicket);
            Assert.Equal("Normal", dbTicket.Type);
            Assert.Equal("Something", dbTicket.Text);

            // ASSERT TASKS API
            _fixture.TasksApiBehavior.VerifyNoOutstandingExpectation();
            
            // ASSERT SMTP
            Assert.Equal(1, _fixture.SmtpServer.ReceivedEmailCount);
            var receivedEmail = _fixture.SmtpServer.ReceivedEmail.SingleOrDefault();
            Assert.NotNull(receivedEmail);
            Assert.Equal("noreply@localhost.com", receivedEmail.From.Address);
            Assert.Equal("administrators@company.com", receivedEmail.To.First().Address);
            Assert.Equal("New ticket created", receivedEmail.Subject);
            Assert.Equal($"Please check the created ticket at {createdTaskUri}", receivedEmail.Body);
        }

        private Uri TestHostBaseUri
        {
            get { return _fixture.ApiBaseUrl; }
        }

        private HttpMessageHandler TestHostMessageHandler
        {
            get { return _fixture.Handler; }
        }

        private string TasksApiBaseUrl
        {
            get { return _fixture.TasksApiBaseUrl; }
        }
    }
}
