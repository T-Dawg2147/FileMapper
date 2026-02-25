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
// Load mapping definitions from the mappings folder
// ---------------------------------------------------------------------------
if (!Directory.Exists(config.MappingsFolderPath))
{
    logger.Error($"Mappings folder not found: {config.MappingsFolderPath}");
    Environment.Exit(2);
    return;
}

var serializer = new MappingSerializer();
var mappings = new List<MappingDefinition>();
foreach (var mapFile in Directory.GetFiles(config.MappingsFolderPath, "*.map.json"))
{
    try
    {
        var m = await serializer.LoadAsync(mapFile);
        mappings.Add(m);
        logger.Info($"Loaded mapping '{m.Name}' from '{mapFile}' " +
                    $"(ExpectedFileName: {m.ExpectedFileName ?? "(none)"}, IsPrefix: {m.FileNameIsPrefix}).");
    }
    catch (Exception ex)
    {
        logger.Warning($"Skipping invalid mapping file '{mapFile}': {ex.Message}");
    }
}

if (mappings.Count == 0)
{
    logger.Error($"No valid .map.json files found in '{config.MappingsFolderPath}'.");
    Environment.Exit(3);
    return;
}

// ---------------------------------------------------------------------------
// Resolve source files
// ---------------------------------------------------------------------------
List<string> sourceFiles;
if (Directory.Exists(config.SourcePath))
{
    sourceFiles = Directory.GetFiles(config.SourcePath).ToList();
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
// Helper: find the best mapping for a given source file
// ---------------------------------------------------------------------------
MappingDefinition? FindMapping(string sourceFileName)
{
    // First pass: exact match
    foreach (var m in mappings)
    {
        if (!string.IsNullOrEmpty(m.ExpectedFileName) && !m.FileNameIsPrefix &&
            string.Equals(sourceFileName, m.ExpectedFileName, StringComparison.OrdinalIgnoreCase))
        {
            return m;
        }
    }

    // Second pass: prefix match (longest prefix wins)
    MappingDefinition? bestPrefix = null;
    int bestLen = 0;
    foreach (var m in mappings)
    {
        if (!string.IsNullOrEmpty(m.ExpectedFileName) && m.FileNameIsPrefix &&
            sourceFileName.StartsWith(m.ExpectedFileName, StringComparison.OrdinalIgnoreCase) &&
            m.ExpectedFileName.Length > bestLen)
        {
            bestPrefix = m;
            bestLen = m.ExpectedFileName.Length;
        }
    }
    if (bestPrefix is not null) return bestPrefix;

    // Fallback: if there is exactly one mapping without an ExpectedFileName, use it
    var fallbacks = mappings.Where(m => string.IsNullOrEmpty(m.ExpectedFileName)).ToList();
    return fallbacks.Count == 1 ? fallbacks[0] : null;
}

// ---------------------------------------------------------------------------
// Convert each file
// ---------------------------------------------------------------------------
var engine = new FileConversionEngine();
int totalRecords = 0;
int failedFiles = 0;
int skippedFiles = 0;
var stopwatch = Stopwatch.StartNew();

Directory.CreateDirectory(config.OutputPath);

foreach (var sourceFile in sourceFiles)
{
    var fileName = Path.GetFileName(sourceFile);
    var mapping = FindMapping(fileName);

    if (mapping is null)
    {
        logger.Warning($"No matching mapping found for '{fileName}' — skipping.");
        skippedFiles++;
        continue;
    }

    var baseName = Path.GetFileNameWithoutExtension(sourceFile);
    var targetExt = mapping.TargetType.ToString().ToLowerInvariant();
    var outputFile = Path.Combine(config.OutputPath, $"{baseName}.{targetExt}");

    logger.Info($"Converting '{sourceFile}' → '{outputFile}' using mapping '{mapping.Name}'.");
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
logger.Info($"Run complete. Total records: {totalRecords}. Failed files: {failedFiles}. Skipped: {skippedFiles}. " +
            $"Time: {stopwatch.Elapsed.TotalSeconds:F2}s.");

if (failedFiles > 0)
{
    Environment.Exit(5);
}
