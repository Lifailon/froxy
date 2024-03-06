# Reverse Proxy .NET (rpnet)

> 💡 At the functional testing stage

[➡️](https://github.com/Lifailon/ReverseProxyNET/blob/rsa/README.md) Choose language [➡️‍](https://github.com/Lifailon/ReverseProxyNET/blob/rsa/README_RU.md) Выберите язык

A command line utility for implementing a .NET based reverse proxy. It is used to provide access to all hosts on the network from one network interface to remote applications via HTTP and TCP protocols accessible through another network interface without unnecessary settings and with authorization support.

## 💁 For what?

This utility solves several problems:

- Firstly, if you use a VPN server to access a specific URL resource (HTTP/HTTPS) on your machine, and want to provide access to it to other machines without using Proxy servers, which require additional configuration on the client side.

- Secondly, if you are using a VPN server in point-to-point mode, and want to give the remote machine access to applications on other machines on your network running via the TCP protocol, for example, for the RDP, SSH or RTSP protocols. This also acts as an alternative to classic ssh tunneling, for example, via `OpenSSH` or `Putty`.

- 🔓 Thirdly, if your Web application or REST API server does not support authorization, then you can add [Base64](https://en.wikipedia.org/wiki/Base64) encryption for all clients, which obliges you to transmit authorization data in the request header. If you are using a browser, you will be provided with a corresponding login form 🔑.

There are many alternatives that provide similar functionality individually. For example, `ncat` on Windows (from [nmap](https://github.com/nmap/nmap)) and `socat` on Linux for TCP or [ReverseProxy](https://github.com/ilanyu/ReverseProxy) in Golang to redirect HTTP/HTTPS traffic.

<!-- ## 🚀 Installation

## 📌 Using

```PowerShell

```

## 📑 Log
-->