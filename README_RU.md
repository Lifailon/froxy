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

Кроссплатформенная утилита командной строки для реализации обратного прокси-сервер на базе .NET. Используется для предоставления доступа хостам с одного сетевого интерфейса к удаленным приложениям через протоколы **TCP, UDP или HTTP/HTTPS** доступных через другой сетевой интерфейс без лишних настроек и с поддержкой авторизации.

## 💁 Для чего?

Данная утилита решает несколько задач:

- Во первых, это безопасность, потому что при подключении у клиента нет прямого доступа к машине и удобство, где в отличии от клиссического Proxy сервера вам не нужно настраивать серверную часть и указывать адрес прокси сервера на стороне клиента в настройках приложения или при каждом обращении через REST клиент.

- Во вторых, если вы пользуетесь VPN сервером в режиме точка-точка или используется разделение сетей, где необходимо предоставить доступ клиенту в зоне DMZ (Demilitarized Zone) к приложению, слушающие соединения на `TCP` или `UDP` порту во внутрейнней сети, например, для протоколов `RTSP`, `SSH`, `RDP`, `syslog` и другие. Данный инструмент может также выступать альтернативой классического ssh туннелирования (например, через `OpenSSH` или `Putty`).

- В третьих, если вы используете VPN для доступа к конкретному url ресурсу через протоколы `HTTP` или `HTTPS` на своей машине, и хотите предоставить к нему доступ другим машинам в сети без использования VPN или Proxy серверов.

