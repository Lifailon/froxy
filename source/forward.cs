using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

class ProxyServer {
    private int ProxyPort;
    private bool UseAuthentication;
    private string ProxyUsername;
    private string ProxyPassword;

    // Конструктор для инициализации параметров
    public ProxyServer(int port, bool useAuthentication, string username, string password) {
        ProxyPort = port;
        UseAuthentication = useAuthentication;
        ProxyUsername = username;
        ProxyPassword = password;
    }

    static async Task ProxyRun(ProxyServer server) {
        // Создаем и запускаем TcpListener для ожидания входящих подключений на указанном порту
        TcpListener listener = new TcpListener(IPAddress.Any, server.ProxyPort);
        listener.Start();
        Console.WriteLine($"Forward proxy server running on port {server.ProxyPort}");
        // Основной цикл прослушивания подключений клиентов
        while (true) {
            // Принимаем новое подключение от клиента
            TcpClient client = await listener.AcceptTcpClientAsync();
            // Обрабатываем подключение клиента асинхронно
            _ = Task.Run(() => server.HandleClientAsync(client));
        }
    }

    private async Task HandleClientAsync(TcpClient client) {
        using (client) {
            // Получаем поток данных клиента
            NetworkStream clientStream = client.GetStream();
            // Получаем информацию о клиенте
            IPEndPoint clientEndPoint = client.Client.RemoteEndPoint as IPEndPoint;
            // Создаем StreamReader и StreamWriter для чтения и записи данных в клиентский поток
            using (StreamReader reader = new StreamReader(clientStream))
            using (StreamWriter writer = new StreamWriter(clientStream) { AutoFlush = false }) {
                // Читаем первую строку запроса (CONNECT ...)
                string requestLine = await reader.ReadLineAsync();
                // Проверяем, что запрос не пустой
                if (string.IsNullOrEmpty(requestLine)) {
                    return; // Если запрос пустой, прекращаем обработку
                }
                // Читаем заголовки запроса, включая "Proxy-Authorization"
                string authorizationHeader = null;
                string line = null;
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync())) {
                    if (line.StartsWith("Proxy-Authorization:", StringComparison.OrdinalIgnoreCase)) {
                        // Извлекаем значение заголовка
                        authorizationHeader = line.Substring("Proxy-Authorization:".Length).Trim();
                    }
                }
                // Проверка авторизации, если включена
                if (UseAuthentication && !IsAuthorized(authorizationHeader)) {
                    await writer.WriteAsync("HTTP/1.1 407 Proxy Authentication Required\r\n");
                    await writer.WriteAsync("Proxy-Authenticate: Basic realm=\"Proxy\"\r\n\r\n");
                    await writer.FlushAsync();
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error authentication: {clientEndPoint.Address}:{clientEndPoint.Port}");
                    return;
                }
                // Разделяем строку запроса на части для проверки типа запроса и целевого хоста
                string[] requestParts = requestLine.Split(' ');
                // Основной код для обработки CONNECT запросов
                if (requestParts.Length < 2 || requestParts[0].ToUpper() != "CONNECT") {
                    // Если запрос не CONNECT, отправляем ошибку 400 и завершаем обработку
                    await writer.WriteAsync("HTTP/1.1 400 Bad Request\r\n\r\n");
                    await writer.FlushAsync();
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error request method: {clientEndPoint.Address}:{clientEndPoint.Port}");
                    return;
                }
                // Извлекаем хост и порт из строки запроса
                string[] hostParts = requestParts[1].Split(':');
                string host = hostParts[0];
                int port = hostParts.Length > 1 ? int.Parse(hostParts[1]) : 443;
                // Для подключения к целевому серверу
                TcpClient server = null;
                try {
                    // Подключаемся к целевому серверу по указанному хосту и порту
                    server = new TcpClient();
                    await server.ConnectAsync(host, port);
                    // Отправляем клиенту подтверждение о подключении
                    await writer.WriteAsync("HTTP/1.1 200 Connection Established\r\n\r\n");
                    await writer.FlushAsync();
                    // Логируем информацию о подключении
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Connect: {clientEndPoint.Address}:{clientEndPoint.Port} => {host}:{port}");
                    // Получаем сетевой поток для связи с сервером
                    using (NetworkStream serverStream = server.GetStream()) {
                        // Запускаем асинхронное пересылание данных в обоих направлениях (клиент - сервер)
                        var clientToServerTask = RelayDataAsync(clientStream, serverStream);
                        var serverToClientTask = RelayDataAsync(serverStream, clientStream);
                        // Ожидаем завершения любой из задач передачи данных
                        await Task.WhenAny(clientToServerTask, serverToClientTask);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Connection error: {ex.Message}");
                    // Отправляем клиенту сообщение об ошибке 502, если подключение к серверу не удалось
                    await writer.WriteAsync("HTTP/1.1 502 Bad Gateway\r\n\r\n");
                    await writer.FlushAsync();
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error connection server: {clientEndPoint.Address}:{clientEndPoint.Port}");
                }
                finally {
                    Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Disconnect: {clientEndPoint.Address}:{clientEndPoint.Port}");
                    // Закрываем соединение с сервером
                    server?.Close();
                }
            }
        }
    }

    // Метод проверки авторизации
    private bool IsAuthorized(string authorizationHeader) {
        if (string.IsNullOrEmpty(authorizationHeader))
            return false;
        string expectedHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ProxyUsername}:{ProxyPassword}"));
        return authorizationHeader.Equals(expectedHeader, StringComparison.OrdinalIgnoreCase);
    }

    // Метод для асинхронной пересылки данных между потоками ввода и вывода
    private async Task RelayDataAsync(NetworkStream input, NetworkStream output) {
        // Буфер передачи данных
        byte[] buffer = new byte[8192];
        int bytesRead;
        try {
            // Читаем данные из входного потока и пишем в выходной до тех пор, пока данные поступают
            while ((bytesRead = await input.ReadAsync(buffer, 0, buffer.Length)) > 0) {
                await output.WriteAsync(buffer, 0, bytesRead);
                // Принудительно отправляем данные в выходной поток
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
        ProxyServer proxyServer = new ProxyServer(port, useAuth, username, password);
        await ProxyRun(proxyServer);
    }
}