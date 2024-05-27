using ChatGptTest.Services;
using ChatGptTest.Settings;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenAI_API;
using System;

namespace ChatGptTest.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddChatGptService(this IServiceCollection services, HostBuilderContext context)
    {
        services
            .AddSingleton<ChatGptSettings>()
            .AddSingleton(serviceProvider =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<ChatGptSettings>>().Value;
                return new OpenAIAPI(settings.ApiKey);
            })
            .AddHttpClient<IChatGptService, ChatGptService>((serviceProvider, client) =>
            {
                var settings = serviceProvider.GetRequiredService<IOptions<ChatGptSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseUrl);
                //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
            });

        return services;
    }
}