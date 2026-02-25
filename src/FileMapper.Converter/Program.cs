using FileMapper.Converter;
using FileMapper.Core.Conversion;
using FileMapper.Core.Models;
using FileMapper.Core.Parsers;
using FileMapper.Core.Serialization;
using System.Diagnostics;
using System.Text.Json;

// ---------------------------------------------------------------------------
// Determine configuration file path
// ---------------------------------------------------------------------------
string configPath;
if (args.Length > 0 && File.Exists(args[0]))
{
    configPath = args[0];
}
else
{
    configPath = Path.Combine(AppContext.BaseDirectory, "converter-config.json");
}

// ---------------------------------------------------------------------------
// Load configuration
// ---------------------------------------------------------------------------
ConverterConfig config;
try
{
    var json = await File.ReadAllTextAsync(configPath);
    config = JsonSerializer.Deserialize<ConverterConfig>(json,
        new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
        ?? throw new InvalidOperationException("Configuration file is empty or malformed.");
}
catch (Exception ex)
{
    Console.Error.WriteLine($"[ERROR] Could not load configuration from '{configPath}': {ex.Message}");
    Environment.Exit(1);
    return; // unreachable — satisfies compiler
}

// ---------------------------------------------------------------------------
// Initialise logger
// ---------------------------------------------------------------------------
var logDir = config.LogFilePath is { Length: > 0 }
    ? Path.GetDirectoryName(config.LogFilePath)!
    : Path.Combine(AppContext.BaseDirectory, "logs");

var logFile = config.LogFilePath is { Length: > 0 }
    ? config.LogFilePath
    : Path.Combine(logDir, $"run-{DateTimeOffset.UtcNow:yyyyMMdd-HHmmss}.log");

using var logger = new RunLogger(logFile);
logger.Info($"FileMapper.Converter starting. Config: {configPath}");

// ---------------------------------------------------------------------------
// Load mapping definition
// ---------------------------------------------------------------------------
if (!File.Exists(config.MappingFilePath))
{
    logger.Error($"Mapping file not found: {config.MappingFilePath}");
    Environment.Exit(2);
    return;
}

var serializer = new MappingSerializer();
MappingDefinition mapping;
try
{
    mapping = await serializer.LoadAsync(config.MappingFilePath);
    logger.Info($"Loaded mapping '{mapping.Name}' ({mapping.SourceType} → {mapping.TargetType}).");
}
catch (Exception ex)
{
    logger.Error($"Failed to load mapping file: {ex.Message}");
    Environment.Exit(3);
    return;
}

// ---------------------------------------------------------------------------
// Resolve source files
// ---------------------------------------------------------------------------
List<string> sourceFiles;
if (Directory.Exists(config.SourcePath))
{
    var ext = FileParserFactory.DetectFromExtension("dummy." + mapping.SourceType.ToString().ToLowerInvariant()) is { } ft
        ? ft.ToString().ToLowerInvariant()
        : "*";
    sourceFiles = Directory.GetFiles(config.SourcePath, $"*.{ext}").ToList();
    logger.Info($"Batch mode: found {sourceFiles.Count} file(s) in '{config.SourcePath}'.");
}
else if (File.Exists(config.SourcePath))
{
    sourceFiles = new List<string> { config.SourcePath };
}
else
{
    logger.Error($"Source path not found: {config.SourcePath}");
    Environment.Exit(4);
    return;
}

// ---------------------------------------------------------------------------
// Convert each file
// ---------------------------------------------------------------------------
var engine = new FileConversionEngine();
int totalRecords = 0;
int failedFiles = 0;
var stopwatch = Stopwatch.StartNew();

bool isBatch = sourceFiles.Count > 1;
if (isBatch)
    Directory.CreateDirectory(config.OutputPath);

foreach (var sourceFile in sourceFiles)
{
    string outputFile;
    if (isBatch)
    {
        var baseName = Path.GetFileNameWithoutExtension(sourceFile);
        var targetExt = mapping.TargetType.ToString().ToLowerInvariant();
        outputFile = Path.Combine(config.OutputPath, $"{baseName}.{targetExt}");
    }
    else
    {
        outputFile = config.OutputPath;
        var outDir = Path.GetDirectoryName(outputFile);
        if (!string.IsNullOrEmpty(outDir))
            Directory.CreateDirectory(outDir);
    }

    logger.Info($"Converting '{sourceFile}' → '{outputFile}'");
    try
    {
        var warnings = new List<(int Row, string Field, string Message)>();
        var count = await engine.ConvertAsync(sourceFile, outputFile, mapping,
            (row, field, msg) => warnings.Add((row, field, msg)));

        foreach (var (row, field, msg) in warnings)
            logger.Warning($"  Row {row}, field '{field}': {msg}");

        logger.Info($"  Success — {count} record(s) written.");
        totalRecords += count;
    }
    catch (Exception ex)
    {
        logger.Error($"  Conversion failed for '{sourceFile}': {ex.Message}");
        failedFiles++;
    }
}

stopwatch.Stop();
logger.Info($"Run complete. Total records: {totalRecords}. Failed files: {failedFiles}. " +
            $"Time: {stopwatch.Elapsed.TotalSeconds:F2}s.");

if (failedFiles > 0)
{
    Environment.Exit(5);
}
