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
        private readonly TaskManager _taskManager;
        public TicketsController(TicketsRepository tickets, TaskManager taskManager)
        {
            _tickets = tickets;
            _taskManager = taskManager;
        }
        [Route("{id}")]
        public ActionResult Get(Guid id)
        {
            return new JsonResult(_tickets.Get(id));
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] Ticket ticket)
        {
            _tickets.Create(ticket);
            //await _taskManager.Create(ticket);
            return Created($"/api/tickets/{ticket.Id}", ticket);
        }
    }
}