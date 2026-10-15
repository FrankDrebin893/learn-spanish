# Aprende Español - Spanish Word Learning App

A modern, standalone Blazor WebAssembly application for mastering Spanish vocabulary through empirical frequency rankings, native speech audio, interactive flashcards, and quizzes.

Designed with the **Tierra y Sol** design system using MudBlazor 8 and ready for static hosting on **GitHub Pages**.

---

## 🌟 Key Capabilities

### 1. Learn Mode (Flashcards)
- Spaced study ordered by natural usage frequency (Top 100, Top 500, up to 10,000 words).
- **Native Pronunciation Audio**: Hear correct Spanish pronunciation using browser speech synthesis (`window.speechSynthesis`).
- Flip cards to reveal English definitions and example sentences.
- Keyboard accelerated: `Space`/`Enter` to reveal, `1` for Still Learning, `2` for Got It, `←`/`→` to navigate.

### 2. Practice & Quiz Mode
- Multiple-choice testing: Spanish-to-English, English-to-Spanish, or Mixed.
- Configurable question counts: 5, 10, 20, or 50 questions.
- Fast keyboard navigation: press `1`–`4` to answer, `Enter` to advance.
- Live streak tracking, immediate audio pronunciation, and end-of-quiz scorecards with review of missed words.

### 3. Detailed Progress & Analytics
- Complete mastery tracking: words are considered "Dominadas" (Learned) after 3+ correct reviews with 70%+ accuracy.
- Granular performance filters: All, Dominadas (Learned), and Difíciles (Struggling).
- Individual word performance table with direct audio playback.

### 4. Zero Backend & GitHub Pages Ready
- **100% Client-Side**: Built with .NET 9 Blazor WebAssembly Standalone (`Microsoft.NET.Sdk.BlazorWebAssembly`).
- **Browser Memory Persistence**: Stores all user progress, accuracy, and quiz history locally in `window.localStorage` (`spanish_word_learner_progress`).
- **Instant Offline Access**: No account, login, or database server required.
- **Automated Deployment**: GitHub Actions workflow included for instant publication to GitHub Pages.

---

## 🎨 Visual Design System: *Tierra y Sol*

- Warm Iberian color palette: Seville Terracotta (`#D9483B`), Saffron Gold (`#E0872E`), Forest Olive (`#16A34A`), and Soft Parchment Canvas (`#FAF8F5`).
- Modern typography powered by Google Fonts `Outfit`.
- Tactile, accessible interactive components designed in adherence with the **Impeccable** design standard.

---

## 🤖 Agent Agnostic Repository

This codebase is agent-agnostic. AI coding assistants (Google Antigravity, Claude Code, Cursor, GitHub Copilot, Codex, etc.) follow the standardized instructions in [AGENTS.md](AGENTS.md).

- **Unified Guidelines**: [AGENTS.md](AGENTS.md)
- **Design Specifications**: [DESIGN.md](DESIGN.md)
- **Product Context**: [PRODUCT.md](PRODUCT.md)
- **Skills**: [.agents/skills/impeccable/](.agents/skills/impeccable/SKILL.md)

---

## 🚀 Running Locally

```bash
# Clone the repository with submodules
git clone --recurse-submodules <repository-url>
cd SpanishWordLearningApp

# Run the development server with hot-reload
dotnet run --project SpanishWordLearner.csproj
```

Open `http://localhost:5062` in your browser.

---

## 🧪 Automated Testing

```bash
dotnet test Tests/SpanishWordLearner.Tests.csproj
```

---

## 📦 Publishing to GitHub Pages

### Automatic (Recommended)
Push to the `main` branch. The included GitHub Actions workflow (`.github/workflows/deploy.yml`) builds and deploys the static WebAssembly bundle automatically.

### Manual Build
```bash
dotnet publish SpanishWordLearner.csproj -c Release -o publish
```
The contents of `publish/wwwroot/` can be hosted on GitHub Pages, Cloudflare Pages, Netlify, or Vercel.

---

## 📚 Data Attribution

Word frequency rankings and Wiktionary definitions are sourced from the [doozan/spanish_data](https://github.com/doozan/spanish_data) repository:
- **Frequency Data**: Lemmatized frequency lists from [FrequencyWords](https://github.com/hermitdave/FrequencyWords) (CC-BY-SA 3.0).
- **Translations**: Wiktionary Spanish-to-English entries (CC-BY-SA).
