using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DotNetConfig;
using Knapcode.TorSharp;
using Spectre.Console;

class TorCommand : RootCommand
{
    const string ControlPassword = "7B002936-978D-46D3-9C96-7579F97F333E";
    const string ConfigDelimiter = "#! dotnet-tor";


    public TorCommand() : base("Tor proxy service")
    {
        var config = Tor.Config.GetSection("tor");

        Add(new Option<int>(new[] { "--proxy", "-p" }, () => (int?)config.GetNumber("proxy") ?? 1337, "Proxy port"));
        Add(new Option<int>(new[] { "--socks", "-s" }, () => (int?)config.GetNumber("socks") ?? 1338, "Socks port"));
        Add(new Option<int>(new[] { "--control", "-c" }, () => (int?)config.GetNumber("control") ?? 1339, "Control port"));

        Add(new AddCommand());
        Add(new ConfigCommand());

        Handler = CommandHandler.Create<int, int, int, CancellationToken>(RunAsync);
    }

    async Task RunAsync(int proxy, int socks, int control, CancellationToken cancellation)
    {
        var settings = new TorSharpSettings
        {
            ZippedToolsDirectory = Path.Combine(Tor.AppPath, "zip"),
            ExtractedToolsDirectory = Path.Combine(Tor.AppPath, "bin"),
            PrivoxySettings =
            {
                Port = proxy,
            },
            TorSettings =
            {
                ControlPassword = ControlPassword,
                SocksPort = socks,
                ControlPort = control,
                //DataDirectory = Tor.DataDir,
            },
        };

        var zipPath = await AnsiConsole.Status().StartAsync("Fetching Tor tools", async _ =>
        {
            var fetcher = new TorSharpToolFetcher(settings, new HttpClient());
            var updates = await fetcher.CheckForUpdatesAsync();
            await fetcher.FetchAsync(updates);
            return updates.Tor.DestinationPath;
        });

        cancellation.ThrowIfCancellationRequested();
        var tor = new TorSharpProxy(settings);

        await tor.ConfigureAsync();
        cancellation.ThrowIfCancellationRequested();

        var zipName = Path.GetFileNameWithoutExtension(zipPath);
        if (zipName.EndsWith(".tar"))
            zipName = Path.GetFileNameWithoutExtension(zipName);

        var configPath = Path.Combine(
            settings.ExtractedToolsDirectory,
            zipName, "Data", "Tor", "torrc");

        if (!File.Exists(configPath))
            throw new ArgumentException($"Tor configuration file not found at expected location {configPath}");

        var torConfig = Tor.Config;

        // Clear dotnet-tor configuration from previous runs.
        var allLines = (await File.ReadAllLinesAsync(configPath, cancellation)).ToList();
        var begin = allLines.IndexOf(ConfigDelimiter);

        if (begin != -1)
        {
            var end = allLines.LastIndexOf(ConfigDelimiter);
            allLines.RemoveRange(begin, end != -1 && end != begin ? end - begin + 1 : allLines.Count - begin);
            await File.WriteAllLinesAsync(configPath, allLines.Where(line => !string.IsNullOrEmpty(line)), cancellation);
        }

        var services = torConfig
            .Where(x => x.Section == "tor" && x.Subsection != null)
            .Select(x => x.Subsection!)
            .Distinct().ToList();

        if (services.Count > 0)
        {
            await File.AppendAllLinesAsync(configPath, new[] { ConfigDelimiter }, cancellation);
            foreach (var service in services)
            {
                var config = torConfig.GetSection("tor", service);
                var serviceDir = Path.Combine(Tor.DataDir, service);
                await File.AppendAllLinesAsync(configPath, new[]
                {
                    "HiddenServiceDir " + serviceDir,
                    $"HiddenServicePort {config.GetNumber("port")} {config.GetString("service")}"
                }, cancellation);
            }
            await File.AppendAllLinesAsync(configPath, new[] { ConfigDelimiter }, cancellation);
        }

        await tor.StartAsync();
        cancellation.ThrowIfCancellationRequested();

        // Save successfully run args as settings
        Tor.Config.GetSection("tor")
           .SetNumber("proxy", proxy)
           .SetNumber("socks", socks)
           .SetNumber("control", control);

        foreach (var service in services)
        {
            var hostName = Path.Combine(Tor.DataDir, service, "hostname");
            if (File.Exists(hostName))
            {
                var config = torConfig.GetSection("tor", service);
                var onion = await File.ReadAllTextAsync(hostName, cancellation);
                AnsiConsole.MarkupLine($"[yellow]Service {service} ({config.GetString("service")}):[/] [lime]{onion.Trim()}[/]");
            }
        }

        while (!cancellation.IsCancellationRequested)
            Thread.Sleep(100);
    }
}
