using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using HomeFoods.API.AITools;
using OpenAI.Chat;
using HomeFoods.Application.Services;

namespace HomeFoods.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly HomeFoods.API.AITools.ChatService _chatService;

        public ChatController(HomeFoods.API.AITools.ChatService chatService)
        {
            _chatService = chatService;
        }
        // POST api/chat/start
        [HttpPost("start")]
        public IActionResult Start()
        {
            var sessionId = ChatService.StartSession();
            return Ok(new { sessionId });
        }

        public class MessageRequest
        {
            public string SessionId { get; set; }
            public string Message { get; set; }
        }

        // POST api/chat/message
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
                // Log if you have logging; return generic error
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Chat service error" });
            }
        }
    }
}
