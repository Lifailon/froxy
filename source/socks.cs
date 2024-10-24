using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class Socks5ProxyServer {
    private int ProxyPort;
    private bool UseAuthentication;
    private string ProxyUsername;
    private string ProxyPassword;

    // Конструктор для инициализации параметров
    public Socks5ProxyServer(int port, bool useAuthentication, string username, string password) {
        ProxyPort = port;
        UseAuthentication = useAuthentication;
        ProxyUsername = username;
        ProxyPassword = password;
    }

    // Запуск SOCKS5 прокси сервера
    static async Task ProxyRun(Socks5ProxyServer server) {
        TcpListener listener = new TcpListener(IPAddress.Any, server.ProxyPort);
        listener.Start();
        Console.WriteLine($"SOCKS5 proxy server running on port {server.ProxyPort}");
        while (true) {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = Task.Run(() => server.HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client) {
        IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
        using (client) {
            NetworkStream clientStream = client.GetStream();
            try {
                // Чтение приветствия клиента
                byte[] greeting = new byte[2];
                await clientStream.ReadAsync(greeting, 0, 2);
                if (greeting[0] != 0x05) {
                    // Если это не SOCKS5, завершаем соединение
                    return;
                }
                int methodsCount = greeting[1];
                byte[] methods = new byte[methodsCount];
                await clientStream.ReadAsync(methods, 0, methodsCount);
                // Проверяем доступные методы аутентификации
                if (UseAuthentication && Array.IndexOf(methods, (byte)0x02) == -1) {
                    // Если требуется аутентификация, но клиент её не поддерживает, отклоняем запрос
                    await clientStream.WriteAsync(new byte[] { 0x05, 0xFF }, 0, 2);
                    return;
                }
                // Отправляем метод аутентификации
                await clientStream.WriteAsync(new byte[] { 0x05, UseAuthentication ? (byte)0x02 : (byte)0x00 }, 0, 2);
                // Если включена аутентификация, выполняем проверку пользователя
                if (UseAuthentication) {
                    if (!await AuthenticateAsync(clientStream, clientEndPoint)) {
                        // Если аутентификация не удалась, завершаем соединение
                        return;
                    }
                }
                // Чтение запроса от клиента
                byte[] request = new byte[4];
                await clientStream.ReadAsync(request, 0, 4);
                if (request[0] != 0x05 || request[1] != 0x01) {
                    // Если это не SOCKS5 или запрос не CONNECT, отклоняем запрос
                    await clientStream.WriteAsync(new byte[] { 0x05, 0x07 }, 0, 2); // 0x07: Command not supported
                    return;
                }
                // Чтение адреса и порта для подключения
                byte[] address = new byte[256];
                int addressLength = 0;
                if (request[3] == 0x01) {
                    // IPv4
                    addressLength = 4;
                }
                else if (request[3] == 0x03) {
                    // Доменное имя
                    await clientStream.ReadAsync(address, 0, 1); // Длина доменного имени
                    addressLength = address[0];
                }
                else if (request[3] == 0x04) {
                    // IPv6
                    addressLength = 16;
                }
                await clientStream.ReadAsync(address, 0, addressLength);
                // Чтение порта
                byte[] portBytes = new byte[2];
                await clientStream.ReadAsync(portBytes, 0, 2);
                int port = (portBytes[0] << 8) + portBytes[1];
                string host = string.Empty;
                if (request[3] == 0x03) {
                    // Доменное имя
                    host = Encoding.ASCII.GetString(address, 0, addressLength);
                } else if (request[3] == 0x01) {
                    // IPv4
                    byte[] ipv4Address = new byte[4];
                    Array.Copy(address, 0, ipv4Address, 0, 4);
                    host = new IPAddress(ipv4Address).ToString();
                } else if (request[3] == 0x04) {
                    // IPv6
                    byte[] ipv6Address = new byte[16];
                    Array.Copy(address, 0, ipv6Address, 0, 16);
                    host = new IPAddress(ipv6Address).ToString();
                }
                // Логируем информацию о запросе подключения
                Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Request: {clientEndPoint.Address}:{clientEndPoint.Port} => {host}:{port}");
                // Подключаемся к целевому серверу
                TcpClient server = new TcpClient();
                try {
                    await server.ConnectAsync(host, port);
                    await clientStream.WriteAsync(new byte[] { 0x05, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }, 0, 10); // Ответ "успешное подключение"
                    // Передача данных между клиентом и сервером
                    using (NetworkStream serverStream = server.GetStream()) {
                        var clientToServerTask = RelayDataAsync(clientStream, serverStream);
                        var serverToClientTask = RelayDataAsync(serverStream, clientStream);
                        await Task.WhenAny(clientToServerTask, serverToClientTask);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error connecting to {host}:{port} - {ex.Message}");
                    await clientStream.WriteAsync(new byte[] { 0x05, 0x01 }, 0, 2); // 0x01: General failure
                }
                finally {
                    server.Close();
                }

                // Проверка на запрос UDP
                if (request[1] == 0x03) {
                    await HandleUdpAssociateAsync(clientStream, host, port);
                }
            }
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally {
                Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Disconnect: {clientEndPoint.Address}:{clientEndPoint.Port}");
            }
        }
    }

    // Метод обработки запроса на UDP
    private async Task HandleUdpAssociateAsync(NetworkStream clientStream, string host, int port) {
        // Создаем UDP-слушатель для обработки UDP-трафика
        UdpClient udpListener = new UdpClient();
        udpListener.Client.Bind(new IPEndPoint(IPAddress.Any, 0)); // Привязываем к любому доступному порту
        IPEndPoint localEndPoint = (IPEndPoint)udpListener.Client.LocalEndPoint;
        // Отправляем клиенту адрес и порт для UDP
        await clientStream.WriteAsync(new byte[] {
            0x05, 0x00, 0x00, 0x01,
            localEndPoint.Address.GetAddressBytes()[0], localEndPoint.Address.GetAddressBytes()[1],
            localEndPoint.Address.GetAddressBytes()[2], localEndPoint.Address.GetAddressBytes()[3],
            (byte)(localEndPoint.Port >> 8), (byte)(localEndPoint.Port & 0xFF)
        }, 0, 10);
        // Обработка UDP-пакетов
        while (true) {
            var udpResult = await udpListener.ReceiveAsync();
            // Console.WriteLine($"Received UDP packet from {udpResult.RemoteEndPoint}");
            // Пересылка полученного пакета на целевой адрес
            await udpListener.SendAsync(udpResult.Buffer, udpResult.Buffer.Length, host, port);
        }
    }

    // Метод аутентификации клиента
    private async Task<bool> AuthenticateAsync(NetworkStream stream, IPEndPoint clientEndPoint) {
        byte[] authRequest = new byte[2];
        await stream.ReadAsync(authRequest, 0, 1); // Читаем 1 байт
        if (authRequest[0] != 0x01) return false; // Только версия 1 поддерживается для username/password аутентификации
        // Login
        byte[] usernameLength = new byte[1];
        await stream.ReadAsync(usernameLength, 0, 1);
        byte[] usernameBytes = new byte[usernameLength[0]];
        await stream.ReadAsync(usernameBytes, 0, usernameBytes.Length);
        string username = Encoding.ASCII.GetString(usernameBytes);
        // Password
        byte[] passwordLength = new byte[1];
        await stream.ReadAsync(passwordLength, 0, 1);
        byte[] passwordBytes = new byte[passwordLength[0]];
        await stream.ReadAsync(passwordBytes, 0, passwordBytes.Length);
        string password = Encoding.ASCII.GetString(passwordBytes);
        if (username == ProxyUsername && password == ProxyPassword) {
            // Аутентификация успешна
            await stream.WriteAsync(new byte[] { 0x01, 0x00 }, 0, 2);
            return true;
        }
        else {
            // Неверный логин/пароль
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error authentication: {clientEndPoint.Address}:{clientEndPoint.Port}");
            Console.WriteLine($"Received username: {username}, password: {password}");
            await stream.WriteAsync(new byte[] { 0x01, 0x01 }, 0, 2); // 0x01: Authentication failed
            return false;
        }
    }

    // Метод для асинхронной пересылки данных между потоками ввода и вывода
    private async Task RelayDataAsync(NetworkStream input, NetworkStream output) {
        byte[] buffer = new byte[8192];
        int bytesRead;
        try {
            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await output.WriteAsync(buffer, 0, bytesRead);
                await output.FlushAsync();
            }
        }
        catch (IOException) {
            // Соединение закрыто
        }
    }

    public static async Task ProxyStart(int port, string username, string password) {
        bool useAuth = false;
        if (username != null && password != null) {
            useAuth = true;
        }
        Socks5ProxyServer proxyServer = new Socks5ProxyServer(port, useAuth, username, password);
        await ProxyRun(proxyServer);
    }
}