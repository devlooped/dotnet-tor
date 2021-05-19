using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using DotNetConfig;
using Knapcode.TorSharp;
using Spectre.Console;

class TorCommand : RootCommand
{
    const string ControlPassword = "7B002936-978D-46D3-9C96-7579F97F333E";
    static readonly ConfigSection config = Config.Build().GetSection("tor");

    string torPath;

    public TorCommand(string torPath) : base("Tor proxy service")
    {
        this.torPath = torPath;

        Add(new Option<int>(new[] { "--proxy", "-p" }, () => (int?)config.GetNumber("proxy") ?? 1337, "Proxy port"));
        Add(new Option<int>(new[] { "--socks", "-s" }, () => (int?)config.GetNumber("socks") ?? 1338, "Socks port"));
        Add(new Option<int>(new[] { "--control", "-c" }, () => (int?)config.GetNumber("control") ?? 1339, "Control port"));

        Add(new ConfigureCommand(torPath));

        Handler = CommandHandler.Create<int, int, int, CancellationToken>(RunAsync);
    }

    async Task RunAsync(int proxy, int socks, int control, CancellationToken cancellation)
    {
        var settings = new TorSharpSettings
        {
            ZippedToolsDirectory = Path.Combine(torPath, "zip"),
            ExtractedToolsDirectory = Path.Combine(torPath, "bin"),
            PrivoxySettings =
            {
                Port = proxy, 
            },
            TorSettings =
            {
                ControlPassword = ControlPassword,
                SocksPort = socks,
                ControlPort = control,
            },
        };

        await AnsiConsole.Status().StartAsync("Fetching Tor tools", 
            async _ => await new TorSharpToolFetcher(settings, new HttpClient()).FetchAsync());

        cancellation.ThrowIfCancellationRequested();
        var tor = new TorSharpProxy(settings);

        await tor.ConfigureAndStartAsync();
        cancellation.ThrowIfCancellationRequested();

        while (!cancellation.IsCancellationRequested)
            Thread.Sleep(100);
    }
}
