using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

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
            { "Russian", "создай заметку об авторе" },
            { "English", "create note about author" }
        };

        private readonly IDictionary<string, IEnumerable<string>> _keywords = new Dictionary<string, IEnumerable<string>>
        {
            { "Russian", new[] { "созда", "замет", "об", "автор" } },
            { "English", new[] { "create", "note", "about", "author" } }
        };
        
        protected override bool IsApplicable(string action, string language)
        {
            var createWordIndex = action.IndexOf(_keywords[language].ElementAt(0), StringComparison.Ordinal);
            var noteWordIndex = action.IndexOf(_keywords[language].ElementAt(1), StringComparison.Ordinal);
            var authorWordIndex = action.IndexOf(_keywords[language].ElementAt(3), StringComparison.Ordinal);

            return Distancer.GetDistance(Descriptions[language], action) > 1 &&
                   createWordIndex < noteWordIndex && noteWordIndex < authorWordIndex;
        }

        public override string Run(string action, string language)
        {
            var actualText = new string(action);
            var symbolMathPattern = DocumentService.GetSymbolsMatchPattern() + "*";
            var author = GetAuthor(action, language);

            foreach (var keyword in _keywords[language])
            {
                var regex = new Regex(keyword + symbolMathPattern);
                actualText = regex.Replace(actualText, string.Empty);
            }

            actualText = actualText.Replace(author, string.Empty);
            actualText = actualText.Trim();
            
            var path = DocumentService.ToFile(actualText, $"{author}_note");

            return "Note saved at " + path;
        }

        private string GetAuthor(string action, string language)
        {
            var author = new string(action);
            var authorStartIndex = action.LastIndexOf(_keywords[language].ElementAt(3), StringComparison.Ordinal);
            authorStartIndex = action.IndexOf(" ", authorStartIndex, StringComparison.Ordinal) + 1;
            var authorEndIndex = action.IndexOf(" ", authorStartIndex, StringComparison.Ordinal);

            if (authorEndIndex == -1)
            {
                authorEndIndex = authorStartIndex;
            }
            
            author = author.Substring(authorStartIndex,authorEndIndex == authorStartIndex
                ? author.Length - authorEndIndex
                : authorEndIndex - authorStartIndex);

            return author.Trim();
        }
    }

    public class PlayAuthorCompositionsAction : Action
    {
        protected override IDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>
        {
            { "Russian", "включи произведения" },
            { "English", "play compositions of" }
        };

        private readonly IDictionary<string, IEnumerable<string>> _keywords = new Dictionary<string, IEnumerable<string>>
        {
            { "Russian", new[] { "включ", "произведен" } },
            { "English", new[] { "play", "composition" } }
        };
        
        protected override bool IsApplicable(string action, string language)
        {
            var createWordIndex = action.IndexOf(_keywords[language].ElementAt(0), StringComparison.Ordinal);
            var compWordIndex = action.IndexOf(_keywords[language].ElementAt(1), StringComparison.Ordinal);

            if (createWordIndex == -1 || compWordIndex == -1)
            {
                return false;
            }
            
            return Distancer.GetDistance(Descriptions[language], action) > 1 &&
                   createWordIndex < compWordIndex;
        }

        public override string Run(string action, string language)
        {
            var mediaPlayer = new MediaPlayer();

            var author = GetAuthor(action, language);
            var directoryInfo = new DirectoryInfo($"D:\\comp\\{author}");
            var files = directoryInfo.GetFiles();
            var mp3File = files.First(file => file.Extension.Equals(".mp3"));

            mediaPlayer.Open(new Uri(mp3File.FullName));
            mediaPlayer.Play();

            return $"Enjoy {author}'s literature!";
        }

        private string GetAuthor(string action, string language)
        {
            var author = new string(action);
            author = Regex.Matches(author, DocumentService.GetWordMatchPattern()).Last().Value;

            return author.Trim().Substring(0, language.Equals("English") ? author.Length : author.Length - 2);
        }
    }

    public class CreateEssayAction : CreateNoteAction
    {
        protected override IDictionary<string, string> Descriptions { get; set; } = new Dictionary<string, string>
        {
            { "Russian", "создай реферат по произведению" },
            { "English", "create an essay on composition" }
        };

        private readonly IDictionary<string, IEnumerable<string>> _keywords = new Dictionary<string, IEnumerable<string>>
        {
            { "Russian", new[] { "созда", "реферат", "произведен" } },
            { "English", new[] { "create", "essay", "composition" } }
        };
        
        protected override bool IsApplicable(string action, string language)
        {
            var createWordIndex = action.IndexOf(_keywords[language].ElementAt(0), StringComparison.Ordinal);
            var essayWordIndex = action.IndexOf(_keywords[language].ElementAt(1), StringComparison.Ordinal);
            var compositionWordIndex = action.IndexOf(_keywords[language].ElementAt(2), StringComparison.Ordinal);

            return Distancer.GetDistance(Descriptions[language], action) > 1 &&
                   createWordIndex < essayWordIndex && essayWordIndex < compositionWordIndex;
        }

        public override string Run(string action, string language)
        {
            var comp = GetComposition(action, language);
            var author = GetAuthor(action);
            var text = DocumentService.FromFile($"D:\\comp\\{author}\\{comp}.txt");
            var essay = CreateEssay(text);
            
            var path = DocumentService.ToFile(essay, $"{author}_{comp}_essay");

            return "Essay saved at " + path;
        }

        private string GetAuthor(string action)
        {
            var author = new string(action);
            author = Regex.Matches(author, DocumentService.GetWordMatchPattern()).Last().Value;

            return author.Trim();
        }

        private string GetComposition(string action, string language)
        {
            var comp = new string(action);
            var compStartIndex = action.LastIndexOf(_keywords[language].ElementAt(2), StringComparison.Ordinal);
            compStartIndex = action.IndexOf(" ", compStartIndex, StringComparison.Ordinal) + 1;
            var compEndIndex = action.IndexOf(" ", compStartIndex, StringComparison.Ordinal);
            comp = comp.Substring(compStartIndex, compEndIndex == -1
                ? comp.Length - compStartIndex
                : compEndIndex - compStartIndex);

            return comp.Trim();
        }

        private string CreateEssay(string text)
        {
            return new EssayComposer().Compose(text);
        }
    }
}
