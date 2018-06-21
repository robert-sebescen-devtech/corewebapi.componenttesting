using System;
using System.Net.Mail;
using Api.Configuration;

namespace Api.Tickets
{
    public class Mailer
    {
        private readonly SmtpClient _client;
        public Mailer(MailerConfiguration mailerConfiguration)
        {
            _client = new SmtpClient(mailerConfiguration.Host, mailerConfiguration.Port)
            {
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false
            };
        }

        public void SendTask(Uri taskUri)
        {
            MailMessage mail = new MailMessage("noreply@localhost.com", "administrators@company.com");
            mail.Subject = "New ticket created";
            mail.Body = $"Please check the created ticket at {taskUri.AbsoluteUri}";
            _client.Send(mail);
        }
    }
}