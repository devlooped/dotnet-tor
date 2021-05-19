![Icon](assets/img/icon-32.png) dotnet-tor
============

[![Version](https://img.shields.io/nuget/v/dotnet-tor.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-tor)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-tor.svg?color=darkmagenta)](https://www.nuget.org/packages/dotnet-tor)
[![License](https://img.shields.io/github/license/kzu/dotnet-tor.svg?color=blue)](https://github.com/kzu/dotnet-tor/blob/main/LICENSE)
[![CI Status](https://github.com/kzu/dotnet-tor/workflows/build/badge.svg?branch=main)](https://github.com/kzu/dotnet-tor/actions?query=branch%3Amain+workflow%3Abuild+)
[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.io/vpre/dotnet-tor/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.io/index.json)

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
  config  Edits the full torrc configuration file.
```

The program will automatically check for updates once a day and recommend updating 
if there is a new version available.