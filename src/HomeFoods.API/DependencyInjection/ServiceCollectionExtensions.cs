using System;
using HomeFoods.API.AITools;
using HomeFoods.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.Chat;

namespace HomeFoods.API.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHomeFoodsAI(this IServiceCollection services, IConfiguration configuration)
        {
            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? configuration["OPENAI_API_KEY"];

            // Register SearchService in Application layer and ChatService for DI so controllers can use them directly.
            services.AddScoped<ISearchService, SearchService>();

            services.AddScoped<ChatService>(sp =>
            {
                var orderService = sp.GetRequiredService<IOrderService>();
                var searchService = sp.GetRequiredService<ISearchService>();
                var chatClient = CreateChatClient(openAiKey);

                return new HomeFoods.API.AITools.ChatService(chatClient!, orderService, searchService);
            });

            return services;
        }

        private static ChatClient? CreateChatClient(string? openAiKey)
        {
            if (string.IsNullOrWhiteSpace(openAiKey))
            {
                return null;
            }

            var chatClientType = typeof(ChatClient);

            foreach (var constructor in chatClientType.GetConstructors())
            {
                var parameters = constructor.GetParameters();

                try
                {
                    if (parameters.Length == 1)
                    {
                        var parameterType = parameters[0].ParameterType;

                        if (parameterType == typeof(string))
                        {
                            return (ChatClient)constructor.Invoke([openAiKey]);
                        }

                        var parameterValue = CreateConstructorParameter(parameterType, openAiKey);
                        if (parameterValue != null)
                        {
                            return (ChatClient)constructor.Invoke([parameterValue]);
                        }
                    }

                    if (parameters.Length == 0)
                    {
                        return (ChatClient)constructor.Invoke(Array.Empty<object>());
                    }
                }
                catch
                {
                }
            }

            return null;
        }

        private static object? CreateConstructorParameter(Type parameterType, string openAiKey)
        {
            try
            {
                return Activator.CreateInstance(parameterType, [openAiKey]);
            }
            catch
            {
            }

            try
            {
                return Activator.CreateInstance(parameterType);
            }
            catch
            {
                return null;
            }
        }
    }
}
