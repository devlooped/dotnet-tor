![Icon](https://raw.githubusercontent.com/devlooped/dotnet-tor/main/assets/img/icon-32.png) dotnet-tor
============

[![Version](https://img.shields.io/nuget/v/dotnet-tor.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-tor) [![Downloads](https://img.shields.io/nuget/dt/dotnet-tor.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-tor) [![License](https://img.shields.io/github/license/devlooped/dotnet-tor.svg?color=blue)](https://github.com/devlooped/dotnet-tor/blob/main/LICENSE)

Installing or updating (same command can be used for both):

```
dotnet tool update -g dotnet-tor
```

<!-- #content -->
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

<!-- #content -->

## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/dotnet-tor/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json) [![Build](https://github.com/devlooped/dotnet-tor/workflows/build/badge.svg?branch=main)](https://github.com/devlooped/dotnet-tor/actions)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`


<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Uno Platform](https://avatars.githubusercontent.com/u/52228309?v=4&s=39 "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://avatars.githubusercontent.com/u/3200210?v=4&s=39 "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://avatars.githubusercontent.com/u/61711586?u=8f653dfcb641e8c18cc5f78692ebc6bb3a0c92be&v=4&s=39 "Michael Hagedorn")](https://github.com/Eule02)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=739e110e62a75870c981640447efa5eb2cb3bc8f&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Seika Logiciel](https://avatars.githubusercontent.com/u/2564602?v=4&s=39 "Seika Logiciel")](https://github.com/SeikaLogiciel)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
