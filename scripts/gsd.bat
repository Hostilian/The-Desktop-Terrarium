@echo off
echo ========================================
echo  Desktop Terrarium - GSD Integration
echo ========================================
echo.
echo Get Shit Done (GSD) Command System
echo.
echo Available GSD Commands:
echo.
echo Project Management:
echo   gsd new-project     - Initialize new project
echo   gsd create-roadmap  - Create project roadmap
echo   gsd map-codebase    - Analyze existing codebase
echo.
echo Phase Management:
echo   gsd discuss-phase N    - Discuss phase vision
echo   gsd research-phase N   - Research phase requirements
echo   gsd plan-phase N       - Create detailed phase plan
echo   gsd execute-plan PATH  - Execute a plan file
echo.
echo Milestone Management:
echo   gsd discuss-milestone    - Plan next milestone
echo   gsd new-milestone NAME   - Create new milestone
echo   gsd complete-milestone V - Complete milestone
echo.
echo Progress & Status:
echo   gsd progress          - Check project status
echo   gsd resume-work       - Resume previous work
echo   gsd verify-work       - Verify completed work
echo.
echo Usage Examples:
echo   gsd new-project
echo   gsd plan-phase 1
echo   gsd execute-plan .planning/phases/01-foundation/01-01-PLAN.md
echo.
echo For detailed help: gsd help
echo.
echo ========================================

if "%1"=="" goto show_help
if "%1"=="help" goto show_help
if "%1"=="new-project" goto new_project
if "%1"=="create-roadmap" goto create_roadmap
if "%1"=="map-codebase" goto map_codebase
if "%1"=="discuss-phase" goto discuss_phase
if "%1"=="research-phase" goto research_phase
if "%1"=="plan-phase" goto plan_phase
if "%1"=="execute-plan" goto execute_plan
if "%1"=="discuss-milestone" goto discuss_milestone
if "%1"=="new-milestone" goto new_milestone
if "%1"=="complete-milestone" goto complete_milestone
if "%1"=="progress" goto progress
if "%1"=="resume-work" goto resume_work
if "%1"=="verify-work" goto verify_work

echo Unknown command: %1
echo Use 'gsd help' for available commands.
goto end

:show_help
echo.
echo GSD Command Reference:
echo.
echo GSD (Get Shit Done) creates hierarchical project plans optimized for
echo solo agentic development with Claude Code.
echo.
echo Core Workflow:
echo   Initialization → Planning → Execution → Milestone Completion
echo.
echo For detailed documentation, see .claude/commands/gsd/help.md
goto end

:new_project
echo Initializing new GSD project...
echo This will create .planning/PROJECT.md and .planning/config.json
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:new-project
goto end

:create_roadmap
echo Creating project roadmap...
echo This will create .planning/ROADMAP.md and phase directories
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:create-roadmap
goto end

:map_codebase
echo Mapping existing codebase...
echo This will analyze the codebase and create documentation
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:map-codebase
goto end

:discuss_phase
if "%2"=="" (
    echo Error: Phase number required
    echo Usage: gsd discuss-phase N
    goto end
)
echo Discussing phase %2 vision...
echo This will help articulate your vision for phase %2
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:discuss-phase %2
goto end

:research_phase
if "%2"=="" (
    echo Error: Phase number required
    echo Usage: gsd research-phase N
    goto end
)
echo Researching phase %2 requirements...
echo This will perform comprehensive research for phase %2
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:research-phase %2
goto end

:plan_phase
if "%2"=="" (
    echo Error: Phase number required
    echo Usage: gsd plan-phase N
    goto end
)
echo Planning phase %2...
echo This will create a detailed execution plan for phase %2
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:plan-phase %2
goto end

:execute_plan
if "%2"=="" (
    echo Error: Plan path required
    echo Usage: gsd execute-plan PATH
    goto end
)
echo Executing plan: %2
echo This will run the tasks in the specified plan file
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:execute-plan %2
goto end

:discuss_milestone
echo Discussing next milestone...
echo This will help plan what to build in the next milestone
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:discuss-milestone
goto end

:new_milestone
if "%2"=="" (
    echo Error: Milestone name required
    echo Usage: gsd new-milestone NAME
    goto end
)
echo Creating new milestone: %2
echo This will add a new milestone section to the roadmap
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:new-milestone %2
goto end

:complete_milestone
if "%2"=="" (
    echo Error: Version required
    echo Usage: gsd complete-milestone VERSION
    goto end
)
echo Completing milestone %2...
echo This will archive the milestone and prepare for next version
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:complete-milestone %2
goto end

:progress
echo Checking project progress...
echo This will show current status and next actions
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:progress
goto end

:resume_work
echo Resuming previous work...
echo This will restore full project context
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:resume-work
goto end

:verify_work
echo Verifying completed work...
echo This will check that work meets requirements
echo.
echo Note: This command should be run through Claude Code interface.
echo Use: /gsd:verify-work
goto end

:end
echo.
echo Press any key to continue...
pause >nul