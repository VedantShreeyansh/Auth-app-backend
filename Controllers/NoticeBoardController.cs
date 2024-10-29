using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;
using System.Linq;

namespace auth_app_backend.Controllers
{
    // Define the controller for the notice board
    [Route("api/[controller]")]
    [ApiController]
    public class NoticeBoardController : ControllerBase
    {
        private readonly NoticeBoardContext _context;

        public NoticeBoardController(NoticeBoardContext context)
        {
            _context = context;
        }

        // GET: api/noticeboard/messages - fetch all messages in descending order by timestamp
        [HttpGet("messages")]
        public async Task<IActionResult> GetMessages()
        {
            var messages = await _context.Messages
                .OrderByDescending(m => m.Timestamp)
                .ToListAsync();
            return Ok(messages);
        }

        // POST: api/noticeboard/send - send a message
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] Message message)
        {
            message.Timestamp = DateTime.UtcNow; // Set the timestamp to the current time
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Message sent successfully" });
        }
    }

    // Define the database context for the NoticeBoard
    public class NoticeBoardContext : DbContext
    {
        public NoticeBoardContext(DbContextOptions<NoticeBoardContext> options) : base(options) { }

        public DbSet<Message> Messages { get; set; }
    }

    // Define the Message model to represent messages in the database
    [Table("messages")]
    public class Message
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; }

        [Required]
        public string RecipientGroup { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
