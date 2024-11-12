﻿using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using auth_app_backend.Model;
using auth_app_backend.Services;

namespace auth_app_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly CouchDbService _couchDbService;

        public UserController(CouchDbService couchDbService)
        {
            _couchDbService = couchDbService;
        }

        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingUsers()
        {
            var pendingUsers = await _couchDbService.GetPendingUsersAsync();


            if (pendingUsers == null || !pendingUsers.Any())
            {
                return NotFound("No pending users found.");
            }

            return Ok(pendingUsers);
        }

        [HttpPost("approve")]
        public async Task<IActionResult> ApproveUser([FromBody] ApprovalDto approvalData)
        {
            if (approvalData == null || string.IsNullOrEmpty(approvalData.UserId))
            {
                return BadRequest("Invalid approval data.");
            }

            var user = await _couchDbService.GetUserByIdAsync(approvalData.UserId); // Pass UserId as string
            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.status = approvalData.IsApproved ? "Approved" : "Rejected";

            try
            {
                await _couchDbService.UpdateUserAsync(user);
                return Ok("User approval status updated.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating user approval status: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id) // Change parameter type to string
        {
            var user = await _couchDbService.GetUserByIdAsync(id); // Pass id as string
            if (user == null)
            {
                return NotFound("User not found.");
            }

            return Ok(user);
        }
    }

    public class ApprovalDto
    {
        public string UserId { get; set; } // Change UserId to string
        public bool IsApproved { get; set; }
    }
<<<<<<< HEAD
}
=======
}
>>>>>>> 36499e728e35cd3a78ae3c8e08b8183a639edb05
