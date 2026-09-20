namespace ytdlp_gui;

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

public class YtdlpRunner
{
    // Changed to return an async stream of strings
    public static async IAsyncEnumerable<string> RunYtdlpAsync(
        string filePath,
        IEnumerable<string> arguments,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            throw new Exception($"The yt-dlp executable was not found at the specified path: {filePath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = filePath,
            CreateNoWindow = true, // Set to true to prevent popup windows
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true, // yt-dlp often writes status updates here
        };

        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using var process = new Process { StartInfo = startInfo };

        if (!process.Start())
        {
            yield return "Error: Failed to start yt-dlp process.";
            yield break;
        }

        // Read both streams concurrently line-by-line
        var readOutTask = process.StandardOutput.ReadLineAsync(cancellationToken).AsTask();
        var readErrTask = process.StandardError.ReadLineAsync(cancellationToken).AsTask();

        while (readOutTask != null || readErrTask != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Prepare tasks to wait on
            var tasksToWait = new List<Task>();
            if (readOutTask != null) tasksToWait.Add(readOutTask);
            if (readErrTask != null) tasksToWait.Add(readErrTask);

            if (tasksToWait.Count == 0) break;

            // Wait until at least one stream has a new line ready
            Task completedTask = await Task.WhenAny(tasksToWait);

            if (completedTask == readOutTask)
            {
                string? line = await readOutTask;
                if (line != null)
                {
                    yield return line;
                    readOutTask = process.StandardOutput.ReadLineAsync(cancellationToken).AsTask(); // Queue next read
                }
                else
                {
                    readOutTask = null; // Standard Output stream reached EOF
                }
            }
            else if (completedTask == readErrTask)
            {
                string? line = await readErrTask;
                if (line != null)
                {
                    // Prefixing stderr to differentiate it, or handle it as you please
                    yield return $"[Error] {line}";
                    readErrTask = process.StandardError.ReadLineAsync(cancellationToken).AsTask(); // Queue next read
                }
                else
                {
                    readErrTask = null; // Standard Error stream reached EOF
                }
            }
        }

        // Clean up and ensure process is dead
        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            throw new Exception($"yt-dlp exited with error code {process.ExitCode}.");
        }
    }
}
