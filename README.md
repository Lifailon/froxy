<h1 align="center">
  Reverse Proxy .NET
</h1>

<p align="center">
<a href="https://github.com/Lifailon/rpnet"><img title="GitHub Release"src="https://img.shields.io/github/v/release/Lifailon/rpnet?display_name=release&logo=GitHub&label=GitHub&link=https%3A%2F%2Fgithub.com%2FLifailon%2Frpnet%2F"></a>
<a href="https://www.nuget.org/packages/reverse.proxy.net"><img title="NuGet Version"src="https://img.shields.io/nuget/v/reverse.proxy.net?logo=NuGet&label=NuGet&link=https%3A%2F%2Fwww.nuget.org%2Fpackages%2Freverse.proxy.net"></a>
<a href="https://github.com/Lifailon/rpnet"><img title="GitHub top language"src="https://img.shields.io/github/languages/top/Lifailon/rpnet?logo=csharp&link=https%3A%2F%2Fgithub.com%2Fcsharp%2Fcsharp&color=green"></a>
<a href="https://github.com/Lifailon/rpnet/blob/rsa/LICENSE"><img title="GitHub License"src="https://img.shields.io/github/license/Lifailon/rpnet?link=https%3A%2F%2Fgithub.com%2FLifailon%2Frpnet%2Fblob%2Frsa%2FLICENSE&color=white"></a>
</p>

