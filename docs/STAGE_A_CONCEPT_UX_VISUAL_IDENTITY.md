# STAGE A — CONCEPT, UX & VISUAL IDENTITY
## Desktop Terrarium — Complete Design Specification

---

## 🌱 WHAT IS DESKTOP TERRARIUM?

### Simple, Friendly Definition

**Desktop Terrarium** is a small, living digital ecosystem that sits at the bottom of your computer screen. Imagine a tiny glass terrarium filled with plants and creatures, but instead of sitting on your desk, it lives right on your desktop background. Plants grow slowly, herbivores graze peacefully, and carnivores hunt — all while you work, study, or browse the web.

It's like having a **digital pet**, a **screensaver**, and a **zen garden** all in one. You can watch it passively, or interact with it by clicking to feed creatures and water plants. The ecosystem runs on its own, creating moments of calm and curiosity throughout your day.

### Why It's Fun and Calming

**Fun Aspects:**
- **Discovery**: Each time you look, something new might be happening — a plant grew, a creature was born, or a predator caught its prey
- **Interaction**: Clicking to feed or water creates a sense of connection and care
- **Emergent Behavior**: The food chain creates unexpected moments — watching herbivores flee from carnivores, or seeing population booms and crashes
- **Achievements**: Unlocking milestones like "First Plant" or "Ecosystem Balance" provides small rewards
- **Visual Delight**: Smooth animations, particle effects, and cute creature designs make it pleasant to watch

**Calming Aspects:**
- **Passive Observation**: No pressure to interact — it thrives on its own
- **Natural Rhythms**: Plants grow slowly, creatures move organically, day transitions to night
- **Non-Intrusive**: Sits quietly at the bottom, never demanding attention
- **Gentle Motion**: All animations use ease-in-out curves, creating smooth, organic movement
- **Color Psychology**: Green (growth, nature) and blue (water, tranquility) dominate the palette
- **No Urgency**: Unlike games with timers or notifications, there's no pressure to act

### Target Audience

**Primary Users:**
1. **Office Workers** (25-45 years old)
   - Seeking stress relief during long work sessions
   - Want a non-distracting desktop companion
   - Appreciate subtle, calming visuals

2. **Students** (18-30 years old)
   - Need a study companion that doesn't interrupt focus
   - Enjoy passive entertainment during breaks
   - Value open-source projects for learning

3. **Developers & Tech Enthusiasts** (20-50 years old)
   - Appreciate clean code and architecture
   - Interested in the technical implementation
   - Enjoy discovering hidden features and keyboard shortcuts

4. **Nature Lovers** (All ages)
   - Want greenery and life on their screens
   - Appreciate ecosystem simulation
   - Enjoy watching natural processes unfold

5. **Casual Gamers** (18-40 years old)
   - Enjoy idle/passive games
   - Like achievement systems and progression
   - Prefer low-commitment experiences

**Secondary Users:**
- **Academic Examiners**: Reviewing the project for educational purposes
- **Open-Source Contributors**: Interested in contributing or learning from the codebase

### Emotional Goals

#### 1. Relaxation 🧘

**Goal Statement**: "I feel calmer after watching my terrarium for a few minutes."

