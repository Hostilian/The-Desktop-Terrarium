# Contributing to Desktop Terrarium

Thank you for your interest in contributing to Desktop Terrarium! This document provides guidelines and instructions for contributing.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [How to Contribute](#how-to-contribute)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing Guidelines](#testing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Community](#community)

---

## 📜 Code of Conduct

This project adheres to a Code of Conduct that all contributors are expected to follow. Please read [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing.

**In short:** Be respectful, inclusive, and professional.

---

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Git
- GitHub account

### Fork & Clone
```bash
# Fork the repository on GitHub

# Clone your fork
git clone https://github.com/YOUR_USERNAME/The-Desktop-Terrarium.git
cd The-Desktop-Terrarium

# Add upstream remote
git remote add upstream https://github.com/Hostilian/The-Desktop-Terrarium.git
```

### Build & Run
```bash
# Restore dependencies
dotnet restore src/DesktopTerrarium.sln

# Build
dotnet build src/DesktopTerrarium.sln

# Run tests
dotnet test src/DesktopTerrarium.sln

# Run application
cd src/Terrarium.Desktop
dotnet run
```

---

## 🤝 How to Contribute

### Types of Contributions

We welcome:
- 🐛 **Bug fixes**
- ✨ **New features**
- 📝 **Documentation improvements**
- 🎨 **UI/UX enhancements**
- ⚡ **Performance optimizations**
- 🧪 **Additional tests**
- 🌍 **Translations** (future)

### Before You Start

1. **Check existing issues** - Someone may already be working on it
2. **Open an issue** - Discuss major changes before implementing
3. **Keep PRs focused** - One feature/fix per PR
4. **Follow conventions** - Match existing code style

---

## 🔄 Development Workflow

### 1. Create Feature Branch
```bash
# Update your fork
git fetch upstream
git checkout main
git merge upstream/main

# Create feature branch
git checkout -b feature/amazing-feature
```

### 2. Make Changes
- Write clean, readable code
- Add tests for new functionality
- Update documentation as needed
- Follow coding standards (below)

### 3. Test Thoroughly
```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Check for warnings
dotnet build /p:TreatWarningsAsErrors=true
```

### 4. Commit Changes
```bash
# Stage changes
git add .

# Commit with descriptive message
git commit -m "feat: add amazing feature

- Detailed description of changes
- Why this change was needed
- Any breaking changes"
```

**Commit Message Format:**
- `feat:` New feature
- `fix:` Bug fix
- `docs:` Documentation only
- `style:` Code style (formatting, etc.)
- `refactor:` Code refactoring
- `test:` Adding tests
- `chore:` Maintenance tasks

### 5. Push & Create PR
```bash
# Push to your fork
git push origin feature/amazing-feature

# Open Pull Request on GitHub
```

---

## 📐 Coding Standards

### General Principles
- **SOLID principles** - Especially Single Responsibility
- **MVVM architecture** - Maintain separation of concerns
- **Method size** - Keep methods under 20 lines
- **No magic numbers** - Use named constants
- **XML documentation** - Document all public APIs

### C# Style Guide

```csharp
// ✅ Good
public class CreatureManager
{
    private const int MaxPopulation = 10000;
    
    /// <summary>
    /// Spawns a new creature at the specified position.
    /// </summary>
    /// <param name="x">X coordinate.</param>
    /// <param name="y">Y coordinate.</param>
    public void SpawnCreature(double x, double y)
    {
        if (Population >= MaxPopulation)
        {
            return;
        }
        
        var creature = CreateCreature(x, y);
        AddToWorld(creature);
    }
}

// ❌ Avoid
public class CreatureManager
{
    public void SpawnCreature(double x, double y)
    {
        if (entities.Count >= 10000) return; // Magic number!
        entities.Add(new Creature { X = x, Y = y, Health = 100, Hunger = 0, Age = 0, Speed = 2.5 }); // Too long!
    }
}
```

### Naming Conventions
- **Classes:** `PascalCase`
- **Methods:** `PascalCase`
- **Properties:** `PascalCase`
- **Fields:** `_camelCase` (private), `PascalCase` (public)
- **Constants:** `PascalCase`
- **Parameters:** `camelCase`

### File Organization
```csharp
// 1. Using statements
using System;
using Terrarium.Logic.Entities;

// 2. Namespace
namespace Terrarium.Logic.Simulation
{
    // 3. XML documentation
    /// <summary>
    /// Manages creature lifecycle.
    /// </summary>
    public class CreatureManager
    {
        // 4. Constants
        private const int DefaultHealth = 100;
        
        // 5. Fields
        private readonly List<Creature> _creatures;
        
        // 6. Constructor
        public CreatureManager()
        {
            _creatures = new List<Creature>();
        }
        
        // 7. Properties
        public int Count => _creatures.Count;
        
        // 8. Public methods
        public void Update(double deltaTime)
        {
            // Implementation
        }
        
        // 9. Private methods
        private void ProcessCreature(Creature creature)
        {
            // Helper logic
        }
    }
}
```

---

## 🧪 Testing Guidelines

### Test Requirements
- **All new features** must have tests
- **Bug fixes** should include regression tests
- **Maintain 90%+ coverage**
- **Tests must pass** before PR approval

### Test Structure (AAA Pattern)
```csharp
[Fact]
public void SpawnCreature_WithValidPosition_CreatesCreature()
{
    // Arrange
    var manager = new CreatureManager();
    var expectedX = 100.0;
    var expectedY = 200.0;
    
    // Act
    manager.SpawnCreature(expectedX, expectedY);
    
    // Assert
    Assert.Equal(1, manager.Count);
    var creature = manager.GetCreatureAt(expectedX, expectedY);
    Assert.NotNull(creature);
}
```

### Test Naming
- Format: `MethodName_Scenario_ExpectedBehavior`
- Be descriptive and specific
- Test one thing per test

---

## 🔍 Pull Request Process

### Before Submitting
- [ ] Code follows style guidelines
- [ ] All tests pass locally
- [ ] Added tests for new features
- [ ] Updated documentation
- [ ] No merge conflicts with main
- [ ] Commits are clean and descriptive

### PR Template
```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
How did you test this?

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-reviewed code
- [ ] Commented complex code
- [ ] Updated documentation
- [ ] Added tests
- [ ] All tests pass
```

### Review Process
1. **Automated Checks** - CI/CD must pass
2. **Code Review** - At least one approval required
3. **Testing** - Reviewer tests functionality
4. **Merge** - Squash and merge to main

### After Merge
- Delete your feature branch
- Update your fork
- Celebrate! 🎉

---

## 💬 Community

### Communication Channels
- **GitHub Issues** - Bug reports and feature requests
- **GitHub Discussions** - General questions and ideas
- **Discord** (Coming Soon) - Real-time chat
- **Email** - support@example.com

### Getting Help
- Check [documentation](docs/)
- Search [existing issues](https://github.com/Hostilian/The-Desktop-Terrarium/issues)
- Ask in [Discussions](https://github.com/Hostilian/The-Desktop-Terrarium/discussions)

---

## 🏆 Recognition

Contributors are recognized in:
- **README.md** - Contributors section
- **CHANGELOG.md** - Release notes
- **GitHub Insights** - Contributors graph

Top contributors may be invited to join the core team!

---

## 📚 Additional Resources

- [Architecture Documentation](docs/ARCHITECTURE_DIAGRAM.md)
- [Development Guide](docs/DEVELOPMENT.md)
- [API Reference](docs/API.md)
- [User Guide](docs/USER_GUIDE.md)

---

## ❤️ Thank You!

Every contribution, no matter how small, makes Desktop Terrarium better. Thank you for being part of our community!

**Happy Coding!** 🌿✨

---

*Questions about contributing? Open an issue or start a discussion!*
