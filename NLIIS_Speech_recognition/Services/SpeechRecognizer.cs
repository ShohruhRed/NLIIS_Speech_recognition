using Google.Cloud.Speech.V1;

namespace NLIIS_Speech_recognition.Services
{
    public class SpeechRecognizer
    {
        private readonly SpeechClient _speechClient;
        public string Language { get; set; }
        public string LastRecognized { get; private set; }

        public SpeechRecognizer()
        {
            _speechClient = SpeechClient.Create();
        }

        public string ToText()
        {
            var response = _speechClient.Recognize(
                new RecognitionConfig
                {
                    Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                    SampleRateHertz = 44100,
                    LanguageCode = GetLanguageCode(Language),
                    AudioChannelCount = 2
                },
                RecognitionAudio.FromFile("D:\\command.wav"));

            LastRecognized = response.Results[0].Alternatives[0].Transcript;

            return LastRecognized;
        }

        private string GetLanguageCode(string language)
        {
            return language switch
            {
                "Russian" => "ru",
                "English" => "en",
                _ => string.Empty
            };
        }
    }
}
