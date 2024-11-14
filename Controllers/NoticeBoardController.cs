using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using auth_app_backend.Model;
using auth_app_backend.Data;
using auth_app_backend.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace auth_app_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeBoardController : ControllerBase
    {
        private readonly NoticeBoardContext _context;
        private readonly IHubContext<NoticeBoardHub> _hubContext;

        public NoticeBoardController(NoticeBoardContext context, IHubContext<NoticeBoardHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages.OrderByDescending(m => m.Timestamp).ToListAsync();
            return Ok(messages);
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            message.Timestamp = DateTime.UtcNow;
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.Sender, message.Receiver, message.Text, message.Timestamp);

            return Ok(new { Message = "Message sent successfully" });
        }
    }
}