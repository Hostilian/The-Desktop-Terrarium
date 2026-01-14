# Desktop Terrarium - GSD Integration

This repository includes the **Get Shit Done (GSD)** project management system for efficient solo development with Claude Code.

## What is GSD?

GSD is a hierarchical project planning system optimized for solo agentic development. It creates structured plans that Claude can execute efficiently, breaking down complex projects into manageable phases and tasks.

## Quick Start

1. **Initialize Project**: `gsd new-project`
2. **Create Roadmap**: `gsd create-roadmap`
3. **Plan First Phase**: `gsd plan-phase 1`
4. **Execute Plan**: `gsd execute-plan .planning/phases/01-foundation/01-01-PLAN.md`

## Core Workflow

```
Initialization → Planning → Execution → Milestone Completion
```

### 1. Project Initialization
- `gsd new-project` - Initialize with project brief and configuration
- `gsd create-roadmap` - Create roadmap and phase breakdown
- `gsd map-codebase` - Analyze existing codebase (for brownfield projects)

### 2. Phase Planning
- `gsd discuss-phase N` - Articulate vision for phase N
- `gsd research-phase N` - Research requirements and best practices
- `gsd plan-phase N` - Create detailed execution plan

### 3. Execution
- `gsd execute-plan PATH` - Execute a PLAN.md file
- `gsd progress` - Check status and next actions

### 4. Milestone Management
- `gsd discuss-milestone` - Plan next milestone features
- `gsd new-milestone NAME` - Create new milestone
- `gsd complete-milestone VERSION` - Archive completed milestone

## GSD Commands

### Project Management
```bash
gsd new-project        # Initialize new project
gsd create-roadmap     # Create project roadmap
gsd map-codebase       # Analyze existing codebase
```

### Phase Management
```bash
gsd discuss-phase 1    # Discuss phase vision
gsd research-phase 1   # Research phase requirements
gsd plan-phase 1       # Create detailed plan
gsd execute-plan path  # Execute plan file
```

### Progress Tracking
```bash
gsd progress           # Check project status
gsd resume-work        # Resume previous work
gsd verify-work        # Verify completed work
```

### Milestone Management
```bash
gsd discuss-milestone     # Plan next milestone
gsd new-milestone "v2.0"  # Create new milestone
gsd complete-milestone 1.0.0  # Complete milestone
```

## File Structure

```
.planning/
├── PROJECT.md          # Project vision and requirements
├── ROADMAP.md          # Phase breakdown and milestones
├── STATE.md           # Project memory and context
├── config.json        # Workflow configuration
└── phases/
    └── 01-foundation/
        └── 01-01-PLAN.md  # Detailed execution plans
```

## Integration with Desktop Terrarium

The GSD system is integrated into the Desktop Terrarium development workflow to manage the complex God Simulator features:

- **Phase 1-5**: Core God Simulator implementation
- **Phase 6**: Emergent Complexity
- **Phase 7**: Polish and UI improvements
- **Phase 8**: Technical optimization
- **Phase 9**: Content expansion
- **Phase 10**: QA and finalization

## Usage in Claude Code

GSD commands are designed to work with Claude Code's slash commands:

- `/gsd:new-project` - Initialize project
- `/gsd:plan-phase 1` - Plan specific phase
- `/gsd:execute-plan path` - Execute plan

## Scripts

- `gsd.bat` - Windows batch script
- `gsd.ps1` - Cross-platform PowerShell script

Both scripts provide command-line access to GSD functionality and guide you to use the proper Claude Code interface.

## Documentation

Detailed documentation is available in `.claude/commands/gsd/help.md` and the references directory.