**How We Achieve This:**
- **Slow Pacing**: Logic updates every 200ms (5 times per second), not frantic
- **Organic Motion**: All movement uses ease-in-out curves, mimicking natural motion
- **Muted Colors**: Dark backgrounds (#0F0F0F) reduce eye strain, green/blue palette is soothing
- **No Alerts**: No pop-ups, notifications, or urgent messages
- **Ambient Experience**: Works as background decoration, not foreground distraction
- **Breathing Room**: Entities have space to move, no cluttered visuals

**Design Decisions:**
- **200ms Logic Tick**: Chosen because it's fast enough for smooth simulation but slow enough to feel calm (not frantic like 60Hz game logic)
- **60 FPS Rendering**: Visual smoothness is important for relaxation, so rendering is separate from logic
- **Transparent Background**: Blends with desktop, doesn't create visual barriers
- **Bottom Positioning**: Always visible but never in the way of work

#### 2. Curiosity 🔍

**Goal Statement**: "I wonder what happens if I let the herbivores multiply?"

**How We Achieve This:**
- **Emergent Behavior**: Food chain dynamics create unpredictable outcomes
- **Statistics Tracking**: Real-time graphs show population trends
- **Discoverable Features**: Hidden keyboard shortcuts (P/H/C to spawn, M for map, G for graph)
- **Achievement System**: Rewards experimentation with different playstyles
- **Day/Night Cycle**: Different behaviors at different times encourage observation
- **Tooltips**: Hover reveals detailed stats, encouraging exploration

**Design Decisions:**
- **No Tutorial**: Users discover features naturally, creating a sense of exploration
- **Statistics Panel**: Accessible via F1, shows births, deaths, peaks — data invites curiosity
- **Achievement Notifications**: Celebrate milestones, encouraging users to try new things
- **Reproduction System**: Watching creatures breed creates "what if" scenarios
- **Weather Effects**: CPU-based storms add unpredictability

#### 3. Ownership 🏠

**Goal Statement**: "This is MY terrarium. I've been growing it for weeks."

**How We Achieve This:**
- **Persistent Saves**: Ecosystem survives restarts and shutdowns
- **Cumulative Statistics**: Total births, deaths, and playtime tracked over time
- **Achievement Progress**: Long-term goals like "100 Plants Grown" create investment
- **Customization**: Adjustable spawn rates, simulation speed, visual effects
- **Personal Interaction**: Clicking to feed creates emotional connection
- **Unique Ecosystems**: Each save file develops differently based on user choices

**Design Decisions:**
- **Auto-Save on Exit**: No risk of losing progress, builds trust
- **Manual Save (Ctrl+S)**: Gives users control over when to save
- **Session Timer**: Shows how long you've been nurturing this ecosystem
- **Settings Panel (F2)**: Customization makes it feel personal
- **Save File Location**: Stored in user's AppData, making it "theirs"

---

## 🎨 VISUAL IDENTITY

### Color Palette

**Decision Rationale**: Colors were chosen to evoke nature, calm, and modern digital aesthetics.

| Color Name | Hex Code | RGB | Usage | Rationale |
|------------|----------|-----|-------|-----------|
| **Primary Green** | `#2ECC71` | 46, 204, 113 | Buttons, highlights, plants, growth indicators | Green is universally associated with nature, growth, and life. This specific shade (#2ECC71) is vibrant enough to be visible on dark backgrounds but not harsh on the eyes. It's the "life" color. |
| **Primary Dark** | `#27AE60` | 39, 174, 96 | Hover states, plant shadows, depth | Darker green provides contrast and depth. Used for shadows and hover states to create visual hierarchy without being jarring. |
| **Secondary Blue** | `#3498DB` | 52, 152, 219 | Links, water effects, secondary actions | Blue represents water (essential for plants) and tranquility. It complements green without clashing. Used for secondary actions to create a clear action hierarchy. |
| **Accent Orange** | `#E67E22` | 230, 126, 34 | Warnings, energy indicators, fire effects | Orange is used sparingly for alerts and energy. It stands out against the green/blue palette, making it perfect for important but non-urgent information. |
| **Background Dark** | `#0F0F0F` | 15, 15, 15 | Main background, website background | Near-black reduces eye strain, especially for long viewing sessions. It makes the green/blue entities pop, creating a "spotlight" effect. |
| **Surface** | `#1A1A1A` | 26, 26, 26 | Cards, panels, UI containers | Slightly lighter than background creates depth. Used for cards and panels to create visual separation without harsh contrast. |
| **Surface Light** | `#252525` | 37, 37, 37 | Hover states on surfaces | Provides subtle feedback on hover. The 10% brightness increase is noticeable but not jarring. |
| **Text Primary** | `#E0E0E0` | 224, 224, 224 | Main text, labels | High contrast (224/255) ensures readability on dark backgrounds. Not pure white (#FFFFFF) to reduce eye strain. |
| **Text Muted** | `#888888` | 136, 136, 136 | Secondary text, captions | 50% gray creates hierarchy — important text is bright, less important is muted. Still readable but doesn't compete for attention. |

**Color Psychology Applied:**
- **Green Dominance**: 60% of visual weight is green, reinforcing the "nature" theme
- **Blue Accents**: 25% blue for water and secondary actions, creating balance
- **Dark Backgrounds**: 90% dark colors reduce cognitive load and eye strain
- **Orange Sparingly**: <5% orange ensures it's attention-grabbing when used

### Typography

**Decision Rationale**: Typography must be modern, readable, and friendly.

**Primary Font: Inter**
- **Why Inter**: Designed specifically for screens, excellent readability at all sizes, friendly but professional
- **Font Stack**: `'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif`
- **Fallback Strategy**: System fonts ensure compatibility if Inter fails to load
- **Weights Used**: 300 (light), 400 (regular), 500 (medium), 600 (semibold), 700 (bold)

**Code Font: JetBrains Mono**
- **Why JetBrains Mono**: Designed for code readability, distinct character shapes prevent confusion (0 vs O, 1 vs l)
- **Font Stack**: `'JetBrains Mono', 'Fira Code', 'Consolas', monospace`
- **Usage**: Code blocks, technical documentation, statistics displays

**Type Scale Rationale**:
- **H1 (clamp(2.5rem, 8vw, 4.5rem))**: Responsive sizing ensures hero text is always readable, never too small on mobile or too large on desktop
- **H2 (2.5rem)**: Section titles need to be prominent but not overwhelming
- **H3 (1.25rem)**: Feature cards and subsections need clear hierarchy
- **Body (1rem)**: Standard 16px base ensures accessibility and readability
- **Small (0.9rem)**: Captions and metadata are smaller but still readable (14.4px)

**Line Height Rationale**:
- **Headings (1.1-1.3)**: Tighter line height for headings creates visual cohesion
- **Body (1.6)**: Generous line height (1.6) improves readability, especially for longer paragraphs
- **Code (1.4)**: Slightly tighter for code blocks to fit more content

### Mood: Playful, Clean, Modern

**Playful Elements:**
- **Emoji Usage**: 🌿🐰🐺 used in UI and documentation adds personality without being childish
- **Bouncy Animations**: Creatures hop, plants sway — movement feels alive, not robotic
- **Particle Effects**: Birth sparkles, death particles, eating animations add delight
- **Achievement Celebrations**: Toast notifications with emoji celebrate milestones
- **Cute Creature Designs**: Soft, rounded shapes (not realistic) create approachability

**Clean Elements:**
- **Minimal UI**: Only essential elements visible, everything else hidden until needed
- **Consistent Spacing**: 8px grid system ensures visual harmony
- **Clear Hierarchy**: Size, color, and position create clear information hierarchy
- **No Clutter**: Empty space is used intentionally, not filled with unnecessary elements
- **Glassmorphism**: Subtle backdrop blur on panels creates depth without heaviness

**Modern Elements:**
- **Gradient Buttons**: Linear gradients (green to blue) create depth and modern feel
- **Smooth Animations**: 60 FPS rendering, ease-in-out curves, no janky motion
- **Dark Theme**: Modern applications favor dark themes for reduced eye strain
- **Responsive Design**: Website adapts to all screen sizes gracefully
- **Transparent Overlays**: Modern desktop applications use transparency for integration

**Balance Decision**: The mood is **70% calm, 20% playful, 10% modern**. Calm dominates because relaxation is the primary goal, but playful elements prevent it from being boring, and modern touches ensure it feels current.

---

## 📐 UX PRINCIPLES

### Core Principles

#### 1. Passive-First Design

**Principle**: The application must work beautifully without any user interaction.

**Implementation**:
- Ecosystem runs automatically on startup
- No tutorial or setup required
- Entities spawn automatically based on configurable rates
- Save/load happens automatically
- All features work in the background

**Rationale**: Users should be able to install, run, and enjoy without learning anything. This reduces friction and supports the "calming" goal.

#### 2. Discoverable Features

**Principle**: Features reveal themselves naturally through exploration, not forced tutorials.

**Implementation**:
- Keyboard shortcuts documented in tooltips (hover to see "Press P to spawn plant")
- Settings panel (F2) lists all available shortcuts
- Tooltips appear on hover, teaching users what's possible
- Achievement notifications hint at features ("You discovered the mini-map!")

**Rationale**: Discovery creates curiosity and ownership. Users feel smart when they find features, not overwhelmed by tutorials.

#### 3. Forgiving UX

**Principle**: No permanent mistakes, everything is recoverable.

**Implementation**:
- Auto-save on exit prevents data loss
- Manual save (Ctrl+S) allows saving before experiments
- Load (Ctrl+L) restores previous state
- Settings can be reset to defaults
- No "game over" state — ecosystem always recovers

**Rationale**: Fear of making mistakes prevents experimentation. Forgiving UX encourages curiosity.

#### 4. Informative but Not Noisy

**Principle**: Status is always clear, but information doesn't overwhelm.

**Implementation**:
- Primary information (entities) always visible
- Secondary information (stats) available on demand (F1)
- Tertiary information (settings) hidden until needed (F2)
- Toast notifications auto-dismiss after 3 seconds
- Max 3 notifications visible at once

**Rationale**: Users need information to make decisions, but too much information creates cognitive load and breaks the "calming" goal.

#### 5. Respectful of User Workflow

**Principle**: Never interrupts or demands attention.

**Implementation**:
- No pop-ups or modal dialogs
- Notifications are non-blocking toasts
- Window sits at bottom, never covers important content
- Hit-test passthrough allows clicking through empty areas
- Global hotkeys work even when window isn't focused

**Rationale**: Desktop Terrarium is a companion, not a distraction. It should enhance the desktop experience, not compete with work.

### Information Hierarchy

**Level 1: Always Visible (Primary)**
- Ecosystem entities (plants, creatures)
- Ground/grass visual
- Day/night visual state (brightness/color)
- Weather effects (if active)

**Rationale**: These are the core experience. Users should always see the "life" happening.

**Level 2: On Demand (Secondary)**
- Entity tooltips (hover)
- Status bar (F1 toggle)
- Mini-map (M key)
- Population graph (G key)

**Rationale**: These provide context and depth but aren't essential. Available when curiosity strikes.

**Level 3: Settings/Advanced (Tertiary)**
- Settings panel (F2)
- Achievement notifications (auto-dismiss)
- Predator warnings (subtle, non-intrusive)

**Rationale**: These are for customization and advanced features. Hidden by default to maintain simplicity.

### Desktop Behavior

**Window Position: Bottom of Screen**
- **Rationale**: Non-intrusive, always visible but never in the way
- **Implementation**: Calculates screen height, positions window at bottom with 0px top offset

**Transparency: Transparent Background**
- **Rationale**: Blends with desktop, creates seamless integration
- **Implementation**: WPF Window with `Background="Transparent"` and `AllowsTransparency="True"`

**Always on Top: Optional (Default: Yes)**
- **Rationale**: Stays visible while working, but users can disable if needed
- **Implementation**: `Topmost="True"` property, toggleable in settings

**Resizable: No**
- **Rationale**: Consistent experience, prevents UI breaking at different sizes
- **Implementation**: `ResizeMode="NoResize"` in WPF

**Window Chrome: None (Borderless)**
- **Rationale**: Seamless integration, no visual barriers
- **Implementation**: `WindowStyle="None"` in WPF

**Taskbar Presence: Minimal**
- **Rationale**: Doesn't clutter taskbar, but accessible if needed
- **Implementation**: Window can be minimized, shows in taskbar but doesn't demand attention

**Hit-Test Passthrough: Enabled for Empty Areas**
- **Rationale**: Users can click through to desktop, only entities capture input
- **Implementation**: Win32 `WM_NCHITTEST` message handling, returns `HTTRANSPARENT` for empty areas

### Light Interactivity

**Mouse Hover:**
- **Plants**: Shake gently (200ms ease-out animation)
- **Creatures**: Show tooltip with stats (health, hunger, age)
- **Empty Space**: No effect (passes through to desktop)

**Mouse Click:**
- **Plants**: Water the plant (increases water level, shows particle effect)
- **Creatures**: Feed the creature (increases hunger satisfaction, shows particle effect)
- **Empty Space**: Passes through to desktop (no action)

**Keyboard Shortcuts:**
- **P**: Spawn plant
- **H**: Spawn herbivore
- **C**: Spawn carnivore
- **W**: Water all plants
- **M**: Toggle mini-map
- **G**: Toggle population graph
- **F1**: Toggle status bar
- **F2**: Toggle settings panel
- **Ctrl+S**: Manual save
- **Ctrl+L**: Manual load

**Rationale**: Light interactivity creates connection without demanding constant attention. Shortcuts are discoverable but not required.

---

## 🎯 DECISION SUMMARY

### Why These Choices?

**Color Palette**: Green/blue/dark creates a calming, nature-focused aesthetic that reduces eye strain and supports relaxation.

**Typography**: Inter is modern and readable, JetBrains Mono is perfect for code. Responsive sizing ensures accessibility.

**Mood Balance**: 70% calm, 20% playful, 10% modern ensures the primary goal (relaxation) is met while remaining engaging.

**Passive-First**: Reduces friction, supports the "calming" goal, and makes the application accessible to all users.

**Bottom Positioning**: Non-intrusive, always visible, never in the way of work.

**Transparency**: Creates seamless desktop integration, making it feel like a natural part of the desktop.

**Light Interactivity**: Creates connection and ownership without demanding constant attention.

**Information Hierarchy**: Prevents cognitive overload while ensuring users have access to information when needed.

---

## ✅ STAGE A COMPLETE

All concepts, UX principles, and visual identity decisions have been fully specified with complete rationale for every choice. The design supports the three emotional goals (relaxation, curiosity, ownership) through deliberate decisions about color, typography, behavior, and interaction.

**Next Stage**: Stage B — Website Design & Implementation

