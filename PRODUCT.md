# PRODUCT.md - Spanish Word Learning App

## 1. Product Truth & Purpose

**Spanish Word Learning App** is a fast, responsive, and privacy-respecting web application designed to help language learners acquire and retain Spanish vocabulary efficiently.

Instead of arbitrary word lists, the curriculum is prioritized by empirical linguistic frequency (based on the `doozan/spanish_data` frequency dataset and Wiktionary definitions). Learners study the most common words first, gaining immediate conversational comprehension.

---

## 2. Target Audience & Personas

- **Autonomous Spanish Learners (A1 - B2)**: Students, professionals, and travelers wanting a frictionless, ad-free flashcard and quiz tool.
- **Micro-Learning Sessions**: Users studying on mobile or desktop during 2-to-10 minute daily study intervals.
- **Privacy-Conscious Users**: Users who prefer zero logins, zero tracking, and local device-only data persistence.

---

## 3. Core Features & User Journeys

1. **Dashboard (Home)**:
   - Daily motivation, vocabulary mastery progress bar, and summary stats (Learned, Seen, Quizzes taken).
   - Instant jump actions: "Start Learning" and "Take a Quiz".
2. **Flashcards (Learn Mode)**:
   - Ordered by natural frequency ranking (`#1 de`, `#2 que`, etc.).
   - Dual-side recall: Spanish word & part-of-speech chip -> reveal English gloss, definitions, and usage examples.
   - Built-in native Spanish pronunciation audio via browser Web Speech API.
   - Self-assessment feedback: "Still Learning" vs "Got It!", updating learning retention.
   - Keyboard shortcuts: `Space`/`Enter` to flip, `1` / `2` to rate, Arrow keys to navigate.
3. **Interactive Quizzes (Quiz Mode)**:
   - Multiple choice tests: Spanish-to-English, English-to-Spanish, or Mixed.
   - Configurable question sets (5, 10, 20, 50 questions).
   - Real-time score feedback, streak counter, and final performance scorecard with accuracy rating.
   - Keyboard accelerated: `1` - `4` for option selection, `Enter` to advance.
4. **Performance Analytics (Statistics)**:
   - Granular breakdown of word performance.
   - Filter words by status: All, Learned, Struggling, or Unseen.
   - Option to reset progress or export/import in future updates.

---

## 4. Operating Context & Technical Constraints

- **Hosting**: Static file hosting on **GitHub Pages** (Single Page Application via Blazor WebAssembly).
- **Data Persistence**: 100% in-browser storage using `window.localStorage` (keyed under `spanish_word_learner_progress`).
- **Data Source**: Pre-bundled JSON payload containing 10,000 frequency-ranked vocabulary entries (`wwwroot/data/words.json`) with embedded offline fallback.
- **Accessibility**: High contrast ratios, full keyboard navigation support, screen-reader friendly semantic labels.
