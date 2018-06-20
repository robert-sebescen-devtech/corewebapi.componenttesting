using System;

namespace Api.Tickets
{
    public class Ticket
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Type { get; set; }
        public string Text { get; set; }
    }
}