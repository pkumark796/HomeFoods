using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HomeFoods.Application.Services;
using HomeFoods.Application.DTOs;
using OpenAI.Chat;
using OpenAI.Responses;

namespace HomeFoods.API.AITools
{
#pragma warning disable OPENAI001
    /// <summary>
    /// Consolidated chat service that supports both conversational AI via ChatClient
    /// and a simple session-based helper backed by the Responses API.
    /// </summary>
    public class ChatService
    {
        private readonly ChatClient _chatClient;
        private readonly IOrderService _orderService;

        // In-memory session store used by the session helper methods
        private static readonly ConcurrentDictionary<string, List<ResponseItem>> _sessions = new();
        private static readonly ResponsesClient _responsesClient;

        static ChatService()
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                // Delay throwing so applications that don't use ResponsesClient can still start.
                _responsesClient = null!;
            }
            else
            {
                _responsesClient = new ResponsesClient(apiKey);
            }
        }

        public ChatService(
            ChatClient chatClient,
            IOrderService orderService)
        {
            _chatClient = chatClient;
            _orderService = orderService;
        }

        // ---------------------------------------------------------------------
        // Session helper API (merged from previous ChatHelper)
        // ---------------------------------------------------------------------

        public static string StartSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            var conversation = new List<ResponseItem>
            {
                ResponseItem.CreateSystemMessageItem("You are a helpful assistant for the HomeFoods portal.")
            };

            _sessions[sessionId] = conversation;
            return sessionId;
        }

        public static string SendMessage(string sessionId, string message)
        {
            if (_responsesClient == null)
            {
                throw new InvalidOperationException("ResponsesClient is not configured. Set OPENAI_API_KEY.");
            }

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

            var response = _responsesClient.CreateResponse(options);
            var answer = response.Value.GetOutputText();

            conversation.Add(ResponseItem.CreateAssistantMessageItem(answer));
            return answer;
        }

        // ---------------------------------------------------------------------
        // Conversational Chat API (uses ChatClient and supports tool-calling)
        // ---------------------------------------------------------------------

        public async Task<string> ChatAsync(
             string userMessage,
             CancellationToken cancellationToken)
        {
            // Quick path: direct lookup when an ORD... order number is present
            var orderMatch = Regex.Match(userMessage ?? string.Empty, @"\bORD\d+\b", RegexOptions.IgnoreCase);
            if (orderMatch.Success)
            {
                var orderNumber = orderMatch.Value.ToUpperInvariant();
                var orderDto = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
                if (orderDto == null)
                {
                    return $"I couldn't find an order with number {orderNumber}. Please check the number and try again.";
                }

                var orderDate = orderDto.OrderDate.ToString("d");
                var total = orderDto.Total.ToString("C");
                var paymentStatus = orderDto.Payment?.Status ?? "N/A";

                return $"Order {orderDto.OrderNumber} is currently '{orderDto.Status}'. Placed on {orderDate}. Total: {total}. Payment status: {paymentStatus}.";
            }

            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("""
                    You are an ecommerce assistant for HomeFoods.

                    If the user's message contains a clear order number (for example ORD2026090647294), you MUST call the get_order_status tool immediately with {"orderNumber":"<ORDER_NUMBER>"} and return the tool's result to the user.

                    For other order queries (no explicit order number), require identity verification before calling the tool: ask the user to confirm either the email on the order, the last 4 digits of the phone number, or the ZIP code.

                    When calling get_order_status provide a JSON argument with either:
                      {"orderNumber":"<ORDER_NUMBER>"}
                    or
                      {"orderId":"<GUID>"}

                    Example function call: {"orderNumber":"ORD2026090647294"}

                    Never invent or guess order data. Only return information from the tool response.
                    """),

                new UserChatMessage(userMessage)
            };

            var options = new ChatCompletionOptions();
            options.Tools.Add(GetOrderStatusTool);

            while (true)
            {
                if (_chatClient == null)
                {
                    // Fallback: if ChatClient is not configured, try the Responses-based helper if available.
                    if (_responsesClient != null)
                    {
                        var sessionId = StartSession();
                        var reply = await Task.Run(() => SendMessage(sessionId, userMessage), cancellationToken);
                        return reply;
                    }

                    // Otherwise return a helpful user-facing message instead of crashing.
                    return "AI chat service is not configured. Please provide an order number (e.g. ORD2026090647294) or the email/last-4 digits/ZIP used on the order so we can look it up.";
                }

                ChatCompletion completion =
                    await _chatClient.CompleteChatAsync(
                        messages,
                        options,
                        cancellationToken);

                if (completion.FinishReason == ChatFinishReason.Stop)
                {
                    return completion.Content[0].Text;
                }

                if (completion.FinishReason == ChatFinishReason.ToolCalls)
                {
                    messages.Add(new AssistantChatMessage(completion));

                    foreach (var toolCall in completion.ToolCalls)
                    {
                        switch (toolCall.FunctionName)
                        {
                            case "get_order_status":
                                {
                                    using var arguments = JsonDocument.Parse(toolCall.FunctionArguments);
                                    var root = arguments.RootElement;

                                    string? orderIdText = null;
                                    string? orderNumber = null;

                                    if (root.TryGetProperty("orderId", out var idProp) && idProp.ValueKind == JsonValueKind.String)
                                    {
                                        orderIdText = idProp.GetString();
                                    }

                                    if (root.TryGetProperty("orderNumber", out var numProp) && numProp.ValueKind == JsonValueKind.String)
                                    {
                                        orderNumber = numProp.GetString();
                                    }

                                    if (string.IsNullOrWhiteSpace(orderIdText) && string.IsNullOrWhiteSpace(orderNumber))
                                    {
                                        messages.Add(new ToolChatMessage(toolCall.Id, JsonSerializer.Serialize(new { success = false, error = "Missing orderId or orderNumber." })));
                                        break;
                                    }

                                    OrderResponseDto? order = null;

                                    if (!string.IsNullOrWhiteSpace(orderIdText) && Guid.TryParse(orderIdText, out var orderId))
                                    {
                                        order = await _orderService.GetByIdAsync(orderId, cancellationToken);
                                    }
                                    else if (!string.IsNullOrWhiteSpace(orderNumber))
                                    {
                                        order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
                                    }

                                    var result = order == null
                                        ? JsonSerializer.Serialize(new { success = false, error = "Order not found." })
                                        : JsonSerializer.Serialize(new { success = true, order });

                                    messages.Add(new ToolChatMessage(toolCall.Id, result));
                                    break;
                                }
                        }
                    }

                    continue;
                }

                throw new InvalidOperationException($"Unexpected finish reason: {completion.FinishReason}");
            }
        }

        private static readonly ChatTool GetOrderStatusTool = ChatTool.CreateFunctionTool(
            functionName: "get_order_status",
            functionDescription: "Gets the current status and details of an ecommerce order.",
            functionParameters: BinaryData.FromBytes("""
            {
              "type": "object",
              "properties": {
                "orderId": {
                  "type": "string",
                  "description": "The order ID as a GUID."
                },
                "orderNumber": {
                  "type": "string",
                  "description": "The human-facing order number, e.g. ORD2026090620073."
                }
              },
              "anyOf": [
                { "required": ["orderId"] },
                { "required": ["orderNumber"] }
              ],
              "additionalProperties": false
            }
            """u8.ToArray())
        );
    }
}
