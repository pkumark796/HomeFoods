using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using HomeFoods.Application.Services;
using HomeFoods.Application.DTOs;
using HomeFoods.Domain.Entities;
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
        private const string ResponseModel = "gpt-5.2";
        private const int DefaultProductSearchLimit = 20;
        private const int DefaultToolSearchLimit = 10;
        private const string OrderStatusToolName = "get_order_status";
        private const string SearchProductsToolName = "search_products";

        private static readonly Regex OrderNumberRegex = new(@"\bORD\d+\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly HashSet<string> SearchStopWords =
        [
            "do", "you", "have", "in", "stock", "give", "me", "all", "available", "price", "prices",
            "show", "list", "find", "search", "for", "any", "the", "a", "an", "of", "with", "and",
            "what", "is", "are", "can", "i", "get", "want", "need", "please"
        ];

        private const string SessionPrompt = "You are a helpful assistant for the HomeFoods customer chat. Only help with order status and product catalog questions. Politely decline other topics.";
        private const string AssistantPrompt = """
            You are a HomeFoods customer assistant.

            This chat is only for two things:
            1. Checking order status
            2. Finding products in the catalog

            If the user asks about anything else (admin, catalog management, category creation, settings, support workflows, or unrelated topics), politely refuse and redirect them to order status or product catalog questions only.

            If the user's message contains a clear order number (for example ORD2026090647294), you MUST call the get_order_status tool immediately with {"orderNumber":"<ORDER_NUMBER>"} and return the tool's result to the user.

            If the user asks about products, availability, stock, brands, sizes, or search terms such as "rice", "basmati", or "basmati rice", you MUST call the search_products tool immediately before replying.
            For questions like "do you have rice in stock?", call search_products with the core product query, for example {"query":"rice","limit":10}.
            Use the tool response to answer whether matching products are in stock. Do not ask a follow-up question before calling the tool unless the user's request is completely unclear.

            For other order queries (no explicit order number), require identity verification before calling the tool: ask the user to confirm either the email on the order, the last 4 digits of the phone number, or the ZIP code.

            When calling get_order_status provide a JSON argument with either:
              {"orderNumber":"<ORDER_NUMBER>"}
            or
              {"orderId":"<GUID>"}

            Example function call: {"orderNumber":"ORD2026090647294"}

            Never invent or guess order data or product details. Only return information from the tool response.
            """;

        private readonly ChatClient? _chatClient;
        private readonly IOrderService _orderService;
        private readonly ISearchService _searchService;

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
            ChatClient? chatClient,
            IOrderService orderService,
            ISearchService searchService)
        {
            _chatClient = chatClient;
            _orderService = orderService;
            _searchService = searchService;
        }

        // ---------------------------------------------------------------------
        // Session helper API (merged from previous ChatHelper)
        // ---------------------------------------------------------------------

        public static string StartSession()
        {
            var sessionId = Guid.NewGuid().ToString();
            var conversation = new List<ResponseItem>
            {
                ResponseItem.CreateSystemMessageItem(SessionPrompt)
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
                Model = ResponseModel
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
            var orderStatusReply = await TryGetOrderStatusReplyAsync(userMessage, cancellationToken);
            if (orderStatusReply != null)
            {
                return orderStatusReply;
            }

            var productSearchReply = await TryGetProductSearchReplyAsync(userMessage, cancellationToken);
            if (productSearchReply != null)
            {
                return productSearchReply;
            }

            if (_chatClient == null)
            {
                return await GetFallbackReplyAsync(userMessage, cancellationToken);
            }

            var messages = CreateMessages(userMessage);
            var options = CreateChatOptions();

            while (true)
            {
                var completionResult = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
                var completion = completionResult.Value;

                switch (completion.FinishReason)
                {
                    case ChatFinishReason.Stop:
                        return completion.Content[0].Text;
                    case ChatFinishReason.ToolCalls:
                        await HandleToolCallsAsync(messages, completion, cancellationToken);
                        continue;
                    default:
                        throw new InvalidOperationException($"Unexpected finish reason: {completion.FinishReason}");
                }
            }
        }

        private async Task<string?> TryGetOrderStatusReplyAsync(string userMessage, CancellationToken cancellationToken)
        {
            var orderNumberMatch = OrderNumberRegex.Match(userMessage ?? string.Empty);
            if (!orderNumberMatch.Success)
            {
                return null;
            }

            var orderNumber = orderNumberMatch.Value.ToUpperInvariant();
            var order = await _orderService.GetByOrderNumberAsync(orderNumber, cancellationToken);
            if (order == null)
            {
                return $"I couldn't find an order with number {orderNumber}. Please check the number and try again.";
            }

            return FormatOrderStatusResponse(order);
        }

        private async Task<string?> TryGetProductSearchReplyAsync(string userMessage, CancellationToken cancellationToken)
        {
            var searchQuery = ExtractProductSearchQuery(userMessage);
            if (string.IsNullOrWhiteSpace(searchQuery))
            {
                return null;
            }

            var products = (await _searchService.SearchProductsAsync(searchQuery, DefaultProductSearchLimit, cancellationToken)).ToList();
            return products.Count == 0
                ? null
                : FormatProductSearchResponse(searchQuery, products);
        }

        private async Task<string> GetFallbackReplyAsync(string userMessage, CancellationToken cancellationToken)
        {
            if (_responsesClient != null)
            {
                var sessionId = StartSession();
                return await Task.Run(() => SendMessage(sessionId, userMessage), cancellationToken);
            }

            return "AI chat service is not configured. Please provide an order number (e.g. ORD2026090647294) or the email/last-4 digits/ZIP used on the order so we can look it up.";
        }

        private static List<ChatMessage> CreateMessages(string userMessage)
        {
            return
            [
                new SystemChatMessage(AssistantPrompt),
                new UserChatMessage(userMessage)
            ];
        }

        private static ChatCompletionOptions CreateChatOptions()
        {
            var options = new ChatCompletionOptions();
            options.Tools.Add(GetOrderStatusTool);
            options.Tools.Add(SearchProductsTool);
            return options;
        }

        private async Task HandleToolCallsAsync(List<ChatMessage> messages, ChatCompletion completion, CancellationToken cancellationToken)
        {
            messages.Add(new AssistantChatMessage(completion));

            foreach (var toolCall in completion.ToolCalls)
            {
                var toolResponse = toolCall.FunctionName switch
                {
                    SearchProductsToolName => await HandleSearchProductsToolCallAsync(toolCall, cancellationToken),
                    OrderStatusToolName => await HandleOrderStatusToolCallAsync(toolCall, cancellationToken),
                    _ => JsonSerializer.Serialize(new { success = false, error = $"Unsupported tool: {toolCall.FunctionName}." })
                };

                messages.Add(new ToolChatMessage(toolCall.Id, toolResponse));
            }
        }

        private async Task<string> HandleSearchProductsToolCallAsync(ChatToolCall toolCall, CancellationToken cancellationToken)
        {
            using var arguments = JsonDocument.Parse(toolCall.FunctionArguments);
            var root = arguments.RootElement;

            if (!TryGetString(root, "query", out var query))
            {
                return JsonSerializer.Serialize(new { success = false, error = "Missing required property 'query'." });
            }

            var limit = TryGetInt32(root, "limit", out var parsedLimit)
                ? Math.Clamp(parsedLimit, 1, 50)
                : DefaultToolSearchLimit;

            var products = await _searchService.SearchProductsAsync(query, limit, cancellationToken);
            var results = products.Select(ToProductSummary);

            return JsonSerializer.Serialize(new { success = true, results });
        }

        private async Task<string> HandleOrderStatusToolCallAsync(ChatToolCall toolCall, CancellationToken cancellationToken)
        {
            using var arguments = JsonDocument.Parse(toolCall.FunctionArguments);
            var root = arguments.RootElement;

            TryGetString(root, "orderId", out var orderIdText);
            TryGetString(root, "orderNumber", out var orderNumber);

            if (string.IsNullOrWhiteSpace(orderIdText) && string.IsNullOrWhiteSpace(orderNumber))
            {
                return JsonSerializer.Serialize(new { success = false, error = "Missing orderId or orderNumber." });
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

            return order == null
                ? JsonSerializer.Serialize(new { success = false, error = "Order not found." })
                : JsonSerializer.Serialize(new { success = true, order });
        }

        private static string FormatOrderStatusResponse(OrderResponseDto order)
        {
            var orderDate = order.OrderDate.ToString("d");
            var total = order.Total.ToString("C");
            var paymentStatus = order.Payment?.Status ?? "N/A";

            return $"Order {order.OrderNumber} is currently '{order.Status}'. Placed on {orderDate}. Total: {total}. Payment status: {paymentStatus}.";
        }

        private static object ToProductSummary(Product product)
        {
            return new
            {
                id = product.Id,
                name = product.Name,
                sku = product.SKU,
                price = product.Price,
                discountedPrice = product.DiscountedPrice,
                stock = product.StockQuantity,
                inStock = product.StockQuantity > 0,
                imageUrl = product.ImageUrl,
                category = product.Category != null ? new { id = product.Category.Id, name = product.Category.Name } : null,
                brand = product.Brand != null ? new { id = product.Brand.Id, name = product.Brand.Name } : null
            };
        }

        private static bool TryGetString(JsonElement element, string propertyName, out string? value)
        {
            value = null;

            if (!element.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            value = property.GetString();
            return !string.IsNullOrWhiteSpace(value);
        }

        private static bool TryGetInt32(JsonElement element, string propertyName, out int value)
        {
            value = default;

            return element.TryGetProperty(propertyName, out var property)
                && property.ValueKind == JsonValueKind.Number
                && property.TryGetInt32(out value);
        }

        private static string? ExtractProductSearchQuery(string? userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
            {
                return null;
            }

            var normalized = Regex.Replace(userMessage.ToLowerInvariant(), @"[^a-z0-9\s]", " ");

            var tokens = normalized
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(token => !SearchStopWords.Contains(token))
                .ToArray();

            if (tokens.Length == 0)
            {
                return null;
            }

            return string.Join(' ', tokens);
        }

        private static string FormatProductSearchResponse(string query, List<Product> products)
        {
            var lines = products
                .Select(product =>
                {
                    var price = (product.DiscountedPrice ?? product.Price).ToString("C");
                    var availability = product.StockQuantity > 0
                        ? $"In stock ({product.StockQuantity})"
                        : "Out of stock";

                    return $"- {product.Name}: {price} - {availability}";
                });

            return $"Here are the available results for '{query}':\n" + string.Join("\n", lines);
        }

        private static readonly ChatTool GetOrderStatusTool = ChatTool.CreateFunctionTool(
            functionName: OrderStatusToolName,
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

        private static readonly ChatTool SearchProductsTool = ChatTool.CreateFunctionTool(
            functionName: SearchProductsToolName,
            functionDescription: "Searches for products by a query string and returns matching product summaries including stock availability.",
            functionParameters: BinaryData.FromBytes("""
            {
              "type": "object",
              "properties": {
                "query": { "type": "string", "description": "Search term, e.g. 'rice'" },
                "limit": { "type": "integer", "description": "Maximum number of results to return", "minimum": 1, "maximum": 50 }
              },
              "required": ["query"],
              "additionalProperties": false
            }
            """u8.ToArray())
        );
    }
}