➡️ [Choose language](https://github.com/Lifailon/rpnet/blob/rsa/README.md) ➡️‍ [Выберите язык](https://github.com/Lifailon/rpnet/blob/rsa/README_RU.md)

A cross-platform command line utility for implementing a .NET-based reverse proxy. It is used to provide access to hosts from one network interface to remote applications via **HTTP/HTTPS, TCP or UDP** protocols accessible through another network interface without unnecessary settings and with authorization support.

## 💁 For what?

This utility solves several problems:

- Firstly, this is security, because when connecting, the client does not have direct access to the machine and convenience, where, unlike a classic Proxy server, you do not need to configure the client part and specify the proxy server address on the client side.

- Secondly, if you are using a VPN server in point-to-point mode or using network separation, where you need to provide access to a client in the DMZ (Demilitarized Zone) to an application listening for connections on a `TCP` or `UDP` port on the internal network, for example , for protocols `RTSP`, `SSH`, `RDP`. This tool can also act as an alternative to classic ssh tunneling (for example, through `OpenSSH` or `Putty`).

- Thirdly, if you use a VPN to access a specific URL resource via the `HTTP` or `HTTPS` protocols on your machine, and want to provide access to it to other machines on the network without using VPN or Proxy servers.

- Fourthly, if your Web application or REST API server does not support authorization, then you can use [Base64](https://en.wikipedia.org/wiki/Base64) encryption, which obliges you to transmit authorization data in the request header for everyone clients that will connect via Proxy. If you are using a browser, a form will be provided to complete basic authorization.

There are many alternatives that provide similar functionality individually. For example, `ncat` on Windows (from [nmap](https://github.com/nmap/nmap)) and `socat` on Linux for TCP or [ReverseProxy](https://github.com/ilanyu/ReverseProxy) in Golang to redirect HTTP/HTTPS traffic. All of the above functionality is implemented in one utility `rpnet`.

## 🚀 Installation

### 💻 Windows

- [Download and install](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) .NET application runtime version 8.0.

- [Download](https://github.com/Lifailon/rpnet/releases/latest) portable version executable from GitHub repository.

### 🐧 Linux

> 💡 Tested on Ubuntu 22.04.

- Install the .NET Application Runtime:

```shell
sudo apt-get install -y dotnet-runtime-8.0
```

- Download the `rpnet` executable file to the `/usr/local/bin/` directory and grant execution permissions:

```shell
sudo curl -s -L https://github.com/Lifailon/rpnet/releases/download/0.0.1-beta/rpnet-0.0.1-beta-linux-x64.1-beta -o /usr/local/bin/rpnet
sudo chmod +x /usr/local/bin/rpnet
```

### 🔨 Build

#### Clone the repository:

```
git clone https://github.com/Lifailon/rpnet.git
cd rpnet
```
#### Start the application:

```
dotnet run
```

#### Build the application without installing .NET system dependencies:

```
dotnet publish -r win-x64 -c Release --self-contained true
```

#### Build the application into one executable file:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```

## 📑 Usage

Get Help:

```shell
rpnet.exe --help

Reverse Proxy server base on .NET.

Parameters:
  -h, --help                       Show help.
  -l, --local <address:port>       Address and port of the interface through which requests will be proxy.
  -r, --remote <address:port/url>  HTTP/HTTPS address of the remote resource to which requests will be proxy.
  -u, --userName <admin>           User name for authorization.
  -p, --password <admin>           User password for authorization.

Examples:
  .\rpnet.exe -l 127.0.0.1:8443 -r 192.168.3.106:80
  .\rpnet.exe -l 127.0.0.1:8443 -r https://kinozal.tv
  .\rpnet.exe -l *:8443 -r https://kinozal.tv
  .\rpnet.exe -local *:8443 -remote https://kinozal.tv -userName proxy -password admin
```

### 📡 TCP

Accepts requests on the interface with IP address `192.168.3.100` and port `8443` to redirect to a remote host with IP address `192.168.3.106`, where the application is running on port `80`.

```shell
rpnet.exe --local 127.0.0.1:8443 --remote 192.168.3.100:80

TCP protocol is used
Listening on 127.0.0.1:8443 for forwarding to 192.168.3.101:80
[14:22:46] 127.0.0.1:50691: [::ffff:192.168.3.101]:80
[14:22:47] 127.0.0.1:50698: [::ffff:192.168.3.101]:80
[14:22:48] 127.0.0.1:50700: [::ffff:192.168.3.101]:80
[14:22:49] 127.0.0.1:50704: [::ffff:192.168.3.101]:80
[14:22:50] 127.0.0.1:50702: [::ffff:192.168.3.101]:80
[14:22:52] 127.0.0.1:50835: [::ffff:192.168.3.101]:80
[14:22:52] 127.0.0.1:50837: [::ffff:192.168.3.101]:80
[14:22:55] 127.0.0.1:50987: [::ffff:192.168.3.101]:80
[14:22:55] 127.0.0.1:50990: [::ffff:192.168.3.101]:80
[14:22:55] 127.0.0.1:50989: [::ffff:192.168.3.101]:80
[14:22:57] 127.0.0.1:51066: [::ffff:192.168.3.101]:80
```

> 💡 To listen to all network interfaces, use the `*` symbol instead of the local IP address (you need to run the console with administrator rights).

### 🔌 UDP

Example of redirecting requests from a client to a Syslog server via relay.

<h1 align="center">
<img src="screen/udp-syslog-relay.jpg" width="600"/></a>
</h1>

> 💡 When using the `UDP` protocol, no local address is specified.

### 🌐 HTTP

Accepts requests on the interface with IP address `192.168.3.100` and port `8443` to redirect to the remote url resource [Kinozal](https://kinozal.tv).

In the example, the connection is made from a client with the IP address `192.168.3.99` using the `GET` method. Lists all the endpoints that the client contacts to load the home page.

```shell
rpnet.exe --local 192.168.3.100:8443 --remote https://kinozal.tv

HTTP protocol is used
Listening on 192.168.3.100:8443 for forwarding to https://kinozal.tv
Not authorization is used
[12:06:15] 192.168.3.99 GET: /
[12:06:16] 192.168.3.99 GET: /pic/0_kinozal.tv.css?v=3.4
[12:06:16] 192.168.3.99 GET: /pic/jquery-3.6.3.min.js?v=1.1
[12:06:16] 192.168.3.99 GET: /pic/use.js?v=3.7
[12:06:16] 192.168.3.99 GET: /pic/logo3.gif
[12:06:16] 192.168.3.99 GET: /pic/emty.gif
[12:06:17] 192.168.3.99 GET: /pic/radio_ban.jpg
[12:06:17] 192.168.3.99 GET: /pic/knz_love.gif
[12:06:17] 192.168.3.99 GET: /pic/sbg.gif
[12:06:17] 192.168.3.99 GET: /pic/l_portiere.gif
[12:06:17] 192.168.3.99 GET: /pic/r_portiere.gif
[12:06:17] 192.168.3.99 GET: /pic/dw2.png       
[12:06:17] 192.168.3.99 GET: /pic/cat/11.gif
[12:06:17] 192.168.3.99 GET: /pic/cat/8.gif 
[12:06:18] 192.168.3.99 GET: /pic/cat/15.gif
[12:06:18] 192.168.3.99 GET: /pic/cat/17.gif
[12:06:18] 192.168.3.99 GET: /pic/status_icons.png
[12:06:19] 192.168.3.99 GET: /pic/srch_l.png
[12:06:19] 192.168.3.99 GET: /pic/srch_r2.png        
[12:06:19] 192.168.3.99 GET: /pic/flags_all.png?v=1  
[12:06:19] 192.168.3.99 GET: /i/poster/8/6/433686.jpg
[12:06:19] 192.168.3.99 GET: /i/poster/1/9/1357519.jpg
[12:06:19] 192.168.3.99 GET: /i/poster/2/1/975821.jpg
[12:06:19] 192.168.3.99 GET: /pic/cat/24.gif
[12:06:19] 192.168.3.99 GET: /i/poster/2/0/175220.jpg 
[12:06:19] 192.168.3.99 GET: /i/poster/2/0/1255220.jpg
[12:06:20] 192.168.3.99 GET: /pic/favicon.ico
```

### 🔓 Authorization

To use client-side authorization, you must fill in the appropriate parameters when starting the server. If the client transmits incorrect authorization data, this will be displayed in the log.

```shell
rpnet.exe --local 192.168.3.100:8443 --remote https://kinozal.tv --userName proxy --password admin

HTTP protocol is used
Listening on 192.168.3.100:8443 for forwarding to https://kinozal.tv
Authorization is used
[15:43:39] 192.168.3.100 GET: /
[15:43:39] 192.168.3.100 GET: Authorization error
[15:43:40] 192.168.3.100 GET: /
[15:43:40] 192.168.3.100 GET: Authorization form sent
[15:43:42] 192.168.3.100 GET: /
[15:43:42] 192.168.3.100 GET: Authorization error
[15:43:43] 192.168.3.100 GET: /
[15:43:43] 192.168.3.100 GET: Authorization form sent
[15:43:47] 192.168.3.100 GET: /
[15:43:48] 192.168.3.100 GET: /pic/0_kinozal.tv.css?v=3.4
[15:43:48] 192.168.3.100 GET: /pic/jquery-3.6.3.min.js?v=1.1
[15:43:48] 192.168.3.100 GET: /pic/use.js?v=3.7
[15:43:48] 192.168.3.100 GET: /pic/logo3.gif
[15:43:48] 192.168.3.100 GET: /pic/emty.gif
[15:43:49] 192.168.3.100 GET: /pic/radio_ban.jpg
[15:43:49] 192.168.3.100 GET: /pic/knz_love.gif
[15:43:49] 192.168.3.100 GET: /pic/sbg.gif
[15:43:49] 192.168.3.100 GET: /pic/l_portiere.gif
[15:43:49] 192.168.3.100 GET: /pic/r_portiere.gif
[15:43:49] 192.168.3.100 GET: /pic/dw2.png
[15:43:49] 192.168.3.100 GET: /pic/cat/8.gif
[15:43:50] 192.168.3.100 GET: /pic/cat/49.gif
[15:43:50] 192.168.3.100 GET: /pic/cat/6.gif
[15:43:50] 192.168.3.100 GET: /pic/cat/45.gif
[15:43:50] 192.168.3.100 GET: /pic/cat/13.gif
[15:43:50] 192.168.3.100 GET: /pic/status_icons.png
[15:43:50] 192.168.3.100 GET: /pic/srch_l.png
[15:43:50] 192.168.3.100 GET: /pic/srch_r2.png      
[15:43:50] 192.168.3.100 GET: /pic/flags_all.png?v=1
[15:43:51] 192.168.3.100 GET: /i/poster/0/8/1767008.jpg
[15:43:51] 192.168.3.100 GET: /i/poster/1/7/1995717.jpg
[15:43:51] 192.168.3.100 GET: /pic/cat/15.gif
[15:43:51] 192.168.3.100 GET: /pic/cat/38.gif
[15:43:51] 192.168.3.100 GET: /i/poster/2/0/1255220.jpg
[15:43:51] 192.168.3.100 GET: /i/poster/2/6/2024026.jpg
[15:43:51] 192.168.3.100 GET: /pic/cat/46.gif
[15:43:52] 192.168.3.100 GET: /pic/cat/24.gif
[15:43:52] 192.168.3.100 GET: /pic/favicon.ico
```