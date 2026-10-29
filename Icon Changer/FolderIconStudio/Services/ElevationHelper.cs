using System.Diagnostics;
using System.Text.Json;
using System.Windows;

namespace FolderIconStudio.Services;

public sealed class ElevationJob
{
    public string Operation { get; set; } = ""; // "apply" | "restore"
    public string FolderPath { get; set; } = "";
    public string? IconPath { get; set; }
}

public static class ElevationHelper
{
    public const string JobFlag = "--elevated-job";

    private static readonly string JobDir = Path.Combine(
        Path.GetTempPath(), "FolderIconStudio", "elevation");

    public static bool TryRunElevatedAndWait(string folderPath, string? iconPath, bool restore, out string? error)
    {
        error = null;
        var exe = GetExecutablePath();
        if (string.IsNullOrEmpty(exe))
        {
            error = "Could not determine application path.";
            return false;
        }

        Directory.CreateDirectory(JobDir);
        var jobPath = Path.Combine(JobDir, $"{Guid.NewGuid():N}.json");
        var resultPath = jobPath + ".result";

        var job = new ElevationJob
        {
            Operation = restore ? "restore" : "apply",
            FolderPath = folderPath,
            IconPath = iconPath
        };

        try
        {
            File.WriteAllText(jobPath, JsonSerializer.Serialize(job));

            var psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"{JobFlag} \"{jobPath}\"",
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = Path.GetDirectoryName(exe) ?? Environment.CurrentDirectory
            };

            using var proc = Process.Start(psi);
            if (proc == null)
            {
                error = "Elevation was cancelled or could not start.";
                return false;
            }

            proc.WaitForExit(120_000);

            if (File.Exists(resultPath))
            {
                var result = File.ReadAllText(resultPath).Trim();
                if (proc.ExitCode == 0 && string.Equals(result, "ok", StringComparison.OrdinalIgnoreCase))
                    return true;

                error = string.IsNullOrWhiteSpace(result)
                    ? $"Elevated operation failed (exit code {proc.ExitCode})."
                    : result;
                return false;
            }

            if (proc.ExitCode == 0)
                return true;

            error = $"Elevated operation failed (exit code {proc.ExitCode}).";
            return false;
        }
        catch (System.ComponentModel.Win32Exception)
        {
            error = "Administrator access was cancelled.";
            return false;
        }
        finally
        {
            TryDelete(jobPath);
            TryDelete(resultPath);
        }
    }

    public static bool TryHandleElevatedJob(string[] args, FolderIconService iconService)
    {
        var jobPath = FindJobPath(args);
        if (jobPath == null)
            return false;

        var resultPath = jobPath + ".result";
        try
        {
            if (!File.Exists(jobPath))
            {
                WriteResult(resultPath, "Elevation job file not found.");
                Environment.Exit(1);
                return true;
            }

            var json = File.ReadAllText(jobPath);
            var job = JsonSerializer.Deserialize<ElevationJob>(json)
                ?? throw new InvalidOperationException("Invalid elevation job.");

            if (string.Equals(job.Operation, "restore", StringComparison.OrdinalIgnoreCase))
                iconService.RestoreDefault(job.FolderPath, elevated: true);
            else if (string.Equals(job.Operation, "apply", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(job.IconPath))
                    throw new InvalidOperationException("Icon path missing from elevation job.");
                iconService.ApplyIcon(job.FolderPath, job.IconPath, elevated: true);
            }
            else
                throw new InvalidOperationException($"Unknown elevation operation: {job.Operation}");

            WriteResult(resultPath, "ok");
            Environment.Exit(0);
            return true;
        }
        catch (Exception ex)
        {
            WriteResult(resultPath, ex.Message);
            MessageBox.Show(
                $"Failed to apply folder icon:\n{ex.Message}",
                "Folder Icon Studio",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Environment.Exit(1);
            return true;
        }
    }

    private static string? FindJobPath(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == JobFlag && i + 1 < args.Length)
                return args[i + 1].Trim('"');
            if (args[i].StartsWith(JobFlag + "=", StringComparison.Ordinal))
                return args[i][(JobFlag.Length + 1)..].Trim('"');
        }

        return null;
    }

    private static void WriteResult(string path, string message) =>
        File.WriteAllText(path, message);

    private static string? GetExecutablePath()
    {
        if (!string.IsNullOrEmpty(Environment.ProcessPath) && File.Exists(Environment.ProcessPath))
            return Environment.ProcessPath;

        var module = Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrEmpty(module) && File.Exists(module))
            return module;

        return null;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch { /* best effort */ }
    }
}
