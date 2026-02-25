# FileMapper

A .NET 8 C# solution for visually designing file-to-file field mappings and batch-converting files between formats (JSON, CSV, XML, XLSX, Fixed-Width Text).

---

## Project Structure

```
FileMapper/
├── FileMapper.slnx                  # Solution file
├── README.md
└── src/
    ├── FileMapper.Core/             # Shared class library
    │   ├── Models/                  # MappingDefinition, FieldMapping, etc.
    │   ├── Parsers/                 # IFileParser + JSON/CSV/XML/XLSX/Fixed-Width implementations
    │   ├── Writers/                 # IFileWriter + JSON/CSV/XML/XLSX/Fixed-Width implementations
    │   ├── Conversion/              # FileConversionEngine + TransformationEngine
    │   ├── Flattening/              # PathFlattener — common-prefix algorithm
    │   ├── Serialization/           # MappingSerializer (.map.json read/write)
    │   └── Validation/              # TypeCompatibilityValidator
    │
    ├── FileMapper.UI/               # WPF mapping designer (Windows only)
    │   └── ViewModels/              # MVVM ViewModels
    │
    └── FileMapper.Converter/        # Console batch converter
        ├── Program.cs               # Entry point
        ├── ConverterConfig.cs       # converter-config.json model
        └── RunLogger.cs             # Structured run logging
```

### `FileMapper.Core` (Class Library — Shared)

Shared logic referenced by both the UI and the Converter. Contains:

- **Mapping model** (`MappingDefinition`, `FieldMapping`, `TransformationRule`, …) serialised as human-readable `.map.json` files.
- **File parsers** (`IFileParser`) — extract field names/paths from JSON, CSV, XML, XLSX and fixed-width text files.
- **File writers** (`IFileWriter`) — write converted records to any supported format.
- **Conversion engine** (`FileConversionEngine`) — applies mappings and transformations, returns warnings per-row.
- **Flattening** (`PathFlattener`) — strips the longest common ancestor prefix from hierarchical paths and ensures uniqueness by retaining additional parent levels.
- **Validation** (`TypeCompatibilityValidator`) — returns `Info`, `Warning`, or `Error` severity for source/target type pairs.
- **Serialisation** (`MappingSerializer`) — reads/writes `.map.json` using `System.Text.Json`.

### `FileMapper.UI` (WPF Application — Mapping Designer)

A WPF desktop application for visually creating, editing, and saving mapping files.

### `FileMapper.Converter` (Console Application — Batch Converter)

A headless console application that reads a mapping file and a config file, converts one or many source files, writes detailed logs, and returns exit codes suitable for Windows Task Scheduler.

---

## How to Build

**Prerequisites:** .NET 8 SDK or later. WPF requires Windows (the project uses `EnableWindowsTargeting` for cross-compilation on Linux CI).

```bash
# Restore & build everything
dotnet build FileMapper.slnx

# Or build individual projects
dotnet build src/FileMapper.Core/FileMapper.Core.csproj
dotnet build src/FileMapper.UI/FileMapper.UI.csproj        # Windows only at runtime
dotnet build src/FileMapper.Converter/FileMapper.Converter.csproj
```

---

## Using the Mapper UI

1. **Launch** `FileMapper.UI.exe` on Windows.
2. Enter a **Mapping Name** in the top bar.
3. **Source File section** — choose the source type from the dropdown, browse to a sample source file, then click **Load Fields**. For JSON/XML sources you will be prompted whether to flatten nested paths.
4. **Target File section** — choose the target type, browse to a sample target file, then click **Load Fields**.
5. In the **Source Fields** list (left panel) click a field, then in the **Target Fields** list (right panel) click the corresponding field, then click **➕ Add Mapping**.
6. Optionally edit **Source Type** / **Target Type** columns in the mapping grid to enable type-compatibility checking.
7. When a mapping has incompatible types a **warning dialog** is shown — click *OK* to acknowledge and proceed, or *Cancel* to discard.
8. Use **💾 Save Mapping** to write the `.map.json` file. Use **📂 Open Mapping** to reload and edit an existing mapping.

---

## Configuring and Running the Converter

### Configuration file (`converter-config.json`)

```json
{
  "mappingFilePath": "C:\\Mappings\\orders.map.json",
  "sourcePath":      "C:\\Input\\orders.json",
  "outputPath":      "C:\\Output\\orders.csv",
  "logFilePath":     "C:\\Logs\\converter.log"
}
```

| Field | Description |
|---|---|
| `mappingFilePath` | Path to the `.map.json` file created by the Mapper UI. |
| `sourcePath` | Path to a single source file **or** a folder for batch processing. |
| `outputPath` | Path for the output file, or a folder when `sourcePath` is a folder. |
| `logFilePath` | *(Optional)* Path of the log file. Defaults to `logs/run-<timestamp>.log` next to the executable. |

### Running

```bash
# Use config file next to the executable
FileMapper.Converter.exe

# Specify a config file path explicitly
FileMapper.Converter.exe C:\Configs\my-config.json
```

### Exit Codes

| Code | Meaning |
|---|---|
| `0` | Success — all records converted. |
| `1` | Configuration file missing or malformed. |
| `2` | Mapping file not found. |
| `3` | Mapping file could not be deserialised. |
| `4` | Source path not found. |
| `5` | One or more files failed to convert. |

---

## Windows Task Scheduler Setup

1. Open **Task Scheduler** → **Create Task**.
2. **General** tab — give the task a name; run as a service account with access to the input/output folders.
3. **Triggers** — configure the schedule (e.g., daily at 02:00).
4. **Actions** → **Start a program**:
   - Program: `C:\Tools\FileMapper\FileMapper.Converter.exe`
   - Arguments: `C:\Configs\converter-config.json` *(optional)*
   - Start in: `C:\Tools\FileMapper\`
5. **Settings** → enable *"If the task fails, restart every …"* as required.

### Configuring an On-Failure Alert

Task Scheduler does not send emails natively in modern Windows, but you can chain tasks:

1. Create a second task called **"FileMapper — Failure Alert"** that runs a PowerShell notification script, sends an email via SMTP, or writes to an event log.
2. In the primary converter task, go to **Actions** → add a second action:
   - **Action type**: Start a program
   - Program: `schtasks.exe`
   - Arguments: `/Run /TN "FileMapper — Failure Alert"`
3. In **Conditions** / **Settings**, set this action to run only when the exit code is **non-zero** by using the *"Run another task on failure"* capability available through the Task Scheduler COM API or a wrapper script:

   ```powershell
   # wrapper.ps1
   & "C:\Tools\FileMapper\FileMapper.Converter.exe" $args
   if ($LASTEXITCODE -ne 0) {
       Send-MailMessage -To "ops@example.com" `
                        -Subject "FileMapper FAILED (exit $LASTEXITCODE)" `
                        -Body "Check log at C:\Logs\converter.log" `
                        -SmtpServer "mail.example.com" `
                        -From "noreply@example.com"
   }
   ```

   Point the Task Scheduler action at `powershell.exe -File C:\Tools\wrapper.ps1` instead.
