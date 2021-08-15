![Icon](https://raw.githubusercontent.com/devlooped/dotnet-tor/main/assets/img/icon-32.png) dotnet-tor
============

[![Version](https://img.shields.io/nuget/v/dotnet-tor.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-tor) [![Downloads](https://img.shields.io/nuget/dt/dotnet-tor.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-tor) [![License](https://img.shields.io/github/license/devlooped/dotnet-tor.svg?color=blue)](https://github.com/devlooped/dotnet-tor/blob/main/LICENSE)

Installing or updating (same command can be used for both):

```
dotnet tool update -g dotnet-tor
```

Usage:

```
> dotnet tor -?
tor
  Tor proxy service

Usage:
  tor [options] [command]

Options:
  -p, --proxy <proxy>      Proxy port [default: 1337]
  -s, --socks <socks>      Socks port [default: 1338]
  -c, --control <control>  Control port [default: 1339]
  --version                Show version information
  -?, -h, --help           Show help and usage information

Commands:
  add <name> <service>  Adds a service to register on the Tor network
  config                Edits the full torrc configuration file.
```

The program will automatically check for updates once a day and recommend updating 
if there is a new version available.

Configured services are persisted in the global [dotnet-config](https://dotnetconfig.org) file at `%userprofile%\tor\.netconfig`, and on first run (after configuration), their `.onion` address and keys will be available in a sub-directory alongside the `.netconfig`. This allows the tool to self-update while preserving all configurations and services.

### Exposing local HTTP APIs via Tor

After installation, you might want to expose an .NET Core HTTP service from local port 7071 over the Tor network on port 80. 
You could configure the service with:

```
> dotnet tor add api 127.0.0.1:7071 -p 80
```

Then start the Tor proxy normally with:

```
> dotnet tor
```

There will now be a `%userprofile%\tor\.netconfig\api\hostname` file with the .onion address for the service, like `2gzyxa5ihm7nsggfxnu52rck2vv4rvmdlkiu3zzui5du4xyclen53wid.onion`. You can now reach web API endpoints natively via a .NET 6 client with:

```csharp
using System;
using System.Net;
using System.Net.Http;

var http = new HttpClient(new HttpClientHandler
{
    Proxy = new WebProxy("socks5://127.0.0.1:1338")
});

var response = await http.GetAsync("http://2gzyxa5ihm7nsggfxnu52rck2vv4rvmdlkiu3zzui5du4xyclen53wid.onion/[endpoint]"));
```

The client can just use another `dotnet-tor` proxy running locally with default configuration values and things will Just Work™ and 
properly reach the destination service running anywhere in the world :).

You can even expose the local HTTPS endpoint instead. In this case, the client would need to perform custom validation 
(or entirely bypass it) of the certificate, but otherwise, things work as-is too.

Service-side:

```
> dotnet tor add api 127.0.0.1:5001 -p 443
```

Note that since we're exposing the service over the default port for SSL, we don't need to specify the port in the client:

```csharp
var http = new HttpClient(new HttpClientHandler
{
    ClientCertificateOptions = ClientCertificateOption.Manual,
    ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
    Proxy = new WebProxy("socks5://127.0.0.1:1338"),
});

var response = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://kbu3mvegpytu4gewdgvjae7zhrzszmetmr5jdlwk5ct5pfzlbaqbdqqd.onion")
{
    Content = new StringContent("Hello World!")
});

response.EnsureSuccessStatusCode();
```

You can play around with a trivial echo service by installing the [dotnet-echo](https://nuget.org/packages/dotnet-echo) tool 
and exposing it over the Tor network.


## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/dotnet-tor/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json) [![Build](https://github.com/devlooped/dotnet-tor/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/dotnet-tor/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.io/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`



## Sponsors

[![sponsored](https://raw.githubusercontent.com/devlooped/oss/main/assets/images/sponsors.svg)](https://github.com/sponsors/devlooped) [![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/byclarius.svg)](https://github.com/clarius)[![clarius](https://raw.githubusercontent.com/clarius/branding/main/logo/logo.svg)](https://github.com/clarius)

*[get mentioned here too](https://github.com/sponsors/devlooped)!*
