using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetConfig;
using Humanizer;
using Knapcode.TorSharp;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Spectre.Console;

#if !CI
AnsiConsole.MarkupLine($"[lime]dotnet tor [/]{string.Join(' ', args)}");
#endif

var config = Config.Build().GetSection("tor");
// Check for updates once a day.
var check = config.GetDateTime("checked");
// Only check for CI builds
if (ThisAssembly.Project.CI.Equals("true", StringComparison.OrdinalIgnoreCase) &&
    (check == null || (DateTime.Now - check) > 24.Hours()))
{
    var update = await GetUpdateAsync();
    config.SetDateTime("checked", DateTime.Now);
    if (update != null)
        AnsiConsole.MarkupLine($"[yellow]New version v{update.Identity.Version} from {(DateTimeOffset.Now - (update.Published ?? DateTimeOffset.Now)).Humanize()} ago is available.[/] Update with: [lime]dotnet tool update -g dotnet-echo[/]");
}

#if !CI
AnsiConsole.MarkupLine($"[yellow]AppPath: {Tor.AppPath}[/]");
#endif

await new TorCommand().InvokeAsync(args);

static Task<IPackageSearchMetadata> GetUpdateAsync() => AnsiConsole.Status().StartAsync("Checking for updates", async context =>
{
    var providers = Repository.Provider.GetCoreV3();
    var source = new PackageSource("https://api.nuget.org/v3/index.json");
    var repo = new SourceRepository(source, providers);
    var resource = await repo.GetResourceAsync<PackageMetadataResource>();
    var metadata = await resource.GetMetadataAsync(ThisAssembly.Project.PackageId, false, false, new SourceCacheContext(), NuGet.Common.NullLogger.Instance, CancellationToken.None);

    var update = metadata
        //.Select(x => x.Identity)
        .Where(x => x.Identity.Version > new NuGetVersion(ThisAssembly.Project.Version))
        .OrderByDescending(x => x.Identity.Version)
        //.Select(x => x.Version)
        .FirstOrDefault();

    return update;
});