using ChatGptTest.Settings;
using NAudio.Wave;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static ChatGptTest.Constants;

namespace ChatGptTest.Services;

public interface IAudioService
{
    Task PlayFile(string filePath);
    Task<string> RecordAudio();
}

public class AudioService(ChatGptSettings settings) : IAudioService
{
    public async Task PlayFile(string filePath)
    {
        try
        {
            ProcessStartInfo startInfo = new ()
            {
                FileName = "afplay",
                Arguments = filePath,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            await process!.WaitForExitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }
    }

    public async Task<string> RecordAudio()
    {
        var outputFilePath = Path.Combine(settings.RecordingPath, $"Recording_{Guid.NewGuid()}.mp3");
        
        try
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "sox",
                Arguments = $"-d {outputFilePath}",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                Console.WriteLine("Recording... Press any key to stop.");
                Console.ReadKey();

                process.Kill();
            }

            Console.WriteLine($"Audio recorded to {outputFilePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return outputFilePath;
    }
}