- В четвертых, если ваше Web-приложение или `REST API` сервер не поддерживают авторизацию, то вы можете воспользоваться [Base64](https://en.wikipedia.org/wiki/Base64) шифрованием, что обязует передавать в заголовке запроса авторизационные данные для всех клиентов, которые будут подключаться через Proxy. Если вы используете браузер, то будет предоставлена соответствующая форма для прохождения базовой авторизации.

Существует много альтернатив, которые предоставляют схожий функционал по отдельности. Например, `ncat` в Windows (из состава [nmap](https://github.com/nmap/nmap)) и `socat` в Linux для TCP или [ReverseProxy](https://github.com/ilanyu/ReverseProxy) на Golang для перенаправления HTTP/HTTPS трафика. Весь вышеперечисленный функционал реализован в одной утилите `rpnet`.

## 🚀 Установка

### 💻 Windows

- [Скачайте и установите](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) среду выполнения приложений .NET версии 8.0.

- [Загрузите](https://github.com/Lifailon/rpnet/releases/latest) исполняемый файл портативной версии из GitHub репозитория.

### 🐧 Linux

> Протестирован в системе Ubuntu 22.04.

- Установите среду выполнения приложений .NET:

```shell
sudo apt-get install -y dotnet-runtime-8.0
```

- Загрузите исполняемый файл `rpnet` в директорию `/usr/local/bin/` и предоставьте права на выполнение:

```shell
sudo curl -s -L https://github.com/Lifailon/rpnet/releases/download/0.0.1-beta/rpnet-0.0.1-beta-linux-x64.1-beta -o /usr/local/bin/rpnet
sudo chmod +x /usr/local/bin/rpnet
```

### 🔨 Сборка

#### Клонируйте репозиторий:

```
git clone https://github.com/Lifailon/rpnet.git
cd rpnet
```
#### Запустите приложение:

```
dotnet run [parameters]
```

#### Собрать приложение в один исполняемый файл:

- Windows:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```

- Linux:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```

#### Собрать самодостаточное приложение (без необходимости в установке зависимостей платформы .NET на исполняемой системе):

- Windows:

```
dotnet publish -r win-x64 -c Release --self-contained true
```

- Linux:

```
dotnet publish -r win-x64 -c Release --self-contained true
```

## 📑 Использование

Получите справку:

```shell
rpnet.exe --help

Reverse Proxy server base on .NET.


```

### 🔌 TCP

Принимает запросы на интерфейсе с ip-адресом `192.168.3.100` и порту `8443` для перенаправления на удаленный хост с ip-адресом `192.168.3.101`, где запущенно приложение на порту `80`.

```shell
rpnet.exe --local 192.168.3.100:8443 --remote 192.168.3.101:80

TCP protocol is used
Listening on 192.168.3.100:8443 for forwarding to 192.168.3.101:80
[15:52:01] 192.168.3.100:37865: [::ffff:192.168.3.101]:80
[15:52:04] 192.168.3.100:37160: [::ffff:192.168.3.101]:80
[15:52:21] 192.168.3.99:35036: [::ffff:192.168.3.101]:80
[15:52:22] 192.168.3.99:35037: [::ffff:192.168.3.101]:80
[15:52:25] 192.168.3.99:35035: [::ffff:192.168.3.101]:80
[15:52:30] 192.168.3.100:37162: [::ffff:192.168.3.101]:80
[15:52:34] 192.168.3.100:38970: [::ffff:192.168.3.101]:80
[15:52:35] 192.168.3.100:37999: [::ffff:192.168.3.101]:80
```

> 💡 Что бы прослушивать все сетевые интерфейсы, используйте символ `*` вместо локального ip-адреса (требуется запустить консоль с правами администратора).

Такой способ подойдет для обработки большенства протоколов, которые работают на базе TCP, в том числе поддерживается передача данных в теле запроса через HTTP.

Но такой способ не подойдет для проксирования запросов к удаленным ресурсам через Интернет.

```PowerShell
$(Test-NetConnection 172.67.189.243 -Port 443).TcpTestSucceeded
True

rpnet.exe--local localhost:8443 --remote 172.67.189.243:443

TCP protocol is used
Error: An invalid IP address was specified.
```

### 📡 UDP

Пример переадресации запросов от клиента (конфигурация `rsyslog`) к [серверу Syslog](https://github.com/MaxBelkov/visualsyslog) через прокси сервер.

💡 При использовании протокола `UDP` локальный адрес не указывается.

<h1 align="center">
    <img src="screen/udp-syslog-relay.jpg" width="700"/></a>
</h1>

### 🌐 HTTP

При использовании проксирования с использованием протокола **HTTP или HTTPS**, необходимо передать url адрес, который начинается с наименования протокола `http://` или `https://`.

💡 Поддерживается передача данных через `GET` и `POST` запросы.

В примере, прокси сервер принимает запросы на интерфейсе с ip-адресом `192.168.3.100` на порту `8443` для перенаправления на удаленный url-ресурс [Кинозал](https://kinozal.tv). Подключение производится от клиента с ip-адресом `192.168.3.99` с использованием метода `GET`. Указаны всех конечные точки, к которым обращается клиент для загрузки главной страницы.

```shell
rpnet.exe --local 192.168.3.100:8443 --remote https://kinozal.tv

HTTP protocol is used
Listening on 192.168.3.100:8443 for forwarding to https://kinozal.tv
Not authorization is used
[16:03:43] 192.168.3.99 GET: /
[16:03:43] 192.168.3.99 GET: /pic/0_kinozal.tv.css?v=3.4
[16:03:43] 192.168.3.99 GET: /pic/jquery-3.6.3.min.js?v=1.1
[16:03:43] 192.168.3.99 GET: /pic/use.js?v=3.7
[16:03:43] 192.168.3.99 GET: /pic/logo3.gif
[16:03:43] 192.168.3.99 GET: /pic/emty.gif
[16:03:44] 192.168.3.99 GET: /pic/radio_ban.jpg
[16:03:44] 192.168.3.99 GET: /pic/knz_love.gif
[16:03:44] 192.168.3.99 GET: /i/poster/1/1/1718811.jpg
[16:03:44] 192.168.3.99 GET: /pic/cat/45.gif
[16:03:44] 192.168.3.99 GET: /pic/l_portiere.gif
[16:03:44] 192.168.3.99 GET: /pic/sbg.gif
[16:03:44] 192.168.3.99 GET: /pic/r_portiere.gif
[16:03:44] 192.168.3.99 GET: /pic/cat/17.gif
[16:03:44] 192.168.3.99 GET: /pic/dw2.png
[16:03:44] 192.168.3.99 GET: /pic/status_icons.png
[16:03:44] 192.168.3.99 GET: /pic/srch_l.png
[16:03:44] 192.168.3.99 GET: /pic/srch_r2.png
[16:03:44] 192.168.3.99 GET: /pic/flags_all.png?v=1
[16:03:44] 192.168.3.99 GET: /i/poster/4/1/1772641.jpg
[16:03:44] 192.168.3.99 GET: /i/poster/3/7/541437.jpg
[16:03:44] 192.168.3.99 GET: /pic/cat/11.gif
[16:03:44] 192.168.3.99 GET: /pic/cat/20.gif
[16:03:45] 192.168.3.99 GET: /i/poster/8/0/467680.jpg
[16:03:45] 192.168.3.99 GET: /i/poster/2/1/2040221.jpg
[16:03:45] 192.168.3.99 GET: /pic/cat/8.gif
[16:03:45] 192.168.3.99 GET: /pic/favicon.ico
```

Авторизация на сайте через `POST` запрос:

```shell
[16:05:19] 192.168.3.99 POST: /takelogin.php
[16:05:20] 192.168.3.99 GET: /pic/0_kinozal.tv.css?v=3.4
[16:05:20] 192.168.3.99 GET: /pic/ava_m.jpg
[16:05:20] 192.168.3.99 GET: /pic/logo3.gif
[16:05:20] 192.168.3.99 GET: /pic/use.js?v=3.7
[16:05:20] 192.168.3.99 GET: /pic/jquery-3.6.3.min.js?v=1.1
[16:05:21] 192.168.3.99 GET: /pic/minus.gif
[16:05:21] 192.168.3.99 GET: /pic/plus.gif
[16:05:21] 192.168.3.99 GET: /pic/r5.gif
[16:05:21] 192.168.3.99 GET: /pic/bnr_pay_sm.jpg
[16:05:21] 192.168.3.99 GET: /pic/emty.gif
[16:05:21] 192.168.3.99 GET: /pic/srch_l.png
[16:05:21] 192.168.3.99 GET: /pic/r_portiere.gif
[16:05:21] 192.168.3.99 GET: /pic/sbg.gif
[16:05:21] 192.168.3.99 GET: /pic/bgmn.gif
[16:05:21] 192.168.3.99 GET: /pic/srch_r2.png
[16:05:21] 192.168.3.99 GET: /pic/l_portiere.gif
[16:05:21] 192.168.3.99 GET: /pic/flags_all.png?v=1
```

### 🔓 Авторизация

Для использования авторизации на стороне прокси сервера, необходимо заполнить соответствующие параметры при запуске. Если клиент передает неверные авторизационные данные, то это будет отображено в логе.

```shell
rpnet.exe --local 192.168.3.100:8443 --remote https://kinozal.tv --userName proxy --password admin

HTTP protocol is used
Listening on 192.168.3.100:8443 for forwarding to https://kinozal.tv
Authorization is used
[16:07:44] 192.168.3.100 GET: /
[16:07:44] 192.168.3.100 GET: Authorization form sent
[16:07:48] 192.168.3.100 GET: /
[16:07:48] 192.168.3.100 GET: Authorization error
[16:07:49] 192.168.3.100 GET: /
[16:07:49] 192.168.3.100 GET: Authorization form sent
[16:07:53] 192.168.3.100 GET: /
[16:07:54] 192.168.3.100 GET: /pic/0_kinozal.tv.css?v=3.4
[16:07:54] 192.168.3.100 GET: /pic/jquery-3.6.3.min.js?v=1.1
[16:07:54] 192.168.3.100 GET: /pic/use.js?v=3.7
[16:07:54] 192.168.3.100 GET: /pic/logo3.gif
[16:07:54] 192.168.3.100 GET: /pic/emty.gif
[16:07:55] 192.168.3.100 GET: /pic/radio_ban.jpg
[16:07:55] 192.168.3.100 GET: /pic/knz_love.gif
[16:07:55] 192.168.3.100 GET: /i/poster/1/1/1718811.jpg
[16:07:55] 192.168.3.100 GET: /pic/cat/45.gif
[16:07:55] 192.168.3.100 GET: /pic/dw2.png
[16:07:55] 192.168.3.100 GET: /pic/cat/17.gif
[16:07:55] 192.168.3.100 GET: /i/poster/4/1/1772641.jpg
[16:07:55] 192.168.3.100 GET: /i/poster/3/7/541437.jpg
[16:07:55] 192.168.3.100 GET: /pic/cat/11.gif
[16:07:55] 192.168.3.100 GET: /pic/cat/20.gif
[16:07:55] 192.168.3.100 GET: /i/poster/8/0/467680.jpg
[16:07:55] 192.168.3.100 GET: /i/poster/2/1/2040221.jpg
[16:07:55] 192.168.3.100 GET: /pic/cat/8.gif
[16:07:56] 192.168.3.100 GET: /pic/srch_l.png
[16:07:56] 192.168.3.100 GET: /pic/srch_r2.png
[16:07:56] 192.168.3.100 GET: /pic/l_portiere.gif
[16:07:56] 192.168.3.100 GET: /pic/r_portiere.gif
[16:07:56] 192.168.3.100 GET: /pic/sbg.gif
[16:07:56] 192.168.3.100 GET: /pic/status_icons.png
[16:07:56] 192.168.3.100 GET: /pic/flags_all.png?v=1
[16:07:57] 192.168.3.100 GET: /pic/favicon.ico
```