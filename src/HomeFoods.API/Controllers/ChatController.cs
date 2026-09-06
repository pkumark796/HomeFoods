using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace HomeFoods.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        // POST api/chat/start
        [HttpPost("start")]
        public IActionResult Start()
        {
            var sessionId = ChatHelper.StartSession();
            return Ok(new { sessionId });
        }

        public class MessageRequest
        {
            public string SessionId { get; set; }
            public string Message { get; set; }
        }

        // POST api/chat/message
        [HttpPost("message")]
        public IActionResult PostMessage([FromBody] MessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Missing message" });

            try
            {
                var reply = ChatHelper.SendMessage(request.SessionId, request.Message);
                return Ok(new { reply });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                // Log if you have logging; return generic error
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Chat service error" });
            }
        }
    }
}
