using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.Tickets
{
    [Route("api/tickets")]
    public class TicketsController : Controller
    {
        private readonly TicketsRepository _tickets;
        public TicketsController(TicketsRepository tickets)
        {
            _tickets = tickets;
        }
        [Route("{id}")]
        public ActionResult Get(Guid id)
        {
            return new JsonResult(_tickets.Get(id));
        }

        [HttpPost]
        public ActionResult Post([FromBody] Ticket ticket)
        {
            _tickets.Create(ticket);
            return Created($"/api/tickets/{ticket.Id}", ticket);
        }
    }
}