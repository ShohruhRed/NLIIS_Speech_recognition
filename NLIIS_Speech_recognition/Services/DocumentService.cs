using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NLIIS_Speech_recognition.Services
{
    public static class DocumentService
    {
        public static string Language { get; set; }
        
        public static string FromFile(string path)
        {
            return File.ReadAllText(path);
        }
        
        public static string ToFile(string text, string name)
        {
            var path = $"D:\\{name}.txt";
            using var file = new StreamWriter(path);
            
            file.WriteLine(text);

            return path;
        }
        
        public static IEnumerable<string> GetWords(string text)
        {
            var pattern = GetWordMatchPattern();
            var words = Regex.Matches(text, pattern)
                .Select(match => match.Value.ToLower());

            return words;
        }
        
        public static IEnumerable<string> GetSentences(string text)
        {
            var sentences = text.Replace("\r", string.Empty)
                .Split("\n")
                .Where(sentence => !sentence.Equals(string.Empty));

            return sentences;
        }

        public static IEnumerable<string> GetTerms(string text, bool removeUseless)
        {
            var wordsEntries = GetWords(text);
            var uniqueWords = wordsEntries.ToHashSet().AsEnumerable();
            
            return uniqueWords;
        }
        
        public static IDictionary<string, int> GetTermsFrequencies(string text)
        {
            var wordsEntries = GetWords(text);
            var termsFrequencies = new Dictionary<string, int>();

            foreach (var wordEntry in wordsEntries)
            {
                var currentWeight = 0;
                
                if (termsFrequencies.ContainsKey(wordEntry))
                {
                    termsFrequencies.TryGetValue(wordEntry, out currentWeight);
                    termsFrequencies.Remove(wordEntry);
                }
                
                termsFrequencies.Add(wordEntry, ++currentWeight);
            }

            return termsFrequencies;
        }

        public static string GetWordMatchPattern()
        {
            return Language switch
            {
                "English" => "[a-zA-Z\\-]{3,}",
                "Deutsch" => "[a-zA-ZäöüÄÖÜß\\-]{3,}",
                _ => throw new ArgumentException($"Language {Language} is not supported")
            };
        }
    }
}
