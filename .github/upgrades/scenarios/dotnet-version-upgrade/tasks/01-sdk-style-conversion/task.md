# 01-sdk-style-conversion: Convert FinalSystemUI to SDK-style

Convert the legacy non-SDK WinForms project to SDK-style while it remains on its current `net472` target. Preserve all source files, resources, WinForms designer relationships, application metadata, embedded resources, app configuration, platform-specific settings, and output behavior needed for a later .NET 10 Windows target. Do not modify unrelated projects.

## Research Findings

- **Scoped project**: `FinalSystemUI/FinalSystemUI.csproj`; topological dependency analysis reports this is the only project in scope and it has no project-to-project dependencies.
- **Current format**: legacy MSBuild project (`ToolsVersion="15.0"`, XML namespace, explicit `Microsoft.CSharp.targets` import), targeting `v4.7.2`.
- **Project type**: WinForms executable (`OutputType=WinExe`) with 54 source/resource files, 11 WinForms designer/resource pairs, generated `Properties/Resources.Designer.cs` and `Properties/Settings.Designer.cs`, embedded PNG content, SQL script content, and `App.config`.
- **References**: explicit framework assembly references only; no `packages.config` was found and no NuGet package references were detected by assessment.
- **Assessment signals**: one non-SDK project; 9,362 binary-incompatible and 1,921 source-incompatible API findings are reserved for the later TFM/code migration task. This task must remain structural and must not change the target framework or introduce package drift.
- **Stubs**: no existing `// STUB:` markers were found in the scoped project.
- **Build tooling**: because this is a WinForms project with designer-generated `.resx` resources and currently targets .NET Framework, use Visual Studio `MSBuild.exe` with restore for validation after conversion.
- **Solution path correction**: the supplied `FinalSystemUI.sln` path does not exist in the repository; the repository contains `UCS.sln`. The project-only scope remains unchanged.

## Conversion Boundaries

- Use `convert_project_to_sdk_style` rather than manually rewriting the project XML.
- Preserve `net472` during this task; `net10.0-windows` is handled by task `02-net10-winforms-migration`.
- Preserve designer/resource metadata and non-code content required by the application.

**Done when**: `FinalSystemUI.csproj` is SDK-style, the project still represents the same WinForms application and assets, the project restores/builds on its current target without errors or warnings, and no unrelated project files are changed.
