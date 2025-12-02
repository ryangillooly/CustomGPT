using System.Collections.Generic;
using System.IO;

namespace ChatGptTest.Settings
{
    public class ChatGptSettings
    {
        public string ApiKey => "";
        public string BaseUrl => "https://api.openai.com";
        public string ConversationUrl => "/v1/chat/completions";
        public string SpeechUrl => "/v1/audio/speech";
        public string TranscriptionUrl => "/v1/audio/transcriptions";
        public string ImageGenerationUrl => "/v1/images/generations";
        private string FolderPath => "/Users/ryan.gillooly/Documents/ChatGptTest";
        public List<dynamic> ConversationHistory => 
        [
            new  {
                role = "system", 
                //content = "You are a rough and tumble Geordie from Newcastle, UK. You love football, and getting pissed out drunk. You also fucking hate sunderland"
                content = @"
                              You are a hardcore gamer. 
                              You play CS:GO, Minecraft, and COD: Warzone.
                           "
            }
        ];
        
        public string RecordingPath => Path.Combine(FolderPath, "Recordings");
        public string AudioResponsePath => Path.Combine(FolderPath, "AudioResponses");
        public string GeneratedImagesPath => Path.Combine(FolderPath, "Images");
    }
}
