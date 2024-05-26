using ChatGptTest.Services;
using ChatGptTest.Settings;
using OpenAI_API;
using System.Diagnostics;
using static ChatGptTest.Extensions.StringExtensions;

namespace ChatGptTest;

public class Application(
    IChatGptService chatGptService, 
    IAudioService audioService, 
    ChatGptSettings settings,
    OpenAIAPI openAi
)
{
    private bool EnableAudio = false;
    public async Task Run()
    {
        Console.WriteLine("Welcome to the ChatGPT console application!");
        Console.WriteLine("Type 'exit' to end the chat.");
        
        while (true)
        {
            var userTextRequest = "";
            
            if (EnableAudio)
            {
                Console.ReadLine();
                var recordingPath = await audioService.RecordAudio();
                userTextRequest = await chatGptService.ConvertAudioToText(recordingPath);
                Console.Write(userTextRequest);
            }
            else
            {
                Console.Write("You: ");
                userTextRequest = Console.ReadLine();
                if (userTextRequest!.ToLower() == "exit") { break; }
            }
            
            settings.ConversationHistory.Add(new { role = "user", content = userTextRequest });
            
            if (userTextRequest.ContainsImageGenerationKeywords())
            {
                var fileList = await chatGptService.GenerateImage(userTextRequest);

                foreach (var file in fileList)
                {
                    OpenImage(file);
                }
                
                continue;
            }
            
            if (EnableAudio)
            {
                await ChatGptAudioResponse();
            }
            else
            {
                await ChatGptTextResponse();
            }
        }
    }
    private static void OpenImage(string filePath)
    {
        try
        {
            Process process = new Process();
            process.StartInfo.FileName = "open"; // Using 'open' command for macOS
            process.StartInfo.Arguments = filePath;
            process.StartInfo.UseShellExecute = true;
            process.Start();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while opening the image: {ex.Message}");
        }
    }
    
    private async Task ChatGptAudioResponse()
    {
        var response = await chatGptService.ChatWithText(settings.ConversationHistory);
        var filePath = await chatGptService.ChatWithAudio(response);
        await audioService.PlayFile(filePath);
        Console.WriteLine($"ChatGPT: {response}");
        settings.ConversationHistory.Add(new { role = "assistant", content = response });
    }
    private async Task ChatGptTextResponse()
    {
        var response = await chatGptService.ChatWithText(settings.ConversationHistory);
        Console.WriteLine($"ChatGPT: {response}");
        settings.ConversationHistory.Add(new { role = "assistant", content = response });
    }
}