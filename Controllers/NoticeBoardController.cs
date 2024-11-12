using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using auth_app_backend.Model;
using auth_app_backend.Data;

namespace auth_app_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeBoardController : ControllerBase
    {
        private readonly NoticeBoardContext _context;

        public NoticeBoardController(NoticeBoardContext context)
        {
            _context = context;
        }

        // GET: api/noticeboard/messages
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
            return Ok(messages);
        }

        // POST: api/noticeboard/send
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            message.Timestamp = DateTime.UtcNow;

            try
            {
                _context.Messages.Add(message);
                await _context.SaveChangesAsync();
                return Ok(new { Message = "Message sent successfully" });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error saving message:", ex.Message); // Debugging log
                return StatusCode(500, new { Error = "Failed to send message", Details = ex.Message });
            }
        }
    }
}