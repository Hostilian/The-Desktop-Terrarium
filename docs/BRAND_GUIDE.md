# Desktop Terrarium Brand Guide

A comprehensive visual identity and UX guide for the Desktop Terrarium project.

---

## 🌱 Brand Overview

### What is Desktop Terrarium?

**Desktop Terrarium** is a calming, interactive desktop companion that brings a tiny living ecosystem to your computer screen. It's like having a miniature nature reserve right at the bottom of your desktop — plants grow, cute creatures wander, and life unfolds while you work.

Think of it as a digital pet meets screensaver meets zen garden. It's designed to be:
- **Fun to download** — A delightful discovery that makes you smile
- **Relaxing to watch** — Gentle animations that calm rather than distract
- **Rewarding to nurture** — Your ecosystem grows and thrives with your attention

### Why Desktop Terrarium?

In a world of notifications, deadlines, and screen fatigue, Desktop Terrarium offers a gentle counterbalance:

| Pain Point | Desktop Terrarium Solution |
|------------|---------------------------|
| Sterile desktop environment | Living, breathing ecosystem |
| Constant work stress | Moments of calm observation |
| Digital disconnection from nature | Miniature natural world |
| Boring desktop background | Interactive, evolving life |

### Target Audience

- **Office workers** seeking stress relief during work
- **Students** wanting a calming study companion
- **Developers** who appreciate clean code and fun projects
- **Nature lovers** who want greenery on their screens
- **Casual gamers** who enjoy idle/passive games

---

## 🎨 Visual Identity

### Color Palette

| Name | Hex Code | RGB | Usage |
|------|----------|-----|-------|
| **Primary Green** | `#2ECC71` | 46, 204, 113 | Buttons, highlights, plants, growth |
| **Primary Dark** | `#27AE60` | 39, 174, 96 | Hover states, plant shadows |
| **Secondary Blue** | `#3498DB` | 52, 152, 219 | Links, water, secondary actions |
| **Accent Orange** | `#E67E22` | 230, 126, 34 | Warnings, fire, energy |
| **Background Dark** | `#0F0F0F` | 15, 15, 15 | Main background |
| **Surface** | `#1A1A1A` | 26, 26, 26 | Cards, panels |
| **Surface Light** | `#252525` | 37, 37, 37 | Hover states on surfaces |
| **Text Primary** | `#E0E0E0` | 224, 224, 224 | Main text |
| **Text Muted** | `#888888` | 136, 136, 136 | Secondary text, captions |

### CSS Variables

```css
:root {
    --primary: #2ECC71;
    --primary-dark: #27AE60;
    --secondary: #3498DB;
    --accent: #E67E22;
    --background: #0F0F0F;
    --surface: #1A1A1A;
    --surface-light: #252525;
    --text: #E0E0E0;
    --text-muted: #888888;
    --gradient-start: #2ECC71;
    --gradient-end: #3498DB;
}
```

### Color Psychology

