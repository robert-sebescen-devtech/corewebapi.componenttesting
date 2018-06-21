namespace Api.Configuration
{
    public class MailerConfiguration
    {
        public string Host { get; }
        public int Port { get; }

        public MailerConfiguration(string host, int port)
        {
            Host = host;
            Port = port;
        }
    }
}