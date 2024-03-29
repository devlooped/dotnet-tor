﻿using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Knapcode.TorSharp;
using Spectre.Console;

class ConfigCommand : Command
{
    public ConfigCommand() : base("config", "Edits the full torrc configuration file.")
    {
        Handler = CommandHandler.Create(RunAsync);
    }

    async Task RunAsync()
    {
        var settings = new TorSharpSettings
        {
            ZippedToolsDirectory = Path.Combine(Tor.AppPath, "zip"),
            ExtractedToolsDirectory = Path.Combine(Tor.AppPath, "bin"),
        };

        var zipPath = await AnsiConsole.Status().StartAsync("Fetching Tor tools", async _ =>
        {
            var fetcher = new TorSharpToolFetcher(settings, new HttpClient());
            var updates = await fetcher.CheckForUpdatesAsync();
            await fetcher.FetchAsync(updates);
            return updates.Tor.DestinationPath;
        });

        var editor = FindCode() ??
            (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?
            "notepad.exe" : "nano");

        var zipName = Path.GetFileNameWithoutExtension(zipPath);
        if (zipName.EndsWith(".tar"))
            zipName = Path.GetFileNameWithoutExtension(zipName);

        var configPath = Path.Combine(
            settings.ExtractedToolsDirectory,
            zipName, "Data", "Tor", "torrc");

        var torProxy = new TorSharpProxy(settings);
        await torProxy.ConfigureAsync();

        if (File.Exists(configPath))
            Process.Start(editor, configPath);
    }

    static string? FindCode()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        var cmd = isWindows ? "where" : "which";
        var psi = new ProcessStartInfo(cmd)
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        var extensions = isWindows && Environment.GetEnvironmentVariable("PATHEXT") is { } v
                       ? v.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                       : Array.Empty<string>();

        // Tries to locate code with all the extensions (like .com, .bat, on Windows)
        foreach (var extension in extensions.DefaultIfEmpty(""))
            psi.ArgumentList.Add("code" + extension);

        using var process = Process.Start(psi);

        var path = process?.StandardOutput.ReadLine();

        if (!string.IsNullOrEmpty(path) &&
            path.IndexOfAny(Path.GetInvalidPathChars()) < 0 &&
            File.Exists(path))
            return path;

        return null;
    }
}
