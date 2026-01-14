# Context Engineering Quick Reference

## 🚀 Quick Start

### For New Features
1. **Plan**: Open fresh Gemini chat → Attach `PROJECT.md`, `ARCHITECTURE.md` → "Create execution plan for [feature]"
2. **Implement**: Open VS Code → Reference context files → Implement one step at a time with Copilot
3. **Test**: Build + run tests after each step

## 📋 Essential Commands

### Development
```bash
# Build project
dotnet build

# Run all tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run application
dotnet run --project Terrarium.Desktop/Terrarium.Desktop.csproj
```

### GSD Scripts (Alternative)
```bash
# Windows
.\gsd.bat help

# PowerShell
.\gsd.ps1 help
```

## 📁 Context Files

### Always Attach to Gemini
- `.planning/PROJECT.md` - Vision and goals
- `.planning/ARCHITECTURE.md` - System design
- `.planning/CONVENTIONS.md` - Coding standards

### Reference in VS Code
- Keep ARCHITECTURE.md and CONVENTIONS.md open in tabs
- Use for Copilot context

## 🎯 Gemini Prompts

### Feature Planning
```
I want to implement [FEATURE].
Based on attached architecture docs, create detailed Execution Plan.
Break into atomic steps with files, pseudo-code, verification.
Follow CONVENTIONS.md patterns.
```

### Bug Fixing
```
Bug: [DESCRIPTION]
Based on architecture, identify root cause and create fix plan.
Include verification steps.
```

## 🤖 Copilot Prompts

### Implementation
```
Implement Step 1: [PASTE STEP]
Follow patterns in .planning/CONVENTIONS.md
Reference .planning/ARCHITECTURE.md for integration
```

### Code Review
```
Review this code for CONVENTIONS.md compliance.
Check ARCHITECTURE.md integration.
Suggest improvements.
```

## ✅ Quality Checks

### After Each Step
- [ ] Code compiles: `dotnet build`
- [ ] Tests pass: `dotnet test`
- [ ] Follows conventions: Check CONVENTIONS.md
- [ ] Proper integration: Check ARCHITECTURE.md

### Before Commit
- [ ] All 152+ tests pass
- [ ] No new warnings
- [ ] Documentation updated
- [ ] Performance maintained

## 🔧 Common Fixes

### Missing Imports
```csharp
// Add to using statements
using System.IO;
using System.Linq;
using Terrarium.Logic.Entities;
```

### WPF Threading
```csharp
// For UI updates
Dispatcher.Invoke(() => {
    // UI code here
});
```

### Null Safety
```csharp
// Use nullable types
public string? OptionalProperty { get; set; }

// Null checks
if (entity != null) { /* use entity */ }
```

## 📊 Project Metrics

- **Tests**: 152+ passing
- **Coverage**: 81% target
- **Performance**: 60 FPS target
- **Architecture**: Layered (Logic/UI/Tests)

## 🚨 Emergency Fixes

### Build Broken
1. Check error messages
2. Add missing using statements
3. Verify file paths
4. Run `dotnet clean` then `dotnet build`

### Tests Failing
1. Check test output
2. Verify logic matches expectations
3. Update test data if needed
4. Check for integration issues

### UI Not Working
1. Check XAML bindings
2. Verify property names
3. Check Dispatcher usage
4. Review MainWindow code

## 🎯 Next Steps

### Phase 6: Emergent Complexity
1. Terrain-aware AI behaviors
2. Adaptive entity strategies
3. Dynamic ecosystem balancing
4. Advanced god powers

### Using Context Engineering
1. Plan with Gemini: "Create plan for terrain-aware movement"
2. Implement with Copilot: Step-by-step execution
3. Test thoroughly: Build + tests + manual verification

## 📞 Support

### When Stuck
1. Re-read relevant context files
2. Check existing similar code
3. Ask Copilot with specific context
4. Review error messages carefully

### Best Practices
- One feature at a time
- Small, testable changes
- Comprehensive testing
- Documentation updates

Remember: Context Engineering gives Gemini + Copilot the same "memory" as GSD. Use the context files to maintain consistency and quality.</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\QUICK_REFERENCE.md
