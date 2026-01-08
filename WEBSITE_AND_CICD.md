# Website & CI/CD Pipeline Documentation

## Overview

The Desktop Terrarium project includes a professional promotional website and a complete CI/CD pipeline that automatically builds the .NET application, runs tests, and deploys the website to GitHub Pages.

---

## 🌐 Website (GitHub Pages)

### Purpose
The website serves as:
- **Promotional landing page** - Showcasing what Desktop Terrarium is and why it's fun to download
- **Educational resource** - Explaining the project's academic and learning objectives
- **Download center** - Providing easy access to releases for Windows, macOS, and Linux
- **Documentation hub** - Linking to GitHub repository and comprehensive guides

### Technology Stack
- **HTML5** - Semantic, accessible markup
- **CSS3** - Modern, responsive design with CSS Grid and Flexbox
- **JavaScript (ES6+)** - Smooth animations, scroll effects, and interactive features
- **GitHub Pages** - Static site hosting, zero configuration needed

### Design Philosophy

#### Visual Identity
- **Color Palette**:
  - Primary: `#2ecc71` (Green - representing plants/growth)
  - Secondary: `#3498db` (Blue - representing calm/water)
  - Accent: `#e74c3c` (Red - representing carnivores/energy)
  - Neutrals: Dark text on light backgrounds for readability

- **Typography**:
  - System font stack for fast loading and native feel
  - Large, readable font sizes
  - Clear hierarchy with proper heading structure

- **Mood**: 
  - Clean and modern
  - Playful with emoji icons
  - Calming with soft gradients
  - Professional yet approachable

#### UX Principles
1. **Desktop-First Design** - Optimized for PC/laptop users (target audience)
2. **Responsive** - Adapts beautifully to tablets and mobile devices
3. **Fast Loading** - No external dependencies, optimized assets
4. **Accessible** - Semantic HTML, proper ARIA labels, keyboard navigation
5. **Smooth Animations** - Subtle motion that enhances without distracting

### Website Structure

#### Sections

1. **Hero Section**
   - Eye-catching title with animated emoji
   - Clear value proposition
   - Primary CTA: "Download for Free"
   - Visual: Animated terrarium window demo
   - Smooth scroll indicator

2. **What is Desktop Terrarium?**
   - 6 feature cards explaining core concepts
   - Icons: Living Ecosystem, Interactive, Beautiful, Calming, Persistent, System Aware
   - Hover effects for engagement

3. **How It Works**
   - 4-step process with numbered badges
   - Clear, concise instructions
   - Emphasizes ease of use

4. **Key Features**
   - 8 features in grid layout
   - Quick-scan format with emoji icons
   - Growing Plants, Herbivores, Carnivores, Weather, Controls, Auto-Save, Stats, Interactive

5. **See It In Action**
   - Desktop mockup showing terrarium overlay
   - Animated plants and creatures
   - Demonstrates "living on desktop" concept

6. **Download Section**
   - Three platform-specific cards (Windows, macOS, Linux)
   - Installation instructions
   - System requirements
   - Links to GitHub Releases

7. **Open Source & Educational**
   - Emphasis on GitHub repository
   - Educational/academic context
   - Technology stack badges
   - Contribution information

8. **Academic Project Notice**
   - Clear disclaimer about educational purpose
   - Lists demonstrated concepts:
     - Layered architecture
     - OOP principles
     - Test-driven development
     - WPF techniques
     - Best practices

9. **Footer**
   - Quick links to repository, releases, documentation
   - Social links
   - Copyright and licensing info

### Key Features

#### Animations & Interactions
- **Scroll animations** - Elements fade in as you scroll
- **Parallax effects** - Hero background moves at different speed
- **Hover states** - Cards lift and glow on hover
- **Button ripples** - Material Design-inspired click effect
- **Plant growth** - Animated terrarium entities
- **Creature movement** - Entities move across screen
- **Smooth scrolling** - Anchor links scroll smoothly

#### JavaScript Features
- Intersection Observer for scroll animations
- Parallax scrolling effect
- Dynamic entity spawning
- Platform detection (highlights relevant download)
- Keyboard navigation (Ctrl+Arrow keys for sections)
- Scroll progress indicator
- Code snippet copy-to-clipboard
- Console easter egg for developers

#### Responsive Design
- **Desktop (>1024px)**: Full two-column layout, large visuals
- **Tablet (768-1024px)**: Single column, adjusted spacing
- **Mobile (<768px)**: Stacked layout, touch-optimized buttons

