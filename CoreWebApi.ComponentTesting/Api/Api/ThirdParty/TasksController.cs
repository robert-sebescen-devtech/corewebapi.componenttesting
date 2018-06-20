using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Api.ThirdParty
{
    [Route("api/tasks")]
    public class TasksController : Controller
    {
        [HttpPost]
        public IActionResult Post(NewTask newTask)
        {
            return Created($"api/tasks/{newTask.Id}", newTask);
        }
    }

    public class NewTask
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Request { get; set; }
    }
}