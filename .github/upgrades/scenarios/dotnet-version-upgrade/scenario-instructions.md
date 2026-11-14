# .NET Version Upgrade

## Strategy
**Selected**: All-at-Once
**Rationale**: Single .NET Framework WinForms project with no project dependencies; dependency-tier sequencing adds no value.

### Execution Constraints
- Convert the legacy project to SDK-style before changing its target framework.
- Upgrade the project in one atomic pass to `net10.0-windows` and keep Windows desktop support explicit.
- Apply package and compatibility changes before code fixes, then restore and build the scoped project.
- Preserve behavior and leave nullable reference types disabled; retain C# 7.3-compatible source patterns unless required by the target framework.
- Final validation must include a warning-free build and applicable tests without modifying unrelated solution projects.

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0
- **Scope**: FinalSystemUI/FinalSystemUI.csproj and required dependencies only
- **C# Compatibility**: Preserve C# 7.3-compatible source patterns during migration unless a framework or SDK requirement necessitates a safe change
- **Commit Strategy**: Single Commit at End

## Source Control
- **Source Branch**: master
- **Working Branch**: upgrade-dotnet-10
- **Commit Strategy**: After Each Task
- **Branch Sync**: Disabled

## Decisions
- Assess project-scoped dependencies rather than the entire solution, based on the user's explicit scope.
- Use net10.0 as the confirmed target framework.
- Use the All-at-Once strategy because the scoped upgrade contains one project with no project dependencies.

## Upgrade Options

### Strategy
- Upgrade Strategy: All-at-Once

### Project Structure
- Project Approach: In-place

### Compatibility
- Unsupported API Handling: Fix Inline
- Windows Native APIs: Windows Compatibility Pack

### Modernization
- Nullable Reference Types: Leave Disabled
