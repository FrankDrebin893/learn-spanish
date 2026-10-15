# DESIGN.md - Design System: Tierra y Sol

## 1. Visual Philosophy: *Tierra y Sol* (Earth and Sun)

The Spanish Word Learning App embodies an Iberian aesthetic inspired by sun-drenched terracotta tiles, warm Mediterranean sand, Seville saffron, and deep Andalusian indigo. It moves away from cold, generic tech templates toward an inviting, tactile learning atmosphere.

### Core Tenets
- **Warmth Over Clinical Neutrality**: Soft parchment canvas instead of stark sterile white or cold blue-gray.
- **Clear Information Hierarchy**: Bold, expressive typography for Spanish vocabulary terms, paired with crisp, legible metadata.
- **Tactile Feedback**: Distinct interactive cards that invite touch and keyboard interaction, with smooth state transitions.
- **Intentional Contrast**: Vibrant accent colors for correct answers and mastery indicators, balanced with warm neutral foundations.

---

## 2. Color Palette & Semantic Tokens

| Token | Hex Value | Semantic Usage |
|---|---|---|
| **Primary (Terracotta)** | `#D9483B` | Brand identity, primary actions, active indicators |
| **Primary Dark** | `#BF3A2E` | Hover/active states on primary buttons |
| **Secondary (Saffron / Amber)** | `#E0872E` | Badges, streaks, highlight chips, focus states |
| **Accent (Spanish Indigo)** | `#1E293B` | High-contrast headers, deep text elements |
| **Success (Olive Green)** | `#16A34A` | Correct quiz answers, "Learned" tags, high accuracy |
| **Error (Crimson)** | `#DC2626` | Incorrect answers, struggling word flags, reset triggers |
| **Warning (Ochre)** | `#D97706` | Cautionary indicators, partial mastery |
| **Background (Parchment Canvas)** | `#FAF8F5` | Main page canvas background |
| **Surface (Clean White)** | `#FFFFFF` | Cards, popovers, active dialog surfaces |
| **Border (Warm Sand)** | `#EBE6DE` | Card borders, dividers, outlines |
| **Text Primary (Deep Slate)** | `#1C1917` | Headings, primary vocabulary words |
| **Text Secondary (Warm Charcoal)** | `#57534E` | Translations, explanations, captions, icons |

---

## 3. Typography Hierarchy

- **Font Family**:
  - Primary UI & Vocabulary: `'Outfit', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`
  - Fallback: Crisp sans-serif with high x-height and clear Spanish diacritic accents (`á`, `é`, `í`, `ó`, `ú`, `ñ`, `ü`, `¿`, `¡`).
- **Scale**:
  - `Display / Word Card`: 2.5rem (40px) to 3.25rem (52px), weight 700.
  - `H1 / Page Titles`: 2rem (32px), weight 700, tracking -0.02em.
  - `H2 / Section Headers`: 1.5rem (24px), weight 600.
  - `H3 / Card Titles`: 1.25rem (20px), weight 600.
  - `Body 1`: 1rem (16px), line-height 1.6, weight 400.
  - `Body 2 / Metadata`: 0.875rem (14px), line-height 1.5, weight 500.
  - `Caption`: 0.75rem (12px), letter-spacing 0.04em, weight 600, uppercase for badges.

---

## 4. Components & Elevation

- **Cards & Papers**:
  - Border-radius: `16px` for main cards, `12px` for inner items/chips.
  - Border: `1px solid #EBE6DE`.
  - Shadows: `0 4px 20px -2px rgba(60, 40, 20, 0.05), 0 2px 6px -1px rgba(0, 0, 0, 0.03)`.
- **Buttons**:
  - Pill and soft-rounded (`12px`) geometry.
  - Smooth transform hover (`translateY(-1px)`) and active tap feel.
- **Audio Button**:
  - Dedicated speaker action with subtle ripple/glow effect indicating voice playback.

---

## 5. Micro-Interactions & Transitions

- **Card Reveal**: Smooth 0.25s opacity and scale transition when revealing translation.
- **Quiz Feedback**: Immediate color change (green for correct, soft red for incorrect) with keyboard feedback.
- **Progress Bars**: Smooth ease-out fill animation.
