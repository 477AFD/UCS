# .NET Version Upgrade Plan

## Overview

**Target**: Upgrade only `FinalSystemUI/FinalSystemUI.csproj` from .NET Framework 4.7.2 to .NET 10 for Windows, including only dependencies required by this project.
**Scope**: One legacy WinForms project, approximately 9,456 lines of code, with no project-to-project dependencies and no detected NuGet package references.

## Upgrade Options

| Option | Selected | Why |
|--------|----------|-----|
| Upgrade Strategy | All-at-Once | A single scoped project has no dependency tiers to sequence. |
| Project Approach | In-place | The WinForms application has no dependent projects requiring a parallel transition. |
| Unsupported API Handling | Fix Inline | The upgraded project must finish without deferred compatibility stubs. |
| Windows Native APIs | Windows Compatibility Pack | The project has broad WinForms and GDI+ usage and remains Windows-specific. |
| Nullable Reference Types | Leave Disabled | Preserve existing null behavior while addressing the extensive framework migration surface. |

### Selected Strategy
**All-at-Once** — All projects upgraded simultaneously in a single operation.
**Rationale**: One legacy .NET Framework WinForms project with no project dependencies.

## Tasks

### 01-sdk-style-conversion: Convert FinalSystemUI to SDK-style

Convert the legacy non-SDK WinForms project to SDK-style while it remains on its current `net472` target. Preserve all source files, resources, WinForms designer relationships, application metadata, embedded resources, app configuration, platform-specific settings, and output behavior needed for a later .NET 10 Windows target. Do not modify unrelated projects.

**Done when**: `FinalSystemUI.csproj` is SDK-style, the project still represents the same WinForms application and assets, the project restores/builds on its current target without errors or warnings, and no unrelated project files are changed.

---

### 02-net10-winforms-migration: Upgrade FinalSystemUI to .NET 10 and fix compatibility issues

Change the converted project to `net10.0-windows` with explicit Windows desktop support, add only required compatibility packages, and resolve the assessed WinForms, GDI+, legacy-control, configuration, and `System.Data.SqlClient` compatibility issues inline while preserving existing behavior. Keep nullable reference types disabled and retain C# 7.3-compatible source patterns unless a target-framework requirement makes a safe change necessary. Restore dependencies and perform the scoped build/fix pass without changing unrelated projects.

**Done when**: the project targets `net10.0-windows`, required dependencies restore successfully, all compile errors and warnings in the modified project are resolved without warning suppression, legacy controls/configuration and incompatible APIs have documented or implemented replacements, behavior-preserving code compiles, and applicable tests pass.

---

### 03-final-validation: Validate the scoped upgrade

Validate the complete project-scoped upgrade after the atomic migration. Build the upgraded project and the solution only as needed to verify that the changed project does not regress solution integration, run discovered tests where applicable, inspect warnings, and document any runtime/UI checks that require user verification because WinForms execution is environment-dependent.

**Done when**: the upgraded project builds with zero errors and zero warnings, applicable tests pass, only the specified project and required workflow artifacts/dependencies were changed, and remaining manual verification items are documented clearly.
