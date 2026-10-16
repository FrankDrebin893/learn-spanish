# AGENTS.md - Project Guidelines for AI Coding Assistants

Welcome! This repository is designed to be agent-agnostic. Whether you are working via Google Antigravity, Claude Code, Cursor, GitHub Copilot, Codex, or another AI assistant, follow the guidelines below.

---

## 1. Project Overview

**learn-spanish** (Aprende Español) is a modern, standalone web application for mastering Spanish vocabulary. It uses vocabulary frequency rankings and Wiktionary definitions from the `doozan/spanish_data` dataset.

### Key Capabilities
- **Learn Mode**: Spaced repetition flashcards ordered by natural frequency, featuring native pronunciation audio and example sentences.
- **Quiz Mode**: Interactive multiple-choice testing (Spanish-to-English, English-to-Spanish, and mixed) with keyboard shortcuts and review sessions.
- **Statistics**: Granular vocabulary mastery tracking, accuracy metrics, and performance filters.
- **Offline & GitHub Pages Ready**: Fully client-side Blazor WebAssembly standalone application with browser `localStorage` persistence. No backend server required.

---

## 2. Architecture & Technology Stack

- **Framework**: .NET 9 Blazor WebAssembly Standalone (`Microsoft.NET.Sdk.BlazorWebAssembly`).
- **UI Component Library**: MudBlazor 8 (Material Design tailored with custom *Tierra y Sol* theme tokens).
- **Persistence**: Browser `localStorage` via Blazor `IJSRuntime` (`Services/ProgressService.cs`).
- **Vocabulary Data**: Pre-processed high-frequency Spanish vocabulary stored in `wwwroot/data/words.json` with embedded fallback data in `Services/WordService.cs`.
- **Hosting**: Static web hosting compatible with **GitHub Pages**, Cloudflare Pages, Netlify, or Vercel.

---

## 3. Key Commands

```bash
# Build the project
dotnet build SpanishWordLearningApp.sln

# Run locally in development (starts hot-reload dev server)
dotnet run --project SpanishWordLearner.csproj

# Run automated tests
dotnet test Tests/SpanishWordLearner.Tests.csproj

# Publish for production / GitHub Pages
dotnet publish SpanishWordLearner.csproj -c Release -o publish
```

---

## 4. Repository Structure

```
learn-spanish/
├── .agents/                 # Open standard agent configuration & skills
│   └── skills/impeccable/  # Impeccable frontend design skill & launcher
├── .github/
│   └── workflows/          # GitHub Actions deployment workflow for GitHub Pages
├── Components/
│   ├── Layout/             # App shell, navigation, header, theme definition
│   ├── Pages/              # Razor pages (Home, Learn, Quiz, Statistics)
│   ├── App.razor           # Root Blazor component
│   └── Routes.razor        # Client-side router configuration
├── Models/                 # Domain models (SpanishWord, UserProgress, QuizSession)
├── Services/               # WordService, ProgressService, DataLoader
├── Tests/                  # xUnit automated test suite
├── wwwroot/                # Static assets, index.html, 404.html, data/words.json
├── AGENTS.md               # Unified instructions for all agents (this file)
├── CLAUDE.md               # Claude Code compatibility pointer
├── PRODUCT.md              # Product context & durable design truth
├── DESIGN.md               # Visual design tokens & styling guidelines
└── SpanishWordLearner.csproj # Project definition
```

---

## 5. Coding Standards & Conventions

1. **C# & .NET 9**:
   - Use file-scoped namespaces.
   - Prefer `required` properties and records for data contracts.
   - Keep asynchronous methods non-blocking with proper cancellation tokens when applicable.
2. **Razor & Blazor Components**:
   - Keep components focused and single-responsibility.
   - Use dependency injection via `@inject`.
   - Prefer MudBlazor component parameters and consistent semantic styling over inline CSS styles.
   - Handle nullability cleanly without suppressing compiler warnings.
3. **Browser Persistence**:
   - Always persist through `ProgressService`, which synchronizes state with browser `localStorage`.
   - Ensure serialization is resilient against schema upgrades.

---

## 6. Git & Contribution Workflow

- Maintain a clean `git status`. Never commit machine-specific local settings or build artifacts.
- Write descriptive, atomic commit messages in conventional commit format.
- Ensure all tests pass (`dotnet test Tests/SpanishWordLearner.Tests.csproj`) before committing changes.
