#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Desktop Terrarium - GSD Integration Script
.DESCRIPTION
    Get Shit Done (GSD) command system for project management
#>

param(
    [Parameter(Position = 0)]
    [string]$Command,

    [Parameter(Position = 1)]
    [string]$Argument
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Desktop Terrarium - GSD Integration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Get Shit Done (GSD) Command System" -ForegroundColor Yellow
Write-Host ""

function Show-Help {
    Write-Host "Available GSD Commands:" -ForegroundColor Green
    Write-Host ""
    Write-Host "Project Management:" -ForegroundColor Magenta
    Write-Host "  gsd new-project     - Initialize new project"
    Write-Host "  gsd create-roadmap  - Create project roadmap"
    Write-Host "  gsd map-codebase    - Analyze existing codebase"
    Write-Host ""
    Write-Host "Phase Management:" -ForegroundColor Magenta
    Write-Host "  gsd discuss-phase N    - Discuss phase vision"
    Write-Host "  gsd research-phase N   - Research phase requirements"
    Write-Host "  gsd plan-phase N       - Create detailed phase plan"
    Write-Host "  gsd execute-plan PATH  - Execute a plan file"
    Write-Host ""
    Write-Host "Milestone Management:" -ForegroundColor Magenta
    Write-Host "  gsd discuss-milestone    - Plan next milestone"
    Write-Host "  gsd new-milestone NAME   - Create new milestone"
    Write-Host "  gsd complete-milestone V - Complete milestone"
    Write-Host ""
    Write-Host "Progress & Status:" -ForegroundColor Magenta
    Write-Host "  gsd progress          - Check project status"
    Write-Host "  gsd resume-work       - Resume previous work"
    Write-Host "  gsd verify-work       - Verify completed work"
    Write-Host ""
    Write-Host "Usage Examples:" -ForegroundColor Blue
    Write-Host "  .\gsd.ps1 new-project"
    Write-Host "  .\gsd.ps1 plan-phase 1"
    Write-Host "  .\gsd.ps1 execute-plan .planning/phases/01-foundation/01-01-PLAN.md"
    Write-Host ""
    Write-Host "For detailed help: .\gsd.ps1 help" -ForegroundColor Blue
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
}

function Invoke-GsdCommand {
    param([string]$GsdCommand, [string]$Arg = "")

    Write-Host ""
    Write-Host "Note: GSD commands should be run through Claude Code interface." -ForegroundColor Yellow
    Write-Host "Use: /$GsdCommand$($Arg -replace '^', ':')" -ForegroundColor Cyan
    Write-Host ""
}

switch ($Command.ToLower()) {
    "" { Show-Help }
    "help" { Show-Help }
    "new-project" {
        Write-Host "Initializing new GSD project..." -ForegroundColor Green
        Write-Host "This will create .planning/PROJECT.md and .planning/config.json"
        Invoke-GsdCommand "gsd:new-project"
    }
    "create-roadmap" {
        Write-Host "Creating project roadmap..." -ForegroundColor Green
        Write-Host "This will create .planning/ROADMAP.md and phase directories"
        Invoke-GsdCommand "gsd:create-roadmap"
    }
    "map-codebase" {
        Write-Host "Mapping existing codebase..." -ForegroundColor Green
        Write-Host "This will analyze the codebase and create documentation"
        Invoke-GsdCommand "gsd:map-codebase"
    }
    "discuss-phase" {
        if (-not $Argument) {
            Write-Host "Error: Phase number required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 discuss-phase N"
            return
        }
        Write-Host "Discussing phase $Argument vision..." -ForegroundColor Green
        Write-Host "This will help articulate your vision for phase $Argument"
        Invoke-GsdCommand "gsd:discuss-phase" $Argument
    }
    "research-phase" {
        if (-not $Argument) {
            Write-Host "Error: Phase number required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 research-phase N"
            return
        }
        Write-Host "Researching phase $Argument requirements..." -ForegroundColor Green
        Write-Host "This will perform comprehensive research for phase $Argument"
        Invoke-GsdCommand "gsd:research-phase" $Argument
    }
    "plan-phase" {
        if (-not $Argument) {
            Write-Host "Error: Phase number required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 plan-phase N"
            return
        }
        Write-Host "Planning phase $Argument..." -ForegroundColor Green
        Write-Host "This will create a detailed execution plan for phase $Argument"
        Invoke-GsdCommand "gsd:plan-phase" $Argument
    }
    "execute-plan" {
        if (-not $Argument) {
            Write-Host "Error: Plan path required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 execute-plan PATH"
            return
        }
        Write-Host "Executing plan: $Argument" -ForegroundColor Green
        Write-Host "This will run the tasks in the specified plan file"
        Invoke-GsdCommand "gsd:execute-plan" $Argument
    }
    "discuss-milestone" {
        Write-Host "Discussing next milestone..." -ForegroundColor Green
        Write-Host "This will help plan what to build in the next milestone"
        Invoke-GsdCommand "gsd:discuss-milestone"
    }
    "new-milestone" {
        if (-not $Argument) {
            Write-Host "Error: Milestone name required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 new-milestone NAME"
            return
        }
        Write-Host "Creating new milestone: $Argument" -ForegroundColor Green
        Write-Host "This will add a new milestone section to the roadmap"
        Invoke-GsdCommand "gsd:new-milestone" $Argument
    }
    "complete-milestone" {
        if (-not $Argument) {
            Write-Host "Error: Version required" -ForegroundColor Red
            Write-Host "Usage: .\gsd.ps1 complete-milestone VERSION"
            return
        }
        Write-Host "Completing milestone $Argument..." -ForegroundColor Green
        Write-Host "This will archive the milestone and prepare for next version"
        Invoke-GsdCommand "gsd:complete-milestone" $Argument
    }
    "progress" {
        Write-Host "Checking project progress..." -ForegroundColor Green
        Write-Host "This will show current status and next actions"
        Invoke-GsdCommand "gsd:progress"
    }
    "resume-work" {
        Write-Host "Resuming previous work..." -ForegroundColor Green
        Write-Host "This will restore full project context"
        Invoke-GsdCommand "gsd:resume-work"
    }
    "verify-work" {
        Write-Host "Verifying completed work..." -ForegroundColor Green
        Write-Host "This will check that work meets requirements"
        Invoke-GsdCommand "gsd:verify-work"
    }
    default {
        Write-Host "Unknown command: $Command" -ForegroundColor Red
        Write-Host "Use '.\gsd.ps1 help' for available commands." -ForegroundColor Yellow
    }
}

Write-Host ""
Write-Host "GSD Command Reference:" -ForegroundColor Green
Write-Host ""
Write-Host "GSD (Get Shit Done) creates hierarchical project plans optimized for" -ForegroundColor White
Write-Host "solo agentic development with Claude Code." -ForegroundColor White
Write-Host ""
Write-Host "Core Workflow:" -ForegroundColor Yellow
Write-Host "  Initialization → Planning → Execution → Milestone Completion" -ForegroundColor White
Write-Host ""
Write-Host "For detailed documentation, see .claude/commands/gsd/help.md" -ForegroundColor Blue