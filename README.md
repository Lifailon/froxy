# Reverse Proxy .NET (rpnet)

Cross-platform .NET based reverse proxy server to provide access to url resources and tcp applications to all hosts on the network from one network interface accessible through another network interface without unnecessary configuration.

## For what?

This utility solves two problems:

First, if you are using a VPN server to access a specific url resource (HTTP/HTTPS) on your machine, and want to provide access to it to other machines without configuring Proxy servers that require additional configuration on the client side.

Second, if you are using a point-to-point VPN server, and you want to give a remote machine access to applications on other machines on your network (an alternative to ssh tunneling).

There are many alternatives that provide the same functionality individually. For example, `ncat` in Windows (from [nmap](https://github.com/nmap/nmap)) and `socat` in Linux for TCP or [ReverseProxy](https://github.com/ilanyu/ReverseProxy) in Golang for redirecting HTTP/HTTPS traffic. This functionality is implemented in one application with authorization 🔓.

<!-- ## 🚀 Installation

## 📌 Using

```PowerShell

```

## 📑 Log
-->