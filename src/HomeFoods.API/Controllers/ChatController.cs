using System;
using HomeFoods.API.AITools;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HomeFoods.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;

        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpPost("start")]
        public IActionResult Start()
        {
            var sessionId = ChatService.StartSession();
            return Ok(new { sessionId });
        }

        public sealed class MessageRequest
        {
            public string SessionId { get; set; } = string.Empty;
            public string Message { get; set; } = string.Empty;
        }

        [HttpPost("message")]
        public async Task<IActionResult> PostMessage([FromBody] MessageRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { error = "Missing message" });

            try
            {
                // Use the injected ChatService to handle the message. ChatService.ChatAsync contains
                // a quick-path lookup for explicit order numbers and the tool-calling loop otherwise.
                var reply = await _chatService.ChatAsync(request.Message, HttpContext.RequestAborted);
                return Ok(new { reply });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Chat service error" });
            }
        }
    }
}
