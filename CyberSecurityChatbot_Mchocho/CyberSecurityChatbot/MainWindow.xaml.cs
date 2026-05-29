using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// WPF main window: chat UI, ASCII art panel, voice greeting, and memory sidebar.
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ChatbotEngine _engine;
        private readonly ObservableCollection<ChatMessage> _messages;

        private const string DefaultAsciiArt =
            "       .-------------.\n" +
            "      /               \\\n" +
            "     |   CYBERSENTRY   |\n" +
            "     |   ===========   |\n" +
            "     |      /|█|\\      |\n" +
            "     |     (  _  )     |\n" +
            "      \\     \\_|_/     /\n" +
            "       \\             /\n" +
            "        \\    [OK]   /\n" +
            "         \\         /\n" +
            "          \\       /\n" +
            "           \\     /\n" +
            "            \\   /\n" +
            "             \\_/";

        public MainWindow()
        {
            InitializeComponent();

            _engine = new ChatbotEngine();
            _messages = new ObservableCollection<ChatMessage>();
            LstChat.ItemsSource = _messages;

            TxtAsciiArt.Text = LoadAsciiArt();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            PlayWelcomeAudio();

            string welcomeMsg =
                "Welcome to CyberSentry! A voice greeting has played.\n\n" +
                "Tell me your name, say you are interested in a topic (e.g. privacy), " +
                "or ask about password safety, online scams, privacy, or phishing tips.";

            _messages.Add(new ChatMessage("Chatbot", welcomeMsg));
            TxtInput.Focus();
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e) => SendMessage();

        private void TxtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SendMessage();
        }

        private void BtnPlayAudio_Click(object sender, RoutedEventArgs e) => PlayWelcomeAudio();

        private void SendMessage()
        {
            string input = TxtInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(input)) return;

            _messages.Add(new ChatMessage("User", input));
            TxtInput.Clear();

            string botResponse = _engine.ProcessInput(input);
            _messages.Add(new ChatMessage("Chatbot", botResponse));

            UpdateMemoryUI();
            ScrollChatToBottom();
        }

        private void UpdateMemoryUI()
        {
            LblUserName.Text = !string.IsNullOrEmpty(_engine.UserName)
                ? _engine.UserName
                : "Not Provided Yet";

            LblInterest.Text = !string.IsNullOrEmpty(_engine.FavoriteTopic)
                ? ToTitleCase(_engine.FavoriteTopic)
                : "Not Provided Yet";

            LblLastTopic.Text = ToTitleCase(_engine.LastDiscussedTopic);
        }

        /// <summary>Plays welcome.wav from Assets (Task 1 voice greeting requirement).</summary>
        private void PlayWelcomeAudio()
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string wavPath = Path.Combine(baseDir, "Assets", "welcome.wav");

                // Fallback: generate if missing (e.g. first run before copy)
                WavGenerator.GenerateWelcomeWav(wavPath);

                if (!File.Exists(wavPath))
                {
                    wavPath = Path.Combine(baseDir, "welcome.wav");
                    WavGenerator.GenerateWelcomeWav(wavPath);
                }

                if (File.Exists(wavPath))
                {
                    using var player = new System.Media.SoundPlayer(wavPath);
                    player.Play();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Audio playback error: {ex.Message}");
            }
        }

        /// <summary>Loads ASCII art from the repository file required for submission.</summary>
        private static string LoadAsciiArt()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ascii_art.txt");
                if (File.Exists(path))
                {
                    return File.ReadAllText(path);
                }
            }
            catch
            {
                // Fall back to embedded art — app must not crash
            }
            return DefaultAsciiArt;
        }

        private void ScrollChatToBottom()
        {
            if (_messages.Count > 0)
            {
                LstChat.ScrollIntoView(_messages[_messages.Count - 1]);
            }
        }

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return char.ToUpper(input[0]) + input.Substring(1).ToLower();
        }
    }
}
