using System.Windows;
using System.Windows.Controls;
using NLIIS_Speech_recognition.Services;

namespace NLIIS_Speech_recognition
{
    public partial class MainWindow : Window
    {
        private readonly SpeechRecognizer _speechRecognizer;
        private readonly AudioRecorder _audioRecorder;
        private readonly Actor _actor;
        
        public MainWindow()
        {
            _speechRecognizer = new SpeechRecognizer();
            _audioRecorder = new AudioRecorder();
            _actor = new Actor();
            
            InitializeComponent();

            _speechRecognizer.Language = "Russian";
        }

        private void Help_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Click Start to start recording of a command\nClick Stop to stop the recording\n" +
                            "Then click Analyze to see the text of the command recognized\n" +
                            "If command can be performed, Act button will start corresponding action\n\n" +
                            "Supported actions:\n" +
                            "\tPlay authors composition (включи произведения Чехова)\n" +
                            "\tCreating note about authors (создай заметку об авторе Пушкин)\n" +
                            "\tCreating an essay of author's composition (создай реферат по произведению Сильмариллион Толкина)", "Help");
        }
        
        private void Authors_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Group 721701:\nSemenikhin Nikita,\nStryzhych Angelika\n\nADDED speech synthesis by students of group 921731", "Authors");
        }

        private void ChangeAnalyzerLanguage(object sender, RoutedEventArgs e)
        {
            _speechRecognizer.Language = ((ComboBoxItem)LanguageSelect.SelectedItem).Content as string;
            DocumentService.Language = _speechRecognizer.Language;
        }

        private void StartRec(object sender, RoutedEventArgs e)
        {
            _audioRecorder.StartRecording(sender, e);
        }

        private void StopRec(object sender, RoutedEventArgs e)
        {
            _audioRecorder.StopRec(sender, e);
        }

        private void Analyze(object sender, RoutedEventArgs e)
        {
            SummaryLabel.Content = _speechRecognizer.ToText();
        }

        private void Act(object sender, RoutedEventArgs e)
        {
            SummaryLabel.Content = _actor.Act(_speechRecognizer.LastRecognized.ToLower(), _speechRecognizer.Language);
        }
    }
}
