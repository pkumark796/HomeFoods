using OpenAI.Responses;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System;

namespace HomeFoods.API.Controllers
{
    // Helper that manages simple in-memory chat sessions and forwards messages to the OpenAI Responses API.
#pragma warning disable OPENAI001
    public static class ChatHelper
    {
        private static readonly ConcurrentDictionary<string, List<ResponseItem>> _sessions = new();
        private static readonly ResponsesClient _client;

        static ChatHelper()
        {
            // Prefer environment variable but fall back to existing value if not set.
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("OpenAI API key is not configured. Set OPENAI_API_KEY environment variable.");
            }

            _client = new ResponsesClient(apiKey);
        }

        public static string StartSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            var conversation = new List<ResponseItem>
            {
                // Optional system prompt to guide the assistant
                ResponseItem.CreateSystemMessageItem("You are a helpful assistant for the HomeFoods portal.")
            };

            _sessions[sessionId] = conversation;
            return sessionId;
        }

        public static string SendMessage(string sessionId, string message)
        {
            if (string.IsNullOrWhiteSpace(sessionId) || !_sessions.TryGetValue(sessionId, out var conversation))
                throw new ArgumentException("Invalid or expired sessionId", nameof(sessionId));

            conversation.Add(ResponseItem.CreateUserMessageItem(message));

            var options = new CreateResponseOptions
            {
                Model = "gpt-5.2"
            };

            foreach (var item in conversation)
            {
                options.InputItems.Add(item);
            }

            var response = _client.CreateResponse(options);
            var answer = response.Value.GetOutputText();

            conversation.Add(ResponseItem.CreateAssistantMessageItem(answer));
            return answer;
        }
    }
#pragma warning restore OPENAI001
}
