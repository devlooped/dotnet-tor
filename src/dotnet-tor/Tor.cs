using System;
using System.IO;
using System.Reflection;
using DotNetConfig;

static class Tor
{
    public static string AppPath { get; } = GetApplicationPath();

    public static string DataDir { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        "tor");

    // We rebuild on every request since other code can mutate the 
    // immutable structure and cause this property to become out of 
    // sync with the persisted state.
    public static Config Config => Config.Build(Path.Combine(DataDir, Config.FileName));

    // See GCM's Program.cs
    static string GetApplicationPath()
    {
        // Assembly::Location always returns an empty string if the application was published as a single file
#pragma warning disable IL3000
        bool isSingleFile = string.IsNullOrEmpty(Assembly.GetEntryAssembly()?.Location);
#pragma warning restore IL3000

        // Use "argv[0]" to get the full path to the entry executable - this is consistent across
        // .NET Framework and .NET >= 5 when published as a single file.
        string[] args = Environment.GetCommandLineArgs();
        string candidatePath = args[0];

        // If we have not been published as a single file on .NET 5 then we must strip the ".dll" file extension
        // to get the default AppHost/SuperHost name.
        if (!isSingleFile && Path.HasExtension(candidatePath))
        {
            return Path.ChangeExtension(candidatePath, null);
        }

        return candidatePath;
    }
}

