# CyberSentry — Cybersecurity Awareness Chatbot (Part 2)

**CyberSentry** is a WPF desktop chatbot (.NET 8) that teaches cybersecurity through an interactive GUI. It meets the Part 2 brief: keyword recognition, random responses, conversation flow, memory, sentiment detection, delegates, generic collections, voice greeting, and ASCII art.

---

## Submission checklist (rubric alignment)

| Requirement | Location in project |
|-------------|---------------------|
| WPF GUI (all interaction via GUI) | `MainWindow.xaml`, `MainWindow.xaml.cs` |
| Voice greeting (WAV file in repo) | `Assets/welcome.wav` |
| ASCII art (file in repo + GUI panel) | `Assets/ascii_art.txt`, sidebar in `MainWindow.xaml` |
| ≥3 keywords (password, scam, privacy) + phishing | `ChatbotEngine.cs` → `_keywordToTopic`, `_topicTips` |
| Random responses per topic | `PickRandomTip()` using `List<string>` per topic |
| Conversation flow (follow-ups) | `IsFollowUpQuery()`, `LastDiscussedTopic` |
| Memory (name + interest) | `UserName`, `FavoriteTopic`, sidebar labels |
| Sentiment (worried, curious, frustrated) | `Sentiment` enum + `SentimentAnalyzerDelegate` |
| Delegates | `SentimentAnalyzerDelegate`, `ResponseTransformerDelegate` |
| Generic collections | `Dictionary<string, List<string>>`, `List<ChatMessage>`, etc. |
| Error handling / no crashes | `try/catch` in audio; fallback messages in `ProcessInput` |
| OOP (classes + methods) | `ChatbotEngine`, `ChatMessage`, `MainWindow` |
| GitHub: ≥6 commits, ≥3 tags | Run `setup_git.ps1` or commit manually (see below) |
| YouTube presentation (unlisted) | Add your link below |

### YouTube presentation link

Replace this placeholder with your unlisted video URL before submitting on ARC:

`https://youtu.be/YOUR_VIDEO_ID_HERE`

---

## Setup and run

**Prerequisites:** [.NET 8 SDK](https://dotnet.microsoft.com/download) on Windows.

```powershell
cd CyberSecurityChatbot
dotnet build
dotnet run
```

On first launch, the app plays `Assets/welcome.wav` and shows ASCII art from `Assets/ascii_art.txt`.

---

## Example conversations (match assignment brief)

**Keyword — password**
- User: `Tell me about password safety.`
- Bot: Random tip from the password list (e.g. strong unique passwords).

**Random phishing tip**
- User: `Give me a phishing tip.`
- Bot: Randomly selects one of several phishing responses.

**Memory**
- User: `I'm interested in privacy.`
- Bot: `Great! I'll remember that you're interested in privacy...`
- Later — User: `Any advice for me?`
- Bot: Recalls privacy interest and gives a personalised tip.

**Sentiment + immediate tip (no second prompt)**
- User: `I'm worried about online scams.`
- Bot: Empathetic opening **and** a scam safety tip in the same reply.

**Follow-up flow**
- User: `Tell me about scams.` → (tip)
- User: `Give me another tip.` → Another random scam tip without resetting.

**Unknown input**
- User: `blah blah xyz`
- Bot: `I'm not sure I understand. Can you try rephrasing?...` (app does not crash)

---

## Project structure

```
CyberSecurityChatbot/
├── Assets/
│   ├── welcome.wav          # Voice greeting (submission file)
│   └── ascii_art.txt        # ASCII shield art (submission file)
├── App.xaml                 # Theme, colours, control styles
├── ChatbotEngine.cs         # Core logic: keywords, memory, sentiment, delegates
├── MainWindow.xaml          # GUI layout
├── MainWindow.xaml.cs       # UI events, audio, ASCII load
├── WavGenerator.cs          # Generates WAV if missing at runtime
├── README.md
└── setup_git.ps1            # Optional: creates 6 commits + 3 tags locally
```

---

## Technical features (for your video)

1. **Generic collections** — `Dictionary<string, List<string>>` for topic tips; `Dictionary<string, string>` for keyword synonyms; `List<ChatMessage>` for history.
2. **Delegates** — `AnalyzeSentiment` and `TransformResponse` separate detection from response formatting.
3. **Sentiment** — Worried/curious/frustrated keywords adjust tone; worried + topic returns empathy **and** a tip in one turn.
4. **Memory** — Name and favourite topic stored and shown in the sidebar; recalled in later replies.
5. **GUI** — Dark theme, chat bubbles (user right / bot left), memory panel, replay audio button.

---

## GitHub workflow (9–10 marks)

1. Create a GitHub repository and push this folder.
2. Make **at least 6 meaningful commits** (e.g. scaffold → engine → audio → UI → assets → README).
3. Create **at least 3 annotated tags** with release notes, e.g. `v1.0.0-alpha`, `v1.0.0-beta`, `v1.0.0`.
4. Submit the GitHub link on ARC and add your YouTube link above.

**Quick local history (if Git is installed):**

```powershell
cd CyberSecurityChatbot
.\setup_git.ps1
```

Then add your remote and push:

```powershell
git remote add origin https://github.com/YOUR_USERNAME/CyberSecurityChatbot.git
git push -u origin main --tags
```

---

## Author notes

Built for PROG6212 Part 2 — GUI, dynamic responses, sentiment, and memory. Task 1 features (ASCII art + voice greeting) are integrated into the WPF interface.
