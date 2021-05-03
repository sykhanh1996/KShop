using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using KShop.BackendServer.Data;
using KShop.ViewModels.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KShop.BackendServer.Controllers
{
    public class CommandsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public CommandsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCommands()
        {
            var user = User.Identity.Name;
            var commands = _context.Commands;

            var commandVms = await commands.Select(u => new CommandVm()
            {
                Id = u.Id,
                Name = u.Name,
            }).ToListAsync();

            return Ok(commandVms);
        }
    }
}
