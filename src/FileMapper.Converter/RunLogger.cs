namespace FileMapper.Converter;

/// <summary>Writes structured log entries to a log file and to the console.</summary>
public class RunLogger : IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    /// <summary>Initialises a new <see cref="RunLogger"/> that writes to <paramref name="logFilePath"/>.</summary>
    /// <param name="logFilePath">Absolute path of the log file to create or append to.</param>
    public RunLogger(string logFilePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(logFilePath)!);
        _writer = new StreamWriter(logFilePath, append: true);
    }

    /// <summary>Writes an informational log entry.</summary>
    public void Info(string message) => Write("INFO", message);

    /// <summary>Writes a warning log entry.</summary>
    public void Warning(string message) => Write("WARN", message);

    /// <summary>Writes an error log entry.</summary>
    public void Error(string message) => Write("ERROR", message);

    private void Write(string level, string message)
    {
        var line = $"[{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}Z] [{level}] {message}";
        _writer.WriteLine(line);
        _writer.Flush();
        Console.WriteLine(line);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            _writer.Dispose();
            _disposed = true;
        }
    }
}
