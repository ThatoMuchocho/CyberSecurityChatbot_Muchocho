using System;
using System.Collections.Generic;
using System.Linq;

namespace CyberSecurityChatbot
{
    /// <summary>
    /// Represents the emotional tone detected in a user message.
    /// Used by the sentiment delegate to tailor empathetic responses.
    /// </summary>
    public enum Sentiment
    {
        Neutral,
        Worried,
        Curious,
        Frustrated
    }

    /// <summary>
    /// Data model for a single chat exchange, bound to the WPF message list.
    /// </summary>
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string MessageText { get; set; }
        public DateTime Timestamp { get; set; }

        /// <summary>True when the message was sent by the user (right-aligned bubble).</summary>
        public bool IsUser => Sender == "User";

        public string BubbleBackground => IsUser ? "#0E4249" : "#1F242C";
        public string BubbleBorder => IsUser ? "#00ADB5" : "#30363D";
        public string BubbleMargin => IsUser ? "60,4,10,4" : "10,4,60,4";

        public ChatMessage(string sender, string messageText)
        {
            Sender = sender;
            MessageText = messageText;
            Timestamp = DateTime.Now;
        }
    }

    /// <summary>Delegate: analyses user text and returns a detected sentiment.</summary>
    public delegate Sentiment SentimentAnalyzerDelegate(string input);

    /// <summary>Delegate: wraps a base cybersecurity tip with mood-appropriate phrasing.</summary>
    public delegate string ResponseTransformerDelegate(string baseResponse, Sentiment sentiment);

    /// <summary>
    /// Core chatbot logic: keyword recognition, random tips, memory, sentiment, and conversation flow.
    /// Uses generic collections (Dictionary, List) and delegates as required by the assignment brief.
    /// </summary>
    public class ChatbotEngine
    {
        private readonly Dictionary<string, List<string>> _topicTips;
        private readonly Dictionary<string, string> _keywordToTopic;
        private readonly List<ChatMessage> _history;
        private readonly Dictionary<Sentiment, List<string>> _sentimentKeywords;
        private readonly List<string> _followUpTriggers;
        private readonly Random _random = new Random();

        /// <summary>Delegate instance for sentiment detection (swappable for testing/extension).</summary>
        public SentimentAnalyzerDelegate AnalyzeSentiment { get; set; }

        /// <summary>Delegate instance for tailoring responses to detected mood.</summary>
        public ResponseTransformerDelegate TransformResponse { get; set; }

        public string? UserName { get; set; }
        public string? FavoriteTopic { get; set; }
        public string LastDiscussedTopic { get; set; } = "general";

        public ChatbotEngine()
        {
            _history = new List<ChatMessage>();
            _topicTips = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            _keywordToTopic = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _sentimentKeywords = new Dictionary<Sentiment, List<string>>();
            _followUpTriggers = new List<string>
            {
                "another tip", "tell me more", "explain more", "give me more",
                "more tips", "explain", "elaborate", "another one", "what else",
                "go on", "continue"
            };

            InitializeTopicTips();
            InitializeKeywordMap();
            InitializeSentimentKeywords();

            AnalyzeSentiment = DefaultSentimentAnalyzer;
            TransformResponse = DefaultResponseTransformer;
        }

        /// <summary>Main entry point: processes user input and returns the bot reply.</summary>
        public string ProcessInput(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
            {
                return "Please type something so I can help you stay secure!";
            }

            _history.Add(new ChatMessage("User", userInput));
            string lowerInput = userInput.ToLower();

            // Memory: capture name
            if (lowerInput.Contains("my name is ") || lowerInput.Contains("i am ") || lowerInput.Contains("i'm "))
            {
                string name = ExtractName(userInput);
                if (!string.IsNullOrEmpty(name))
                {
                    UserName = name;
                    string reply = $"Nice to meet you, {UserName}! I will remember your name. What cybersecurity topic are you most interested in? (e.g. passwords, privacy, scams, phishing)";
                    LogBotReply(reply);
                    return reply;
                }
            }

            // Memory: capture stated interest
            string interestTopic = DetectTopicInterest(lowerInput);
            if (!string.IsNullOrEmpty(interestTopic))
            {
                FavoriteTopic = interestTopic;
                LastDiscussedTopic = interestTopic;
                string reply = $"Great! I'll remember that you're interested in {FavoriteTopic}. It's a crucial part of staying safe online.";
                LogBotReply(reply);
                return reply;
            }

            // Delegate: detect sentiment before building the response
            Sentiment userSentiment = AnalyzeSentiment(userInput);

            string matchedTopic = MatchTopicFromKeywords(lowerInput);
            bool isFollowUp = IsFollowUpQuery(lowerInput);

            if (isFollowUp && matchedTopic == "")
            {
                matchedTopic = LastDiscussedTopic;
            }

            if (!string.IsNullOrEmpty(matchedTopic) && _topicTips.ContainsKey(matchedTopic))
            {
                LastDiscussedTopic = matchedTopic;
                string randomTip = PickRandomTip(matchedTopic);
                randomTip = ApplyMemoryPersonalization(randomTip, matchedTopic, isFollowUp, userSentiment);

                // Delegate: transform tip with sentiment-aware phrasing
                string finalReply = TransformResponse(randomTip, userSentiment);
                LogBotReply(finalReply);
                return finalReply;
            }

            // Proactive memory recall when no keyword matched
            string? memoryReply = TryMemoryRecallResponse(lowerInput);
            if (memoryReply != null)
            {
                LogBotReply(memoryReply);
                return memoryReply;
            }

            // Greetings
            if (ContainsAny(lowerInput, "hello", "hi", "hey", "good morning", "good afternoon"))
            {
                string greeting = !string.IsNullOrEmpty(UserName)
                    ? $"Hello again, {UserName}! How can I help you secure your digital life today? Ask about passwords, scams, privacy, or phishing."
                    : "Hello! I am your Cybersecurity Awareness Chatbot. Tell me your name, or ask about password safety, online scams, privacy, or phishing tips.";
                LogBotReply(greeting);
                return greeting;
            }

            // Fallback — must not crash on unknown input
            string fallback = BuildFallbackMessage();
            LogBotReply(fallback);
            return fallback;
        }

        public List<ChatMessage> GetChatHistory() => _history;

        private void InitializeTopicTips()
        {
            _topicTips["password"] = new List<string>
            {
                "Make sure to use strong, unique passwords for each account. Avoid using personal details in your passwords.",
                "Use a password manager to securely store and generate complex passwords for all your online profiles.",
                "Enable Multi-Factor Authentication (MFA) wherever possible. It adds a crucial second layer of security beyond just a password.",
                "Never share your passwords with anyone, and change them immediately if you suspect a breach."
            };

            _topicTips["scam"] = new List<string>
            {
                "Always verify the sender's email address and identity before clicking links or providing personal information online.",
                "If an offer sounds too good to be true, such as winning a lottery you never entered, it is almost certainly a scam.",
                "Scammers often create a false sense of urgency. Take your time and verify the claim independently before acting.",
                "Never download attachments from unsolicited emails. They frequently contain malware or spyware."
            };

            _topicTips["privacy"] = new List<string>
            {
                "Review and tighten the privacy settings on all your social media accounts to limit what strangers can see about you.",
                "Be mindful of what you post online. Personal details can be used by attackers to answer your security questions.",
                "Use a Virtual Private Network (VPN) when browsing on public Wi-Fi to protect your data from interception.",
                "Regularly check app permissions on your devices and disable location, microphone, or camera access for apps that do not need them."
            };

            _topicTips["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Hover over hyperlinks in emails to inspect the actual destination URL before clicking. Look out for subtle spelling errors.",
                "Authentic organisations will rarely ask you to verify sensitive credentials via a direct email link.",
                "Use a browser with phishing protection and keep your security software updated to block malicious pages."
            };
        }

        /// <summary>Maps many synonyms to the four core cybersecurity topics.</summary>
        private void InitializeKeywordMap()
        {
            void Map(string keyword, string topic) => _keywordToTopic[keyword] = topic;

            Map("password", "password");
            Map("passcode", "password");
            Map("passkey", "password");
            Map("credential", "password");
            Map("login", "password");

            Map("scam", "scam");
            Map("fraud", "scam");
            Map("con artist", "scam");
            Map("identity theft", "scam");

            Map("privacy", "privacy");
            Map("private data", "privacy");
            Map("personal data", "privacy");
            Map("data protection", "privacy");

            Map("phishing", "phishing");
            Map("fake email", "phishing");
            Map("suspicious link", "phishing");
            Map("spoof", "phishing");
        }

        private void InitializeSentimentKeywords()
        {
            _sentimentKeywords[Sentiment.Worried] = new List<string>
            {
                "worried", "scared", "fear", "anxious", "nervous", "afraid",
                "panic", "threatened", "overwhelmed", "unsafe", "concerned"
            };
            _sentimentKeywords[Sentiment.Curious] = new List<string>
            {
                "curious", "wonder", "why", "how", "explain", "learn",
                "interested", "tell me", "what is", "help me understand"
            };
            _sentimentKeywords[Sentiment.Frustrated] = new List<string>
            {
                "frustrated", "annoyed", "hate", "angry", "mad", "broken",
                "stuck", "confused", "fed up", "sick of"
            };
        }

        private Sentiment DefaultSentimentAnalyzer(string input)
        {
            if (string.IsNullOrEmpty(input)) return Sentiment.Neutral;

            string lowerInput = input.ToLower();

            // Worried takes priority when user expresses fear about security topics
            foreach (var keyword in _sentimentKeywords[Sentiment.Worried])
            {
                if (lowerInput.Contains(keyword)) return Sentiment.Worried;
            }
            foreach (var keyword in _sentimentKeywords[Sentiment.Frustrated])
            {
                if (lowerInput.Contains(keyword)) return Sentiment.Frustrated;
            }
            foreach (var keyword in _sentimentKeywords[Sentiment.Curious])
            {
                if (lowerInput.Contains(keyword)) return Sentiment.Curious;
            }

            return Sentiment.Neutral;
        }

        private string DefaultResponseTransformer(string baseResponse, Sentiment sentiment)
        {
            switch (sentiment)
            {
                case Sentiment.Worried:
                    // Matches assignment example: empathy + immediate tip in one turn
                    return "It's completely understandable to feel that way. Scammers and online threats can be very convincing. Let me share some tips to help you stay safe:\n\n" + baseResponse;

                case Sentiment.Curious:
                    return "I love your curiosity — learning is the best defence in cybersecurity. Here is a helpful insight:\n\n" + baseResponse;

                case Sentiment.Frustrated:
                    return "I understand how frustrating security can feel when things go wrong. Take a breath — here is a simple, actionable tip:\n\n" + baseResponse;

                default:
                    if (!string.IsNullOrEmpty(UserName) && _random.Next(3) == 0)
                    {
                        return $"Hi {UserName}, here is a security tip for you:\n\n" + baseResponse;
                    }
                    return baseResponse;
            }
        }

        private string MatchTopicFromKeywords(string lowerInput)
        {
            foreach (var entry in _keywordToTopic)
            {
                if (lowerInput.Contains(entry.Key))
                {
                    return entry.Value;
                }
            }
            return "";
        }

        private string PickRandomTip(string topic)
        {
            var tips = _topicTips[topic];
            return tips[_random.Next(tips.Count)];
        }

        private string ApplyMemoryPersonalization(string tip, string topic, bool isFollowUp, Sentiment sentiment)
        {
            if (sentiment != Sentiment.Neutral) return tip;

            if (!string.IsNullOrEmpty(FavoriteTopic) && topic == FavoriteTopic)
            {
                if (!string.IsNullOrEmpty(UserName))
                {
                    return $"As someone interested in {FavoriteTopic}, {UserName}, you might find this useful: {tip}";
                }
                return $"As someone interested in {FavoriteTopic}, you might find this useful: {tip}";
            }

            if (!string.IsNullOrEmpty(UserName) && !isFollowUp && _random.Next(3) == 0)
            {
                return $"Well, {UserName}, here is something you should consider: {tip}";
            }

            return tip;
        }

        /// <summary>Recalls stored name or interest when the user asks general questions later.</summary>
        private string? TryMemoryRecallResponse(string lowerInput)
        {
            if (!string.IsNullOrEmpty(FavoriteTopic) &&
                ContainsAny(lowerInput, "remember", "recall", "what do you know", "what did i say", "my interest"))
            {
                string namePart = !string.IsNullOrEmpty(UserName) ? $"{UserName}, you" : "You";
                return $"{namePart} told me you are interested in {FavoriteTopic}. Would you like another tip on that topic?";
            }

            if (!string.IsNullOrEmpty(FavoriteTopic) &&
                ContainsAny(lowerInput, "recommend", "suggest", "what should i", "any advice", "help me"))
            {
                string tip = PickRandomTip(FavoriteTopic);
                return $"Since you are interested in {FavoriteTopic}, here is personalised advice: {tip}";
            }

            return null;
        }

        private string BuildFallbackMessage()
        {
            if (!string.IsNullOrEmpty(FavoriteTopic))
            {
                return "I'm not sure I understand. Can you try rephrasing? You can also ask for 'another tip' or ask about passwords, scams, privacy, or phishing.";
            }
            return "I'm not sure I understand. Can you try rephrasing? Or ask about 'password safety', 'online scams', 'privacy tips', or 'give me a phishing tip'.";
        }

        private string ExtractName(string input)
        {
            string lower = input.ToLower();
            string[] triggers = { "my name is ", "i am ", "i'm " };

            foreach (var trigger in triggers)
            {
                int index = lower.IndexOf(trigger);
                if (index == -1) continue;

                string rawName = input.Substring(index + trigger.Length).Trim();
                char[] trimChars = { '.', '!', '?', ' ', ',' };
                rawName = rawName.Trim(trimChars);

                string[] words = rawName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (words.Length > 0)
                {
                    string first = words[0];
                    if (first.Equals("interested", StringComparison.OrdinalIgnoreCase) ||
                        first.Equals("worried", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    return char.ToUpper(first[0]) + first.Substring(1).ToLower();
                }
            }
            return "";
        }

        private string DetectTopicInterest(string lowerInput)
        {
            bool declaresInterest = lowerInput.Contains("interested in ") ||
                                   lowerInput.Contains("like ") && lowerInput.Contains(" topic") ||
                                   lowerInput.Contains("favorite topic") ||
                                   lowerInput.Contains("want to learn about ") ||
                                   lowerInput.Contains("want to know about ");

            if (!declaresInterest) return "";

            foreach (var topic in _topicTips.Keys)
            {
                if (lowerInput.Contains(topic)) return topic;
            }
            return "";
        }

        private bool IsFollowUpQuery(string lowerInput) =>
            _followUpTriggers.Any(trigger => lowerInput.Contains(trigger));

        private static bool ContainsAny(string input, params string[] terms) =>
            terms.Any(term => input.Contains(term));

        private void LogBotReply(string reply) =>
            _history.Add(new ChatMessage("Chatbot", reply));
    }
}