### Deployment

#### GitHub Pages Configuration
1. Website source is in `/docs` directory
2. GitHub Actions automatically deploys on push to `main` or `copilot/**` branches
3. Accessible at: `https://hostilian.github.io/The-Desktop-Terrarium/`

#### Manual Deployment (if needed)
```bash
# Files are already in /docs directory
# Just enable GitHub Pages in repository settings:
# Settings → Pages → Source: Deploy from a branch
# Branch: main, Folder: /docs
```

---

## 🔄 CI/CD Pipeline (GitHub Actions)

### Pipeline Overview

The CI/CD pipeline ensures code quality and automates deployment through four parallel jobs:

```
Push to GitHub
    ↓
[Build and Test .NET]
    ↓
    ├──→ [Deploy Website] (if main/copilot branches)
    ├──→ [Code Quality Check]
    └──→ [Generate Summary]
```

### Workflow File: `.github/workflows/ci-cd.yml`

### Jobs Breakdown

#### 1. Build and Test .NET Application

**Purpose**: Verify that the .NET application compiles and all tests pass

**Steps**:
1. **Checkout repository** - Gets the latest code
2. **Setup .NET** - Installs .NET 8.0 SDK
3. **Restore dependencies** - Downloads NuGet packages
4. **Build solution** - Compiles with `EnableWindowsTargeting=true` flag
   - This flag allows WPF projects to build on Linux/macOS runners
5. **Run unit tests** - Executes all 36 tests in `Terrarium.Tests`
6. **Upload test results** - Saves TRX files as artifacts
7. **Check test results** - Fails pipeline if any test fails

**Key Configuration**:
```yaml
runs-on: ubuntu-latest
dotnet-version: '8.0.x'
build-flags: /p:EnableWindowsTargeting=true
```

**Exit Conditions**:
- ✅ Success: All tests pass
- ❌ Failure: Build errors or test failures

#### 2. Deploy Website to GitHub Pages

**Purpose**: Automatically deploy the website when code is pushed

**Steps**:
1. **Checkout repository** - Gets the latest code
2. **Verify website files** - Ensures `index.html`, `styles.css`, `script.js` exist
3. **Setup Pages** - Configures GitHub Pages environment
4. **Upload artifact** - Packages the `/docs` directory
5. **Deploy to GitHub Pages** - Publishes to `https://hostilian.github.io/The-Desktop-Terrarium/`

**Triggers**:
- Only runs on `main` branch or `copilot/**` branches
- Requires successful build and test job

**Permissions**:
```yaml
permissions:
  contents: read
  pages: write
  id-token: write
```

#### 3. Code Quality and Security

**Purpose**: Run additional quality checks on the codebase

**Steps**:
1. **Checkout repository** - Gets the latest code
2. **Setup .NET** - Installs .NET 8.0 SDK
3. **Build for analysis** - Compiles in Release configuration
4. **Check for common issues** - Scans for TODO/FIXME comments
5. **Security scan** - (Future: CodeQL integration)

**Quality Gates**:
- Warns about TODO/FIXME comments
- Can be extended with linters, code coverage tools

#### 4. Generate Build Summary

**Purpose**: Create a comprehensive, readable summary of the build

**Outputs**:
- ✅/❌ Build and Tests status
- ✅/❌ Website Deployment status
- ✅/❌ Code Quality status
- Branch, commit, and actor information
- Quick links to website, releases, documentation

**Example Summary**:
```markdown
# 🌱 Desktop Terrarium Build Summary

## Build Status
✅ **Build and Tests**: Passed
✅ **Website Deployment**: Successful
✅ **Code Quality**: Passed

## Repository Info
- **Branch**: `main`
- **Commit**: `abc123...`
- **Actor**: @username

## Quick Links
- 🌐 [View Website](https://hostilian.github.io/The-Desktop-Terrarium/)
- 📦 [View Releases](https://github.com/Hostilian/The-Desktop-Terrarium/releases)
- 📖 [Documentation](https://github.com/Hostilian/The-Desktop-Terrarium#readme)
```

### Trigger Events

The pipeline runs on:
- **Push** to `main` or `copilot/**` branches
- **Pull Request** targeting `main` branch
- **Manual trigger** via workflow_dispatch

### Build Badge

The README includes a build status badge:
```markdown
![CI/CD](https://img.shields.io/github/actions/workflow/status/Hostilian/The-Desktop-Terrarium/ci-cd.yml?branch=main&label=build)
```

