using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using ChatGptTest;
using ChatGptTest.Services;
using OpenAI_API;
using Serilog.Events;
using System;
using System.Threading.Tasks;
using static ChatGptTest.Extensions.ServiceCollectionExtensions;

class Program
{
    static async Task Main(string[] args)
    {
        // Configure Serilog
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            Log.Information("Starting up the application");

            var host = CreateHostBuilder(args).Build();
            var application = host.Services.GetService<Application>();

            await application!.Run();
        }
        catch (Exception ex) when (ex is not ArgumentException)
        {
            Log.Fatal(ex, "Application start-up failed");
        }
        finally
        {
            await Log.CloseAndFlushAsync();
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseSerilog() // Add Serilog
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton(Log.Logger);
                services.AddSingleton<IAudioService, AudioService>();
                services.AddChatGptService(context);
                services.AddTransient<Application>();
            });
}
