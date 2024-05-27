using ChatGptTest.Extensions;
using ChatGptTest.Models;
using ChatGptTest.Settings;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using static ChatGptTest.Constants;

namespace ChatGptTest.Services;

public interface IChatGptService
{
    Task<string> ChatWithText(List<dynamic> conversationHistory);
    Task<string> ChatWithAudio(string text, string fileName = null);
    Task<string> ConvertAudioToText(string filePath);
    Task<List<string>> GenerateImage(string request);
}

public class ChatGptService(HttpClient httpClient, ChatGptSettings settings) : IChatGptService
{
    public async Task<string> ChatWithText(List<dynamic> conversationHistory)
    {
        var requestPayload = new
        {
            model = "gpt-4o", // or "gpt-3.5-turbo" or any other available model
            messages = conversationHistory,
            max_tokens = 300,
            temperature = 0.7
        };
        
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.ConversationUrl)
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json"),
            Headers = {{ "Authorization", $"Bearer {settings.ApiKey}" }}
        };
        
        var response = await httpClient.SendAsync(requestMessage);

        if (response.IsSuccessStatusCode)
        {
            string responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseJson = JsonConvert.DeserializeObject(responseContent);
            string chatGptResponse = responseJson.choices[0].message.content;
            return chatGptResponse.Trim();
        }

        string errorResponse = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Error: {response.StatusCode}");
        Console.WriteLine($"Details: {errorResponse}");
        return "Error: Unable to get a response from ChatGPT.";
    }
    public async Task<string> ChatWithAudio(string text, string fileName = null)
    {
        var requestPayload = new
        {
            model = "tts-1",
            input = text,
            voice = "alloy"
        };

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.SpeechUrl)
        {
            Content = new StringContent(JsonConvert.SerializeObject(requestPayload), Encoding.UTF8, "application/json"),
            Headers = {{ "Authorization", $"Bearer {settings.ApiKey}" }}
        };
        
        try
        {
            var filePath = Path.Combine(settings.AudioResponsePath, $"Speech_{Guid.NewGuid()}.mp3");
            var response = await httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();

            await using (var responseStream = await response.Content.ReadAsStreamAsync())
            {
                await using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    await responseStream.CopyToAsync(fileStream);
                }
            }

            Console.WriteLine($"Speech saved to {filePath}");
            return filePath;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
        }

        return null;
    }
    public async Task<string> ConvertAudioToText(string filePath)
    {
        byte[] audioBytes = File.ReadAllBytes(filePath);

        using (var content = new MultipartFormDataContent())
        {
            var audioContent = new ByteArrayContent(audioBytes);
            audioContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav"); // Change to "audio/mp3" if using mp3

            content.Add(audioContent, "file", Path.GetFileName(filePath));
            content.Add(new StringContent("whisper-1"), "model");

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.TranscriptionUrl)
            {
                Content = content,
                Headers = {{ "Authorization", $"Bearer {settings.ApiKey}" }}
            };
            
            var response = await httpClient.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed: {response.StatusCode}, {errorResponse}");
            }

            string responseContent = await response.Content.ReadAsStringAsync();
            JObject jsonResponse = JObject.Parse(responseContent);
            return jsonResponse["text"].ToString();
        }
    }
    public async Task<List<string>> GenerateImage(string request)
    {
        List<string> outputFileList = [];
        
        var requestPayload = new
        {
            prompt = request,
            num_images = request.GetImageCount()
        };
        
        var jsonPayload = JsonConvert.SerializeObject(requestPayload);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, settings.ImageGenerationUrl)
        {
            Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"),
            Headers = {{ "Authorization", $"Bearer {settings.ApiKey}" }}
        };

        try
        {
            httpClient.DefaultRequestHeaders.Remove("Authorization");
            var response = await httpClient.SendAsync(requestMessage);
            
            if (!response.IsSuccessStatusCode)
            {
                string errorResponse = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}");
                Console.WriteLine($"Details: {errorResponse}");
            }

            // Get the response content
            var jsonResponse = await response.Content.ReadAsStringAsync();
            var responseContent = JsonConvert.DeserializeObject<GenerateImageResult>(jsonResponse);

            // Extract the image URL from the response
            foreach (var result in responseContent!.Results)
            {
                var outputFilePath = Path.Combine(settings.GeneratedImagesPath, $"GenImage_{Guid.NewGuid()}.png");
                var imageBytes = await httpClient.GetByteArrayAsync(result.Url);

                // Save the image using SkiaSharp
                using (var ms = new MemoryStream(imageBytes))
                {
                    using (var skStream = new SKManagedStream(ms))
                    {
                        using (var skBitmap = SKBitmap.Decode(skStream))
                        {
                            using (var image = SKImage.FromBitmap(skBitmap))
                            {
                                using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
                                {
                                    await using (var fileStream = File.OpenWrite(outputFilePath))
                                    {
                                        data.SaveTo(fileStream);
                                        outputFileList.Add(outputFilePath);
                                    }
                                }
                            }
                        }
                    }
                }

                Console.WriteLine($"File has been generated and saved to - {outputFilePath}");
            }
            
            return outputFileList;
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Request error: {e.Message}");
            throw;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Unexpected error: {e.Message}");
            throw;
        }
    }
}
