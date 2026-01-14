# Desktop Terrarium - Context Engineering Workflow

## Overview

This project uses **Context Engineering** - a manual adaptation of the GSD (Get Shit Done) methodology for Gemini + VS Code Copilot. Instead of automated tools, we create structured context files that give AI assistants the same "memory" and planning capabilities.

## Phase 1: Context Setup (One-Time)

### Required Context Files
```
.planning/
├── PROJECT.md      # Vision and goals
├── STACK.md        # Technologies and dependencies
├── ARCHITECTURE.md # System design and data flow
└── CONVENTIONS.md  # Coding standards and patterns
```

These files are already created and provide comprehensive context about the codebase.

## Phase 2: Feature Planning (Gemini)

### When You Want to Build Something New

1. **Open Fresh Gemini Chat**
   - Start a new conversation for each feature
   - This prevents context contamination

2. **Attach Context Files**
   ```
   Upload these files to Gemini:
   - .planning/PROJECT.md
   - .planning/ARCHITECTURE.md
   - .planning/CONVENTIONS.md (optional, for style guidance)
   ```

3. **Use the Architect Prompt**
   ```
   I want to implement [FEATURE DESCRIPTION].
   Based on the attached architecture documents, create a detailed Execution Plan.

   Break this into atomic, verifiable steps. For each step, provide:
   1. The file to edit/create
   2. A pseudo-code summary of the change
   3. Verification step (how to check if it works)
   4. Dependencies on other steps

   Follow the patterns in CONVENTIONS.md for naming and structure.
   ```

### Example Feature Request
```
"I want to add a 'Pause Simulation' button to the UI that stops all entity movement and reproduction while keeping the render loop active for inspection."
```

## Phase 3: Implementation (VS Code Copilot)

### Step-by-Step Execution

1. **Open VS Code**
   - Navigate to the project
   - Open the relevant file mentioned in the plan

2. **Reference Context**
   - Open `.planning/ARCHITECTURE.md` in a tab
   - Open `.planning/CONVENTIONS.md` in a tab
   - These provide implicit context for Copilot

3. **Implement One Step at a Time**
   ```
   Open Copilot Chat (Ctrl+Alt+I)
   Prompt: "Implement Step 1 from my plan: [Paste the instruction].
   Follow the patterns in .planning/CONVENTIONS.md and the architecture in .planning/ARCHITECTURE.md"

   Do NOT implement multiple steps at once.
   ```

4. **Verification After Each Step**
   - Run the verification check from the plan
   - Fix any issues before proceeding
   - Commit after each successful step

### Copilot Context Tips
- **Explicit References**: Mention specific files/methods from the architecture
- **Pattern Following**: Reference CONVENTIONS.md for naming and structure
- **Incremental Changes**: Make small, testable changes
- **Error Prevention**: Ask Copilot to check for common issues

## Phase 4: Integration Testing

### After Implementation
1. **Build the Project**: `dotnet build`
2. **Run Tests**: `dotnet test`
3. **Manual Testing**: Verify the feature works as expected
4. **Performance Check**: Ensure no performance regressions

### Common Issues
- **Missing Using Statements**: Add required imports
- **Null Reference Warnings**: Handle nullable types properly
- **UI Thread Issues**: Use Dispatcher for WPF updates
- **Test Failures**: Update tests for new functionality

## Workflow Examples

### Example 1: Adding a New Entity Type

**Gemini Prompt:**
```
I want to add a new "Omnivore" creature that can eat both plants and meat.
Based on the attached architecture, create an execution plan with atomic steps.
```

**Copilot Implementation:**
```
For each step in the plan:
1. Create Omnivore.cs in Entities/
2. Update SimulationEngine to handle omnivores
3. Add omnivore spawning logic
4. Update tests
```

### Example 2: UI Enhancement

**Gemini Prompt:**
```
Add a minimap showing entity positions in the top-right corner.
The minimap should update in real-time and allow clicking to center the view.
```

**Copilot Implementation:**
```
1. Add minimap canvas to MainWindow.xaml
2. Create MinimapRenderer class
3. Update render loop to draw minimap
4. Add click handling for minimap navigation
```

## Best Practices

### Planning Phase
- **Be Specific**: Clear, measurable requirements
- **Break Down**: Maximum 3-5 steps per plan
- **Dependencies**: Note step prerequisites
- **Verification**: Concrete testing criteria

### Implementation Phase
- **One Step at a Time**: Don't combine steps
- **Test Frequently**: Verify after each change
- **Follow Conventions**: Consistent with existing code
- **Document Changes**: Update comments and docs

### Context Management
- **Fresh Chats**: New Gemini conversation per feature
- **File Attachments**: Always attach context files
- **Reference Patterns**: Point to existing similar code
- **Incremental Context**: Build understanding gradually

## Quality Gates

### Code Quality
- ✅ Builds without errors
- ✅ All tests pass (152+ tests)
- ✅ Follows CONVENTIONS.md
- ✅ Proper documentation

### Feature Completeness
- ✅ Meets requirements from plan
- ✅ Integrates with existing systems
- ✅ Handles edge cases
- ✅ User-friendly interface

### Performance
- ✅ Maintains 60 FPS
- ✅ No memory leaks
- ✅ Efficient algorithms
- ✅ Scales with entity count

## Troubleshooting

### Common Gemini Issues
- **Vague Plans**: Add more specific requirements
- **Missing Context**: Attach all relevant files
- **Wrong Assumptions**: Reference specific architecture sections

### Common Copilot Issues
- **Wrong Patterns**: Explicitly reference CONVENTIONS.md
- **Missing Imports**: Check STACK.md for required namespaces
- **Integration Problems**: Review ARCHITECTURE.md data flow

### Build/Test Issues
- **Compilation Errors**: Check method signatures in existing code
- **Test Failures**: Update test expectations for new features
- **UI Problems**: Verify WPF threading requirements

## Advanced Usage

### Complex Features
For complex multi-step features:
1. Create high-level plan in Gemini
2. Break into sub-plans for each component
3. Implement and test each sub-plan separately
4. Integrate components in final step

### Refactoring
When refactoring existing code:
1. Plan the refactoring steps carefully
2. Ensure all tests still pass
3. Update documentation
4. Consider performance impact

### Debugging
When features don't work as expected:
1. Review the original plan vs implementation
2. Check integration points in ARCHITECTURE.md
3. Add logging/debugging as needed
4. Revert and reimplement if necessary

## Integration with Development

### Git Workflow
- **Feature Branches**: `feature/xxx-description`
- **Small Commits**: One per implemented step
- **Descriptive Messages**: Reference the step number

### Documentation Updates
- **README.md**: Update for new features
- **Architecture**: Update for structural changes
- **Context Files**: Keep current with code changes

This Context Engineering approach provides the same structured development benefits as GSD, but adapted for Gemini + VS Code Copilot. The key is maintaining comprehensive context files that give AI assistants the necessary "memory" to make informed decisions.</content>
<parameter name="filePath">c:\Users\Hostilian\The-Desktop-Terrarium\.planning\CONTEXT_ENGINEERING_WORKFLOW.md
