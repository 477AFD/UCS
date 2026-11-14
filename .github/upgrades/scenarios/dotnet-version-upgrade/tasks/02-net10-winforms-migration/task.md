# 02-net10-winforms-migration: Upgrade FinalSystemUI to .NET 10 and fix compatibility issues

Change the converted project to `net10.0-windows` with explicit Windows desktop support, add only required compatibility packages, and resolve the assessed WinForms, GDI+, legacy-control, configuration, and `System.Data.SqlClient` compatibility issues inline while preserving existing behavior. Keep nullable reference types disabled and retain C# 7.3-compatible source patterns unless a target-framework requirement makes a safe change necessary. Restore dependencies and perform the scoped build/fix pass without changing unrelated projects.

## Research Findings

- **Project scope**: `FinalSystemUI/FinalSystemUI.csproj` only; no project-to-project dependencies were found.
- **Current SDK-style properties**: `TargetFramework=net472`, `UseWindowsForms=true`, `ImportWindowsDesktopTargets=true`, `GenerateAssemblyInfo=false`; no `LangVersion`, `Nullable`, or `PackageReference` entries.
- **Required package surfaces**: `System.Configuration.ConfigurationManager` is required for `ConfigurationManager.ConnectionStrings`; `System.Data.SqlClient` is required for the existing `SqlConnection`, `SqlCommand`, `SqlParameter`, and `SqlDataReader` code. Resolved stable versions for `net10.0-windows` are `11.0.0` and `4.9.1` respectively.
- **Selected compatibility choice**: add stable `Microsoft.Windows.Compatibility` `11.0.0` because assessment detected broad Windows Forms/GDI+ usage and the confirmed option keeps the application Windows-specific.
- **Configuration**: `App.config` contains the `UniformDb` SQL Server LocalDB connection string and an obsolete .NET Framework `<startup><supportedRuntime ... /></startup>` element; preserve connection strings/app settings and remove only the framework-specific startup declaration.
- **Code inventory**: `Database/Db.cs` uses `System.Configuration` and `System.Data.SqlClient`; the remaining application surface is WinForms designer code and `System.Drawing`. No legacy controls such as `StatusBar`, `MainMenu`, or `ToolBar` were identified in the pre-migration inventory; `DataGridView` is already used.
- **Compatibility constraints**: keep `<Nullable>disable</Nullable>` and explicitly set `<LangVersion>7.3</LangVersion>` to preserve the requested source-language compatibility while the target framework changes.
- **Validation tooling**: use Visual Studio `MSBuild.exe` with restore because the project is WinForms and contains designer-generated `.resx` resources. Build only `FinalSystemUI.csproj` during this task.
- **Stubs**: no existing `// STUB:` markers were found, so no stub-resolution decomposition is required.

## Migration Boundaries

- Replace the framework target with `net10.0-windows`; do not multi-target.
- Keep the existing `System.Data.SqlClient` API surface to minimize behavior changes; do not broaden the task to `Microsoft.Data.SqlClient` migration.
- Do not enable nullable reference types or apply unrelated C# syntax modernization.
- Do not modify projects outside `FinalSystemUI`.

**Done when**: the project targets `net10.0-windows`, required dependencies restore successfully, all compile errors and warnings in the modified project are resolved without warning suppression, legacy controls/configuration and incompatible APIs have documented or implemented replacements, behavior-preserving code compiles, and applicable tests pass.
