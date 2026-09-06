using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HomeFoods.API.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddHomeFoodsAI(this IServiceCollection services, IConfiguration configuration)
        {
            var openAiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? configuration["OPENAI_API_KEY"];

            // Register ChatService for DI so controllers can use it directly.
            // ChatService depends on OpenAI.Chat.ChatClient which may have multiple constructors across SDK versions.
            // Create ChatClient via reflection to remain tolerant to constructor differences.
            services.AddScoped<HomeFoods.API.AITools.ChatService>(sp =>
            {
                var orderService = sp.GetRequiredService<HomeFoods.Application.Services.IOrderService>();

                object? chatClientInstance = null;
                if (!string.IsNullOrWhiteSpace(openAiKey))
                {
                    var chatClientType = typeof(OpenAI.Chat.ChatClient);
                    var ctors = chatClientType.GetConstructors();
                    foreach (var ctor in ctors)
                    {
                        var parameters = ctor.GetParameters();
                        try
                        {
                            if (parameters.Length == 1)
                            {
                                var pType = parameters[0].ParameterType;
                                // Try constructor that accepts string
                                if (pType == typeof(string))
                                {
                                    chatClientInstance = ctor.Invoke(new object[] { openAiKey });
                                    break;
                                }

                                // Try to construct parameter type with the key or parameterless
                                object? paramInstance = null;
                                try { paramInstance = Activator.CreateInstance(pType, new object[] { openAiKey }); } catch { }
                                if (paramInstance == null)
                                {
                                    try { paramInstance = Activator.CreateInstance(pType); } catch { }
                                }

                                if (paramInstance != null)
                                {
                                    chatClientInstance = ctor.Invoke(new object[] { paramInstance });
                                    break;
                                }
                            }
                            else if (parameters.Length == 0)
                            {
                                chatClientInstance = ctor.Invoke(Array.Empty<object>());
                                break;
                            }
                        }
                        catch
                        {
                            // ignore and try next ctor
                        }
                    }
                }

                var chatClient = chatClientInstance == null ? null : (OpenAI.Chat.ChatClient)chatClientInstance!;
                return new HomeFoods.API.AITools.ChatService(chatClient!, orderService);
            });

            return services;
        }
    }
}