- **Green (#2ECC71)** — Growth, nature, life, calm, harmony
- **Blue (#3498DB)** — Trust, water, tranquility, depth
- **Dark backgrounds** — Focus, immersion, reduced eye strain
- **Orange accents** — Energy, warmth, alerts (used sparingly)

---

## 🔤 Typography

### Font Stack

```css
font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
```

### Type Scale

| Level | Size | Weight | Line Height | Usage |
|-------|------|--------|-------------|-------|
| H1 | clamp(2.5rem, 8vw, 4.5rem) | 700 | 1.1 | Hero headlines |
| H2 | 2.5rem (40px) | 700 | 1.2 | Section titles |
| H3 | 1.25rem (20px) | 600 | 1.3 | Feature cards, subsections |
| Body | 1rem (16px) | 400 | 1.6 | Paragraph text |
| Small | 0.9rem (14.4px) | 400 | 1.5 | Captions, metadata |
| Mono | 0.85rem | 500 | 1.4 | Code blocks (JetBrains Mono) |

### Code Font

```css
font-family: 'JetBrains Mono', 'Fira Code', 'Consolas', monospace;
```

---

## 💫 Mood & Tone

### Brand Personality

| Attribute | Description |
|-----------|-------------|
| **Calming** | Soft animations, muted colors, gentle motion |
| **Playful** | Cute creatures, bouncy animations, emoji support |
| **Modern** | Clean design, glassmorphism, smooth gradients |
| **Approachable** | Friendly copy, helpful tooltips, forgiving UX |
| **Authentic** | Open-source, transparent about limitations |

### Voice & Tone Guidelines

✅ **Do:**
- Use friendly, conversational language
- Include emoji for personality 🌱🐰🐺
- Explain features in simple terms
- Celebrate small wins ("Your first plant is growing!")

❌ **Don't:**
- Use technical jargon without explanation
- Be overly formal or corporate
- Create urgency or FOMO
- Overwhelm with information

### Example Copy

| Context | Bad | Good |
|---------|-----|------|
| Feature description | "Implements 60Hz render loop" | "Smooth animations at 60 FPS" |
| Error message | "NullReferenceException" | "Oops! Something went wrong. Try restarting." |
| Achievement | "ACHIEVEMENT_001_UNLOCKED" | "🏆 Green Thumb — Your first plant reached max size!" |

---

## 🖥️ Desktop Behavior

### Window Characteristics

| Property | Value | Rationale |
|----------|-------|-----------|
| **Position** | Bottom of screen | Non-intrusive, always visible |
| **Transparency** | Transparent background | Blends with desktop |
| **Always on top** | Yes (optional) | Stays visible while working |
| **Resizable** | No | Consistent experience |
| **Window chrome** | None (borderless) | Seamless integration |
| **Taskbar presence** | Minimal | Doesn't clutter taskbar |

### Interaction Model

```
┌─────────────────────────────────────────────────────────────┐
│                    YOUR DESKTOP AREA                         │
│                                                              │
│                   [Applications, Icons]                      │
│                                                              │
│                                                              │
├──────────────────────────────────────────────────────────────┤
│ 🌿    🐰         🌱    🐺      🌿          🐰    🌱   │  ← Terrarium
│    Ground/Grass                                               │
└──────────────────────────────────────────────────────────────┘
```

### Mouse Interaction

| Action | Entity | Result |
|--------|--------|--------|
| **Hover** | Plant | Plant shakes gently |
| **Hover** | Creature | Tooltip with stats |
| **Click** | Plant | Waters the plant |
| **Click** | Creature | Feeds the creature |
| **Click** | Empty space | Passes through to desktop |

### Hit-Test Passthrough

The window uses Win32 hit-testing to allow clicks on empty areas to pass through to the desktop. Only entities and UI elements capture mouse input.

---

## 🎭 Animation Guidelines

### Motion Principles

1. **Ease-in-out** — Natural, organic motion
2. **Subtle amplitude** — Small movements, not jarring
3. **Varied timing** — Staggered animations prevent robotic feel
4. **Purpose-driven** — Every animation communicates something

### Animation Timing

| Animation | Duration | Easing | Purpose |
|-----------|----------|--------|---------|
| Plant shake | 200ms | ease-out | Feedback on hover |
| Creature walk | Continuous | linear | Normal movement |
| Birth particle | 500ms | ease-out | Celebrate new life |
| Death fade | 300ms | ease-in | Gentle departure |
| UI slide-in | 300ms | ease-out | Panel appearance |
| Tooltip fade | 150ms | ease-in-out | Quick context |

### Particle Effects

- **Birth**: Green sparkles rising upward ✨
- **Death**: Gray particles falling down 💨
- **Eating**: Small food particles absorbed 🍃
- **Watering**: Blue droplets descending 💧

---

## 📐 UX Principles

### Core UX Tenets

1. **Passive-first** — Works beautifully without interaction
2. **Discoverable** — Features reveal themselves naturally
3. **Forgiving** — No permanent mistakes, everything recoverable
4. **Informative** — Status always clear without being noisy
5. **Respectful** — Never interrupts or demands attention

### Information Hierarchy

```
┌─ Primary (Always visible) ─────────────────────────┐
│  • Ecosystem entities (plants, creatures)           │
│  • Day/night visual state                          │
│  • Weather effects (if active)                     │
└────────────────────────────────────────────────────┘

┌─ Secondary (On hover/demand) ──────────────────────┐
│  • Entity tooltips                                 │
│  • Status bar (F1)                                 │
│  • Mini-map (M)                                    │
│  • Population graph (G)                            │
└────────────────────────────────────────────────────┘

┌─ Tertiary (Settings/Advanced) ─────────────────────┐
│  • Settings panel (F2)                             │
│  • Achievement notifications                        │
│  • Predator warnings                               │
└────────────────────────────────────────────────────┘
```

### Keyboard Shortcuts Philosophy

- **Single keys** for frequent actions (P, H, C, M, G)
- **F-keys** for UI toggles (F1, F2)
- **Ctrl+key** for system actions (Save, Load)
- **Global hotkeys** for background control (Ctrl+Alt+S/L)

---

## 🌿 Entity Visual Design

### Plants

| Visual Element | Design |
|----------------|--------|
| Shape | Circle or custom sprite |
| Color | Green gradient (#27AE60 → #2ECC71) |
| Size | Grows from 5px to 30px radius |
| Health indicator | Opacity reduces as health decreases |
| Water indicator | Blue ring intensity shows hydration |

### Herbivores

| Visual Element | Design |
|----------------|--------|
| Shape | Small oval or sprite (🐰🐑) |
| Color | Soft earth tones (tan, white, brown) |
| Movement | Gentle, wandering pattern |
| Eating animation | Bobs toward plants |
| Flee animation | Quick dash away from carnivores |

### Carnivores

| Visual Element | Design |
|----------------|--------|
| Shape | Larger oval or sprite (🐺🦊) |
| Color | Gray, orange, darker tones |
| Movement | Purposeful, tracking pattern |
| Hunting animation | Accelerates toward prey |
| Idle animation | Prowling, scanning |

---

## 📊 UI Components

### Toast Notifications

```
┌──────────────────────────────────┐
│ 🌱 New plant sprouted!           │
│    Your ecosystem is growing     │
└──────────────────────────────────┘
         ↑
    Appears top-right
    Auto-dismisses after 3s
    Max 3 visible at once
```

### Tooltips

```
┌─────────────────────────────────┐
│ 🐰 Sheep                        │
│ ────────────────────            │
│ Health: ████████░░ 80%          │
│ Hunger: ██░░░░░░░░ 20%          │
│ Age: 45 seconds                 │
│ ────────────────────            │
│ Click to feed                   │
└─────────────────────────────────┘
```

### Settings Panel

- Accessible via F2
- Slide-in from right edge
- Dark glassmorphism background
- Organized in collapsible sections
- Changes apply immediately

---

## 📝 Emotional Goals

### Relaxation

> "I feel calmer after watching my terrarium for a few minutes."

**Design supports this through:**
- Slow, organic animations
- Natural color palette
- No urgent notifications
- Passive observation mode

### Curiosity

> "I wonder what happens if I let the herbivores multiply?"

**Design supports this through:**
- Discoverable features (hidden keyboard shortcuts)
- Emergent ecosystem behavior
- Statistics and graphs showing trends
- Achievement system rewarding exploration

### Ownership

> "This is MY terrarium. I've been growing it for weeks."

**Design supports this through:**
- Persistent save/load (survives restarts)
- Cumulative statistics (total births, deaths)
- Achievement progress
- Customizable spawn rates

---

## 🎓 Academic Context

Desktop Terrarium was developed as an educational project demonstrating:

- **Layered Architecture** — Separation of concerns
- **Object-Oriented Programming** — Inheritance, interfaces, encapsulation
- **Unit Testing** — 119+ tests with 81% coverage
- **CI/CD** — Automated builds, tests, and deployment
- **Clean Code** — Following .NET naming conventions

This context shapes the brand as:
- **Transparent** about its educational origins
- **Open-source** to encourage learning
- **Well-documented** for academic review

---

## ✅ Brand Checklist

Use this checklist when creating new features or content:

- [ ] Does the color palette match the defined variables?
- [ ] Is the typography consistent with the type scale?
- [ ] Does the animation feel smooth and purposeful?
- [ ] Is the copy friendly and jargon-free?
- [ ] Does it support passive observation?
- [ ] Is the feature discoverable without being intrusive?
- [ ] Does it contribute to relaxation, curiosity, or ownership?

---

*Last updated: January 2026*
