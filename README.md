# 🌿 Desktop Terrarium

> Interactive ecosystem simulations and games for your desktop

[![GitHub Pages](https://img.shields.io/badge/Play-Online-brightgreen)](https://hostilian.github.io/The-Desktop-Terrarium/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

## 🎮 Play Online,
but so far website is not finished. For the experience pls Clone/build/run locally

**[→ Play in your browser](https://hostilian.github.io/The-Desktop-Terrarium/)**

Experience Snake, 2048, and T-Rex Runner with stunning animations and fullscreen support!

## 🖥️ Desktop Application

### Quick Start

```bash
# Clone the repository
git clone https://github.com/Hostilian/The-Desktop-Terrarium.git

# Build and run
.\clean_build.bat
.\publish\Terrarium.Desktop.exe
```

### Features

- **Live Sandbox** - Particle physics simulation
- **Civilization Builder** - 4X strategy game
- **Classic Games** - 2048, Tetris, Snake, T-Rex Runner, Pacman
- **Beautiful UI** - Modern WPF interface with animations

## 📁 Project Structure

```
The-Desktop-Terrarium/
├── src/                    # Source code (C# projects)
│   ├── Terrarium.Desktop/  # Main WPF application
│   ├── Terrarium.Logic/    # Game logic and simulations
│   └── Terrarium.Tests/    # Unit tests
├── widgets/                # Python-based game widgets
├── docs/                   # Web game platform (HTML5/JS)
├── scripts/                # Build and utility scripts
└── .github/                # CI/CD workflows
```

## 🛠️ Development

**Requirements:**
- .NET 8.0 SDK
- Python 3.11+ (for widgets)

**Build:**
```bash
.\clean_build.bat
```

**Test:**
```bash
dotnet test Terrarium.Tests/
```

## 📝 License

MIT License - see [LICENSE](LICENSE) for details

## 🎯 Contributing

Contributions welcome! Please feel free to submit a Pull Request.
