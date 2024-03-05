# Reverse Proxy .NET (rpnet)

[➡️](https://github.com/Lifailon/ReverseProxyNET/blob/rsa/README.md) Choose language [➡️‍](https://github.com/Lifailon/ReverseProxyNET/blob/rsa/README_RU.md) Выберите язык

A cross-platform reverse proxy server based on .NET to provide access to all hosts on the network from one network interface to remote applications via HTTP and TCP protocols accessible through another network interface without unnecessary settings.

## 💁 For what?

This utility solves two problems:

Firstly, if you use a VPN server to access a specific URL resource (HTTP/HTTPS) on your machine, and want to provide access to it to other machines without setting up Proxy servers, which require additional configuration on the client side.

Secondly, if you are using a VPN server in point-to-point mode, and want to give the remote machine access to applications on other machines on your network running via the TCP protocol, for example, for the RDP, SSH or RTSP protocols. This also acts as an alternative to classic ssh tunneling, for example, through `Putty`.

There are many alternatives that provide the same functionality separately. For example, `ncat` on Windows (from [nmap](https://github.com/nmap/nmap)) and `socat` on Linux for TCP or [ReverseProxy](https://github.com/ilanyu/ReverseProxy) in Golang to redirect HTTP/HTTPS traffic. This functionality is implemented in one application with the ability to authorize 🔓 clients 🔑.

<!-- ## 🚀 Installation

## 📌 Using

```PowerShell

```

## 📑 Log
-->