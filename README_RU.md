<h2 align="center">
    Froxy
</h2>

<h4 align="center">
    <a href="README.md">English</a> | <strong>Русский</strong>
</h4>

Кроссплатформенная утилита командной строки для реализации классического прямого и обратного прокси сервера на базе **.NET**. Поддерживается переадресация любого `HTTPS` трафика (`CONNECT` запросы) для прямого проксирования и **TCP**, **UDP** или **HTTP/HTTPS** (поддерживаются `GET` и `POST` запросы) протоколы для реализации обратоного прокси сервера.

## 💁 Для чего?

Какие задачи решает обратный прокси сервер:

- **Безопаснось**, так как у клиента нет прямого доступа к целевой машине, на которой запущено приложение, например, веб-серверу.

- Если ваше Web-приложение или `API` сервер не поддерживают **авторизацию**, то вы можете воспользоваться [Base64](https://en.wikipedia.org/wiki/Base64) шифрованием, что обязует передавать в заголовке запроса авторизационные данные для всех клиентов, которые будут подключаться через Proxy. Если вы используете браузер, то будет предоставлена соответствующая форма для прохождения базовой авторизации.

- В отличии от классического Proxy вам не нужно указывать адрес прокси сервера на стороне клиента в настройках приложения или менять код и использовать внешние модули (например, [proxy-agents](https://github.com/TooTallNate/proxy-agents)) при каждом обращении через API клиент, а только изменить url на адрес proxy сервера.

- Если вы пользуетесь **VPN** сервисом для доступа к конкретному url ресурсу в Интернете через протоколы `HTTP` или `HTTPS` на своей машине, вы можете предоставить к нему доступ другим машинам в сети, без необходимости устанавливать и использовать VPN на других клиентах (например, на мобильных устройствах адрес Proxy сервера можно прописать в конфигурации Wi-Fi подключения).

- Предоставить прямой доступ к другим хостам во второй подсети, если вы используете **VPN** сервер в режиме точка-точка (например, [Radmin](https://www.radmin-vpn.com)).

- Возможность предоставить доступ внешнему клиенту в зоне **DMZ** (Demilitarized Zone) к приложениям, которое находится во внутренней сети, например, для протоколов `RTSP`, `SSH`, `RDP`, `Syslog` и т.п., то достаточно установить прокси с сетевым доступом в обе подсети и предоставить доступ через него только к выбранным приложениям на разных хостах.

- Может выступать альтернативой классического **ssh туннелирования**, как в `OpenSSH`, `Putty` и других.

Существует много альтернатив, которые предоставляют схожий функционал по отдельности. Например, **ncat** в Windows (из состава [nmap](https://github.com/nmap/nmap)), **socat** в Linux для `TCP` или [ReverseProxy](https://github.com/ilanyu/ReverseProxy) на Golang для перенаправления HTTP/HTTPS трафика. Весь вышеперечисленный функционал реализован в одной утилите.

## 🚀 Установка

### 💻 Windows

- [Скачайте и установите](https://dotnet.microsoft.com/en-us/download/dotnet/8.0/runtime) среду выполнения приложений `.NET` версии 8.0.

- [Загрузите](https://github.com/Lifailon/froxy/releases/latest) исполняемый файл портативной версии из GitHub репозитория.

### 🐧 Linux

- Установите среду выполнения приложений `.NET`:

```shell
sudo apt-get install -y dotnet-runtime-8.0
```

- Загрузите исполняемый файл `froxy` в директорию `/usr/local/bin/` и предоставьте права на выполнение:

```shell
sudo curl -s -L https://github.com/Lifailon/froxy/releases/download/0.3.0/froxy-0.3.0-linux-x64 -o /usr/local/bin/froxy
sudo chmod +x /usr/local/bin/froxy
```

💡 Протестирован в системе Ubuntu.

### 📌 Без установки зависимостей

Если вы не хотите устанавливать среду выполнения `.NET`, [загрузите](https://github.com/Lifailon/froxy/releases/latest) zip-архив с версией **self-contained**, которая уже содержит в себе все зависимости (доступно для обеих платформ).

### 🔨 Сборка

- Клонируйте репозиторий:

```shell
git clone https://github.com/Lifailon/froxy
cd froxy/source
```

- Соберите и запустите приложение:

```shell
dotnet build && dotnet run [parameters]
```

- Собрать приложение в один исполняемый файл:

Windows:

```
dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true
```

Linux:

```
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
```

- Собрать самодостаточное приложение (без необходимости в установке зависимостей платформы .NET на исполняемой системе):

Windows:

```
dotnet publish -r win-x64 -c Release --self-contained true
```

Linux:

```
dotnet publish -r linux-x64 -c Release --self-contained true
```

## 📑 Использование

Получите справку:

```shell
froxy --help

Forward and reverse proxy server base on .NET.

Parameters:
  -h, --help                       Get help.
  -v, --version                    Get version.
  -f, --forward <port>             Start forward (CONNECT method for HTTPS) proxy server on the selected port.
  -l, --local <port/address:port>  The interface address and port for TCP or only the port for UDP, through which proxy requests will pass.
  -r, --remote <address:port/url>  TCP/UDP or HTTP/HTTPS (GET and POST methods) address of the remote resource to which requests will be proxy.
  -u, --user <login>               User name for authorization (supported for forward and reverse HTTP/HTTPS protocols only).
  -p, --pass <password>            User password for authorization.

Examples:
  froxy --forward 8080
  froxy --local 5514 --remote 192.168.3.100:514
  froxy --local 127.0.0.1:8443 --remote 192.168.3.101:80
  froxy --local 127.0.0.1:8443 --remote https://example.com
  froxy --local *:8443 --remote https://example.com --user admin --pass admin
```

### 📭 Forward Proxy

Запуск прямого прокси на сервере:

```shell
froxy --forward 8080 --user admin --pass admin

Forward proxy server running on port 8080
[14:58:18] Error authentication: 192.168.3.101:47156
[14:58:20] Connect: 192.168.3.101:47170 => kinozal.tv:443
[14:58:20] Disconnect: 192.168.3.101:47170
[14:58:35] Connect: 192.168.3.101:47522 => rutracker.org:443
[14:58:36] Disconnect: 192.168.3.101:47522
```

Отправка запросов на клиенте через прокси:

```shell
curl -x http://192.168.3.100:8080 --proxy-user adm:adm https://kinozal.tv/browse.php?s=the+rookie
curl -x http://192.168.3.100:8080 --proxy-user admin:admin https://kinozal.tv/browse.php?s=the+rookie
curl -x http://192.168.3.100:8080 --proxy-user admin:admin https://rutracker.org/forum/index.php
```

Вы также можете запустить прокси в режиме демона (фоновый процесс) и передать вывод логов в файл:

```shell
froxy --forward 8080 --user admin --pass admin >> froxy.log & 
```

Синтаксис одинаков для обоих систем (Linux и Windows). Возможно запустить несколько экземпляров для обработки разных запросов в режиме реверсивного прокси.

Что бы прочитать вывод логов, используйте следующую конструкцию в Windows:

```PowerShell
Get-Content $(Get-Process froxy).Path.Replace("exe","log")
Forward proxy server running on port 8080
[16:59:53] Connect: 192.168.3.101:47818 => rutracker.org:443
[16:59:53] Disconnect: 192.168.3.101:47818
```

Для завершения всех фоновых процессов в Windows:

```PowerShell
Get-Process *froxy* | Stop-Process
```

Linux:

```shell
pkill froxy
```

### 🔌 TCP

В примере, принимает запросы на интерфейсе с ip-адресом `192.168.3.100` и порту `8443` для перенаправления на удаленный хост с ip-адресом `192.168.3.101`, где запущенно приложение на порту `80`.

```shell
froxy --local 192.168.3.100:8443 --remote 192.168.3.101:80

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

💡 Что бы прослушивать все сетевые интерфейсы, используйте символ `*` вместо локального ip-адреса (требуется запустить консоль с правами администратора).

Такой способ подойдет для обработки большенства протоколов, которые работают на базе TCP, в том числе поддерживается передача данных в теле запроса через HTTP.

Но, такой способ не подойдет для проксирования запросов к удаленным ресурсам через Интернет.

```PowerShell
$(Test-NetConnection 172.67.189.243 -Port 443).TcpTestSucceeded
True

froxy--local localhost:8443 --remote 172.67.189.243:443

TCP protocol is used
Error: An invalid IP address was specified.
```

Для решения такой задачи необходимо использовать проксирование через протокол HTTP или HTTPS.

### 🚧 SSH туннелирование через TCP

Пример ssh подключения через прокси сервер:

```shell
froxy --local 192.168.3.100:3131 --remote 192.168.3.101:2121
```

На стороне клиента подключаемся к хосту `192.168.3.101` через прокси с адресом `192.168.3.100`:

```shell
ssh lifailon@192.168.3.100 -p 3131
```

![img](image/tcp-ssh-tunnel.jpg)

### 📡 UDP

Пример переадресации запросов от клиента (конфигурация `rsyslog` клиента справа) по протоколу UDP (один символ `@` в конфигурации) к [серверу Visual Syslog](https://github.com/MaxBelkov/visualsyslog) слушающего запросы на порту `514` через прокси сервер, который слушает запросы на порту `5514`.

💡 При использовании протокола `UDP` локальный адрес не указывается.

```shell
froxy --local 5514 --remote 192.168.3.100:514
```

![](image/udp-syslog-relay.jpg)

### 🌐 HTTP & HTTPS

При использовании проксирования с использованием протоколов **HTTP или HTTPS**, необходимо передать url адрес, который начинается с наименования протокола `http://` или `https://`.

💡 Поддерживается передача данных через `GET` и `POST` запросы.

В примере, прокси сервер принимает запросы на интерфейсе с ip-адресом `192.168.3.100` на порту `8443` для перенаправления на удаленный url-ресурс [Кинозал](https://kinozal.tv). Подключение производится от клиента с ip-адресом `192.168.3.99` с использованием метода `GET`. Указаны все конечные точки, к которым обращается клиент для загрузки главной страницы.

```shell
froxy --local 192.168.3.100:8443 --remote https://kinozal.tv

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
froxy --local 192.168.3.100:8443 --remote https://kinozal.tv --userName proxy --password admin

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

---

## Другие проекты:

- 🧲 [Kinozal Bot](https://github.com/Lifailon/Kinozal-Bot) - Telegram бот, который позволяет автоматизировать процесс доставки контента до вашего телевизора, используя только телефон. Предоставляет удобный интерфейс для взаимодействия с торрент трекером [Кинозал](https://kinozal.tv) и базой данных [TMDB](https://www.themoviedb.org) для отслеживания даты выхода серий, сезонов и поиска актеров для каждой серии, а также возможность управлять торрент клиентом [qBittorrent](https://github.com/qbittorrent/qBittorrent) или [Transmission](https://github.com/transmission/transmission) на вашем компьютере, находясь удаленно от дома и из единого интерфейса.

- ✨ [TorAPI](https://github.com/Lifailon/TorAPI/blob/main/README_RU.md) - неофициальный `API` (backend) для торрент трекеров RuTracker, Kinozal, RuTor и NoNameClub. Используется для быстрого поиска раздач, а также получения torrent-файлов, магнитных ссылок и подробной информации о раздаче по названию фильма, сериала или идентификатору раздачи, а также предоставляет новостную RSS ленту для всех провайдеров.

- 🔎 [LibreKinopoisk](https://github.com/Lifailon/LibreKinopoisk) - расширение Google Chrome, которое добавляет кнопки на сайт Кинопоиск и предоставляет интерфейс **TorAPI** в стиле [Jackett](https://github.com/Jackett/Jackett) (без необходимости устанавливать серверную часть и использовать VPN) для быстрого поиска фильмов и сериалов в открытых источниках.

- ❤️ [WebTorrent Desktop api](https://github.com/Lifailon/webtorrent-desktop-api) - форк клиента [WebTorrent Desktop](https://github.com/webtorrent/webtorrent-desktop), в котором добавлен механизм удаленного управления через `REST API` на базе [Express Framework](https://github.com/expressjs/express).