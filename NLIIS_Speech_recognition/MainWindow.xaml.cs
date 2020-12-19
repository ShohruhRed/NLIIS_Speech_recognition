using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using NLIIS_Speech_recognition.Services;

namespace NLIIS_Speech_recognition
{
    public partial class MainWindow : Window
    {
        private readonly OpenFileDialog _openFileDialog;
        
        public MainWindow()
        {
            MongoDBConnector.CreateSession("nliis");
            
            _openFileDialog = new OpenFileDialog
            {
                DefaultExt = "txt",
                CheckFileExists = true,
                Multiselect = false
            };

            InitializeComponent();
            Sentences.SelectionChanged += BuildSyntaxTree;
            DocumentService.Language = "English";
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Choose a file to translate" +
                            "\nTranslated - see the actual full text translation to Deutsch" +
                            "\nFrequency list - list of words translations found in DB & text with frequency and grammatical info" +
                            "\nTiny translate list - all words found in text, translated separately and displayed with it's frequencies" +
                            "\nSyntax tree - choose a sentence from original text and get it's syntax tree" +
                            "\n\nThe status of the recent action you can find in the label at bottom", "Help");
        }
        
        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Group 721701:\nSemenikhin Nikita,\nStryzhych Angelika", "Authors");
        }

        private void ProcessDocument(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DocumentToUploadPath.Text))
            {
                SummaryLabel.Content = "Please, specify the filename";

                return;
            }
            
            var text = DocumentService.FromFile(DocumentToUploadPath.Text);
            var translated = Translator.TranslateText(text);

            TranslatedTextBox.Text = translated;
            Sentences.ItemsSource = DocumentService.GetSentences(text);

            var termsFrequencies = DocumentService.GetTermsFrequencies(text)
                .OrderByDescending(pair => pair.Value)
                .ToDictionary(pair => pair.Key, pair => pair.Value);
            var dbItemsTranslated = Translator.TranslateItemsFromDB(text);
            var termsTranslates = Translator.TranslateWords(termsFrequencies.Keys);

            FrequencyListTextBox.ItemsSource = dbItemsTranslated.Select(item => new
            {
                Original = item.Original,
                Translated = item.Translated,
                PartOfSpeech = item.PartOfSpeech,
                Frequency = termsFrequencies.TryGetValue(item.Original, out var frequency) ? frequency.ToString() : "-"
            });
            var tinies = termsTranslates.Select(item => new
            {
                Original = item.Original,
                Translated = item.Translated,
                Frequency = termsFrequencies[item.Original]
            });
            TinyTranslateListTextBox.ItemsSource = tinies;

            var saveTitle = text.Substring(0, text.Length < 10 ? text.Length : 10);
            var saveTextTranslationFilename = $"{saveTitle}__text_translated";
            var saveWordsTranslationFilename = $"{saveTitle}__words_translated";
            var wordsTranlations = string.Join(
                "\n",
                tinies.Select(tiny => tiny.Original + ";" + tiny.Translated + ";" + tiny.Frequency));
            var saveTextTranslationPath = DocumentService.ToFile(translated, saveTextTranslationFilename);
            var saveWordsTranslationPath = DocumentService.ToFile(wordsTranlations, saveWordsTranslationFilename);
            
            SummaryLabel.Content = $"Text translation saved to {saveTextTranslationPath}; words translation saved to {saveWordsTranslationPath}";
        }

        private void UploadTranslations(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(DocumentToUploadPath.Text))
            {
                SummaryLabel.Content = "Please, specify the filename";
                
                return;
            }
            
            DictionaryService.AddFromFile(DocumentToUploadPath.Text);
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            if (_openFileDialog.ShowDialog() == true)
            {
                DocumentToUploadPath.Text = _openFileDialog.FileName;
            }
        }

        private void BuildSyntaxTree(object sender, RoutedEventArgs e)
        {
            var buildFrom = (string) Sentences.SelectedItem;
            
            if (string.IsNullOrEmpty(buildFrom))
            {
                return;
            }
            
            SyntaxTreeBuilder.BuildTree(buildFrom);
            
            var bitmap = new BitmapImage();

            try
            {
                using var imageFileStream = new FileStream(@"D:\tree.jpg", FileMode.Open);
                bitmap.BeginInit();                
                bitmap.StreamSource = imageFileStream;                
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                bitmap.Freeze();
                
                TreeImage.Source = bitmap;
            }
            catch (Exception ex)
            {
                SummaryLabel.Content = $"Something wrong with the image: {ex.Message}";
            }
        }
    }
}
