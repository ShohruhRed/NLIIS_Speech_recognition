using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Emgu.CV;

namespace NLIIS_Speech_recognition.Services
{
    public abstract class Action
    {
        protected abstract IDictionary<string, string> Descriptions { get; set; }

        protected virtual bool IsApplicable(string action, string language)
        {
            return Distancer.GetDistance(Descriptions[language], action) < 5;
        }

        public abstract string Run(string action, string language);

        public static Action GetApplicableAction(IEnumerable<Action> actions, string stringAction, string language)
        {
            return actions.FirstOrDefault(action => action.IsApplicable(stringAction, language));
        }
    }

    public class CreateNoteAction : Action
    {
        protected override IDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>
        {
            { "Russian", "создай заметку" },
            { "English", "create a note" }
        };

        private readonly IDictionary<string, IEnumerable<string>> _keywords = new Dictionary<string, IEnumerable<string>>
        {
            { "Russian", new[] { "созда", "замет" } },
            { "English", new[] { "create", "note" } }
        };
        
        protected override bool IsApplicable(string action, string language)
        {
            var createWordIndex = action.IndexOf(_keywords[language].ElementAt(0), StringComparison.Ordinal);
            var noteWordIndex = action.IndexOf(_keywords[language].ElementAt(1), StringComparison.Ordinal);

            return Distancer.GetDistance(Descriptions[language], action) > 1 &&
                   createWordIndex < noteWordIndex;
        }

        public override string Run(string action, string language)
        {
            var actualText = new string(action);
            var symbolMathPattern = DocumentService.GetSymbolsMatchPattern() + "+";

            foreach (var keyword in _keywords[language])
            {
                var regex = new Regex(keyword + symbolMathPattern);
                actualText = regex.Replace(actualText, string.Empty);
            }

            actualText = actualText.Trim();
            var path = DocumentService.ToFile(actualText, "note");

            return "Note saved at " + path;
        }
    }

    public class PlayMusicAction : Action
    {
        protected override IDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>
        {
            { "Russian", "включи музыку на диске" },
            { "English", "play music from drive" }
        };

        public override string Run(string action, string language)
        {
            var mediaPlayer = new MediaPlayer();

            var disk = GetDisk(action);
            var directoryInfo = new DirectoryInfo($"{disk}:\\music");
            var files = directoryInfo.GetFiles();
            var mp3File = files.First(file => file.Extension.Equals(".mp3"));

            mediaPlayer.Open(new Uri(mp3File.FullName));
            mediaPlayer.Play();

            return "Enjoy the music!";
        }

        private string GetDisk(string action)
        {
            var disk = new string(action);
            disk = Regex.Replace(disk, DocumentService.GetSymbolsMatchPattern() + "{2,}", string.Empty);

            return disk.Trim();
        }
    }

    public class CameraCaptionAction : Action
    {
        protected override IDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>
        {
            { "Russian", "сфотографируй" },
            { "English", "take a photo" }
        };

        public override string Run(string action, string language)
        {
            var capture = new VideoCapture();
            var image = capture.QueryFrame();

            const string path = "D:\\camera.jpg";
            image.Save(path);

            return "Photo was saved to " + path;
        }
    }
}
