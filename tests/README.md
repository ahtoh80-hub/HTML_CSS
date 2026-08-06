# Unit tests

xUnit tests for the C# projects in this repository.

## Running

```bash
dotnet test tests/HTML_CSS.Tests.sln
```

With a coverage report (Cobertura, written to `TestResults/`):

```bash
dotnet test tests/HTML_CSS.Tests.sln --settings tests/coverlet.runsettings --collect:"XPlat Code Coverage"
```

Requires the .NET 8 SDK. The tests run on Linux, macOS and Windows.

## Layout

| Project | Sources under test |
| --- | --- |
| `PJT1.Tests` | `PJT1/Models/DataBD.cs`, `PJT1/Repositories/*.cs`, `PJT1/Services/ExcelService.cs` |
| `IO_PJT.Tests` | `IO_PJT/Models/IoPointModels.cs`, `IO_PJT/Services/DatabaseService.cs` |

`PJT1` and `IO_PJT` are WinForms applications (`net8.0-windows`, `WinExe`), which a cross-platform
test project cannot reference. The test projects therefore compile the UI-independent source files
directly (`<Compile Include="../../..." />`), which also means the test assemblies themselves have to
be instrumented for coverage — hence `IncludeTestAssembly` in `coverlet.runsettings`.

## Not covered

* Forms (`MainForm`, `FormDB`, `AddRecordForm`, `FormatSelectionDialog`) and `Program` — WinForms UI.
* `PJT1/Services/TxtExportService.cs` — the formatting logic is `private static` and every public
  entry point shows a `MessageBox`/`SaveFileDialog`, so it needs a seam before it can be tested.
* `IO_PJT/Services/DatabaseService.cs` beyond path handling — the remaining members open OleDb
  connections and need the Windows ACE/Jet providers plus a real Access database.
* `IO_PJT/Utils/Logger.cs` — writes into a `RichTextBox` and calls `Application.DoEvents()`.
