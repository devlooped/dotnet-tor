using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Threading;

class AddCommand : Command
{
    public AddCommand() : base("add", "Adds a service to register on the Tor network")
    {
        Add(new Argument<string>("name", "Name of the service to register"));
        Add(new Argument<string>("service", "Address and port of the local service being registered, such as 127.0.0.1:8080"));
        Add(new Option<int?>(new[] { "--port", "-p" }, "Optional port on the Tor network to listen on, if different than the service port"));

        Handler = CommandHandler.Create<string, int?, string, CancellationToken>(Run);
    }

    static void Run(string name, int? port, string service, CancellationToken cancellation)
    {
        if (!Uri.TryCreate("http://" + service, UriKind.Absolute, out var uri))
            throw new ArgumentException("Service specified is not valid. It should be an IP:PORT combination: " + service);

        Tor.Config.GetSection("tor", name)
           .SetNumber("port", port ?? uri.Port)
           .SetString("service", service);
    }
}