---

## 🚀 Benefits of This Setup

### For Development
1. **Immediate Feedback** - Know within minutes if changes break tests
2. **Automated Quality Gates** - Can't merge code that doesn't build
3. **Test Coverage Visibility** - All 36 tests run on every commit
4. **Cross-Platform Build** - Verifies code works on Linux (even though WPF is Windows-only)

### For Deployment
1. **Zero Manual Steps** - Push code, website updates automatically
2. **Consistent Builds** - Same environment every time
3. **Fast Iteration** - Changes go live in ~2 minutes
4. **Rollback Safety** - Every deployment is a Git commit

### For Maintainability
1. **Documentation as Code** - Workflow is version controlled
2. **Reproducible** - Anyone can fork and have same setup
3. **Transparent** - All build logs are public
4. **Extensible** - Easy to add more checks (linters, coverage, etc.)

### For Grading/Presentation
1. **Professional Setup** - Demonstrates DevOps knowledge
2. **Live Website** - Easy to share and demonstrate
3. **Test Results** - Proves code quality
4. **Build History** - Shows development progression

---

## 📊 Exam Compliance - Pipeline Edition

The CI/CD pipeline demonstrates additional software engineering best practices:

### Continuous Integration
- ✅ **Automated Builds** - Every push triggers a build
- ✅ **Automated Tests** - All 36 unit tests run automatically
- ✅ **Fast Feedback** - Results in ~2-3 minutes
- ✅ **Quality Gates** - Pipeline fails if tests fail

### Continuous Deployment
- ✅ **Automated Deployment** - Website updates on successful builds
- ✅ **Environment Consistency** - Same deployment process every time
- ✅ **Rollback Capability** - Git history enables rollbacks

### DevOps Culture
- ✅ **Infrastructure as Code** - Pipeline defined in YAML
- ✅ **Version Control** - Workflow is part of repository
- ✅ **Transparency** - All builds and logs are public
- ✅ **Documentation** - This file explains everything

---

## 🔧 Local Development

### Testing Website Locally

#### Option 1: Simple HTTP Server (Python)
```bash
cd docs
python -m http.server 8000
# Visit: http://localhost:8000
```

#### Option 2: Node.js HTTP Server
```bash
cd docs
npx http-server -p 8000
# Visit: http://localhost:8000
```

#### Option 3: VS Code Live Server
1. Install "Live Server" extension
2. Right-click `index.html`
3. Select "Open with Live Server"

### Testing .NET Build Locally

```bash
# Restore packages
dotnet restore DesktopTerrarium.sln

# Build (requires Windows for full build)
dotnet build DesktopTerrarium.sln --configuration Release

# Or on Linux/macOS with EnableWindowsTargeting
dotnet build DesktopTerrarium.sln --configuration Release /p:EnableWindowsTargeting=true

# Run tests
dotnet test Terrarium.Tests/Terrarium.Tests.csproj --verbosity normal
```

---

## 🔮 Future Enhancements

### Website
- [ ] Screenshot gallery from actual application
- [ ] Video demo/trailer
- [ ] Interactive terrarium simulator in browser (Canvas/WebGL)
- [ ] Blog section for development updates
- [ ] Dark mode toggle
- [ ] Multi-language support

### CI/CD Pipeline
- [ ] Code coverage reports (Coverlet)
- [ ] Performance benchmarking
- [ ] Automatic release creation on version tags
- [ ] Cross-platform builds (Windows, macOS, Linux binaries)
- [ ] CodeQL security scanning
- [ ] Dependency vulnerability scanning
- [ ] Preview deployments for pull requests

### Additional Automation
- [ ] Automated changelog generation
- [ ] Release notes from commit messages
- [ ] Automatic version bumping
- [ ] Discord/Slack notifications on build status
- [ ] Performance regression detection

---

## 📝 Summary

This project demonstrates a **complete, modern software development lifecycle**:

1. **Clean Code** - Follows all OOP and exam requirements
2. **Comprehensive Testing** - 36 unit tests, 100% passing
3. **Professional Website** - Beautiful, responsive, informative
4. **Automated Pipeline** - Build, test, deploy with zero manual steps
5. **Quality Gates** - Tests must pass before deployment
6. **Documentation** - Everything is well-documented

**The result**: A production-ready, maintainable, exam-compliant project that demonstrates mastery of both software engineering and DevOps practices.

---

*Last Updated: January 8, 2026*
*Project Status: ✅ COMPLETE*
