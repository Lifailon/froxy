using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

class rpNet {
    // Создаем единственный экземпляр HttpClient, чтобы использовать его для всех запросов, который не будет пересоздаваться при каждом использовании метода
    private static readonly HttpClient client = new HttpClient();
    static async Task Main(string[] args) {
        // Объявляем переменные для параметров
        string userName = null;
        string password = null;
        string local_addr = null;
        string local = null;
        string remote = null;
        // Анализируем аргументы командной строки
        for (int i = 0; i < args.Length; i += 2) {
            // Проверяем содержимое аргументов на соответствие
            if (args[i].Contains("-h")) {
                Console.WriteLine("Reverse Proxy server base on .NET.\n");
                Console.WriteLine("Parameters:");
                Console.WriteLine("  -h, --help                       Show help.");
                Console.WriteLine("  -l, --local <port/address:port>  Address and port of the interface or only the port (for udp) through which proxy requests will pass.");
                Console.WriteLine("  -r, --remote <address:port/url>  TCP/UDP or HTTP/HTTPS address of the remote resource to which requests will be proxy.");
                Console.WriteLine("  -u, --userName <LOGIN>           User name for authorization (HTTP only).");
                Console.WriteLine("  -p, --password <PASSWORD>        User password for authorization.\n");
                Console.WriteLine("Examples:");
                Console.WriteLine(@"  rpnet.exe --local 127.0.0.1:8443 --remote 192.168.3.101:80");
                Console.WriteLine(@"  rpnet.exe --local 5514 --remote 192.168.3.100:514");
                Console.WriteLine(@"  rpnet.exe --local 127.0.0.1:8443 --remote https://kinozal.tv");
                Console.WriteLine(@"  rpnet.exe --local *:8443 --remote https://kinozal.tv --userName proxy --password admin");
                Console.WriteLine();
                return;
            }
            // Следующий номер индекса (1/3) меньше общего количества аргументов (4) и этот аргумент не содержит ключ
            if ((args[i] == "-l" || args[i] == "--local") && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
                local_addr = args[i + 1];
                local = $"http://{local_addr}/";
            }
            else if ((args[i] == "-r" || args[i] == "--remote") && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
                remote = args[i + 1];
            }
            else if ((args[i] == "-u" || args[i] == "--userName") && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
                userName = args[i + 1];
            }
            else if ((args[i] == "-p" || args[i] == "--password") && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
                password = args[i + 1];
            }
        }
        // Проверяем, что были переданы все нужные аргументы для запуска
        if (local == null || remote == null) {
            Console.WriteLine("Usage: rpnet --local <port/address:port> --remote <address:port/url>");
            return;
        }
        // Проверяем протокол доступа к удаленному ресурсу
        // TCP или UDP
        if (!remote.StartsWith("http://") && !remote.StartsWith("https://")) {
            // UDP
            if (!local_addr.Contains('.') && !local_addr.Contains(':')) {
                Console.WriteLine("UDP protocol is used");
                try {
                    int local_port = int.Parse(local_addr);
                    string[] addr_split_remote = remote.Split(':');
                    string remote_addr = addr_split_remote[0];
                    string string_port_remote = addr_split_remote[1];
                    int remote_port = int.Parse(string_port_remote);
                    // Создаем экземпляр UDP сокета
                    UdpClient udpClient = new UdpClient(local_port);
                    IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(remote_addr), remote_port);
                    Console.WriteLine($"Listening on {local_port} port for forwarding to {remote}");
                    while (true) {
                        var receivedResult = await udpClient.ReceiveAsync();
                        byte[] receivedData = receivedResult.Buffer;
                        await udpClient.SendAsync(receivedData, receivedData.Length, remoteEndPoint);
                    }
                }
                catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
            // TCP
            else {
                Console.WriteLine("TCP protocol is used");
                try {
                    // Забираем адрес и порт из параметров локального и удаленного адреса
                    string[] addr_split = local_addr.Split(':');
                    string local_string_addr = addr_split[0];
                    string string_port = addr_split[1];
                    int local_port = int.Parse(string_port);
                    string[] addr_split_remote = remote.Split(':');
                    string remote_addr = addr_split_remote[0];
                    string string_port_remote = addr_split_remote[1];
                    int remote_port = int.Parse(string_port_remote);                
                    // Проверяем переданный адрес для прослушивания и создаем экземпляр TCP сокета
                    TcpListener listener;
                    if (local_string_addr == "*") {
                        listener = new TcpListener(IPAddress.Any, local_port);
                    }
                    else {
                        IPAddress local_ip_addr = IPAddress.Parse(local_string_addr);
                        listener = new TcpListener(local_ip_addr, local_port);
                    }
                    // Запускаем сокет для прослушивания входящих соединений
                    listener.Start();
                    Console.WriteLine($"Listening on {local_string_addr}:{local_port} for forwarding to {remote}");
                    while (true) {
                        // Асинхронно ожидаем входящего соединения от TCP клиента на сокете
                        var client = await listener.AcceptTcpClientAsync();
                        // Обрабатываем входящее соединение в отдельной задаче
                        _ = HandleTcpRequest(client, remote_addr, remote_port);
                    }
                }
                // Возврощать ошибку в случае проблемы с запуском, например, неправильно передан локальный адрес (требуемый адрес для своего контекста неверен)
                catch (Exception ex) {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }
        // HTTP
        else {
            Console.WriteLine("HTTP protocol is used");
            try {
                // Создаем HttpListener для обработки входящих HTTP запросов
                var server = new HttpListener();
                // Передаем параметр локального url (на всех адресах: *) и запускаем
                server.Prefixes.Add(local);
                server.Start();
                Console.WriteLine($"Listening on {local_addr} for forwarding to {remote}");
                // Устанавливаем максимальное количество одновременных соединений
                ServicePointManager.DefaultConnectionLimit = 1000;
                // Проверяем использование базовой авторизации на прокси сервере
                if (userName != null && password != null) {
                    Console.WriteLine("Authorization is used");
                }
                else {
                    Console.WriteLine("Not authorization is used");
                }
                // Бесконечный цикл для обработки запросов
                while (true) {
                    // var context = await server.GetContextAsync();
                    // _ = HandleWebRequest(context, remote, userName, password);
                    // Ожидаем входящий запрос. Оператор await ожидает завершения асинхронной операции без блокировки основного потока
                    var context = await server.GetContextAsync();
                    // Метод HandleHttpRequest выполняется параллельно в отдельных задачах, что позволяет серверу обрабатывать несколько запросов одновременно
                    _ = Task.Run(() => HandleHttpRequest(context, remote, userName, password));
                }
            }
            // Возврощать ошибку в случае проблемы с запуском, например, нет доступа
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    // Обработка TCP запросов
    static async Task HandleTcpRequest(TcpClient client, string remoteHost, int remotePort) {
        TcpClient remoteClient = null;
        try {
            // Создаем новый TCP клиент для подключения к удаленному хосту
            remoteClient = new TcpClient();
            // Асинхронно подключаемся к удаленному хосту с помощью клиента
            await remoteClient.ConnectAsync(remoteHost, remotePort);
            // Получаем сетевые потоки с данными от клиента и сервера
            var clientStream = client.GetStream();
            var remoteStream = remoteClient.GetStream();
            // В асинхронных задачах копируем данные из клиентского потока в поток удаленного сервера и наоборот
            Task clientToRemoteTask = clientStream.CopyToAsync(remoteStream);
            Task remoteToClientTask = remoteStream.CopyToAsync(clientStream);
            // Ожидаем завершения всех асинхронных задач копирования данных
            await Task.WhenAny(clientToRemoteTask, remoteToClientTask);
        } catch (Exception ex) {
            Console.WriteLine($"Error: {ex.Message}");
        } finally {
            // Логируем запрос
            Console.WriteLine(
                $"[{DateTime.Now.ToString("HH:mm:ss")}] {client.Client.RemoteEndPoint}: {remoteClient.Client.RemoteEndPoint}"
            );
            // Закрываем соединения с клиентом и сервером
            client.Close();
            remoteClient?.Close();
        }
    }

    // Обработка HTTP GET и POST запросов через HttpClient
    static async Task HandleHttpRequest(HttpListenerContext context, string remote, string userName, string password) {
        var request = context.Request; // Получаем объект запроса от клиента
        var response = context.Response; // Получаем объект ответа, который будет отправлен клиенту
        Console.WriteLine(
            $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + // ({request.UserAgent})
            $"{request.HttpMethod}: {request.RawUrl}"
        );
        // Используем проверку аутентификации, если переданы соответствующие параметры
        if (userName != null && password != null) {
            // Проверяем, содержит ли заголовок запроса информацию о базовой аутентификации
            if (request.Headers["Authorization"] != null) {
                // Получаем строку аутентификации из заголовка
                string authHeader = request.Headers["Authorization"];
                // Извлекаем базовую часть и декодируем ее
                string encodedUsernamePassword = authHeader.Split(' ')[1];
                byte[] decodedBytes = Convert.FromBase64String(encodedUsernamePassword);
                string usernamePassword = Encoding.UTF8.GetString(decodedBytes);
                // Разделяем имя пользователя и пароль
                string[] parts = usernamePassword.Split(':');
                string userName_remote = parts[0];
                string password_remote = parts[1];
                // Проверка данных
                if (userName_remote == userName && password_remote == password) {
                    // Продолжаем обработку запроса
                }
                else {
                    // Возвращаем клиенту статус ошибки авторизации и логируем
                    Console.WriteLine(
                        $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + // ({request.UserAgent})
                        $"{request.HttpMethod}: Authorization error"
                    );
                    response.StatusCode = 401;
                    response.StatusDescription = "Unauthorized";
                    response.OutputStream.Close();
                    return;
                }
            }
            else {
                // Если заголовок Authorization отсутствует, отправляем клиенту запрос авторизации
                Console.WriteLine(
                    $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + // ({request.UserAgent})
                    $"{request.HttpMethod}: Authorization form sent"
                );
                response.StatusCode = 401;
                response.Headers.Add("WWW-Authenticate", "Basic realm=\"Secure Area\"");
                response.OutputStream.Close();
                return;
            }
        }
        try {
            // Объявляем переменную для хранения ответа от удаленного сервера
            HttpResponseMessage remoteResponse;
            // Обрабатываем GET-запрос
            if (request.HttpMethod == "GET") {
                // Перенаправляем GET-запрос на удаленный сервер (параллельно без блокировки основного потока)
                remoteResponse = await client.GetAsync(remote + request.RawUrl);
            }
            // Обрабатываем POST-запрос
            else if (request.HttpMethod == "POST") {
                // Читаем содержимое тела запроса от клиента
                var requestData = await new StreamReader(request.InputStream).ReadToEndAsync();
                // Создаем объект с содержимым запроса для отправки на удаленный сервер
                var content = new StringContent(requestData, Encoding.UTF8, request.ContentType ?? "application/json");
                // Перенаправляем POST-запрос на удаленный сервер
                remoteResponse = await client.PostAsync(remote + request.RawUrl, content);
            }
            // Обрабатываем другие типы запросов (PUT, DELETE и т.д.)
            else {
                // Отвечаем (Method Not Allowed)
                response.StatusCode = 405;
                response.Close();
                return;
            }
            // Устанавливаем статус-код ответа для клиента, полученный от удаленного сервера
            response.StatusCode = (int)remoteResponse.StatusCode;
            // Устанавливаем тип содержимого ответа, полученный от удаленного сервера
            response.ContentType = remoteResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
            // Если запрос к удаленному серверу успешен
            if (remoteResponse.IsSuccessStatusCode) {
                // Читаем содержимое ответа
                var responseData = await remoteResponse.Content.ReadAsByteArrayAsync();
                // Устанавливаем длину содержимого
                response.ContentLength64 = responseData.Length;
                // Отправляем содержимое клиенту в выходной поток ответа
                response.OutputStream.Write(responseData, 0, responseData.Length);
            }
            else {
                // Создаем сообщение об ошибке
                var errorMessage = $"Error occurred: {remoteResponse.ReasonPhrase}";
                // Кодируем сообщение об ошибке в байты
                var errorData = Encoding.UTF8.GetBytes(errorMessage);
                // Устанавливаем длину содержимого
                response.ContentLength64 = errorData.Length;
                // Отвечаем клиенту с содержимым ошибки
                response.OutputStream.Write(errorData, 0, errorData.Length);
            }
        }
        // Обрабатываем исключения (аналогично ответу, в случае ошибке запроса к удаленному серверу)
        catch (HttpRequestException ex) {
            response.StatusCode = 500;
            var errorMessage = $"Error occurred: {ex.Message}";
            var errorData = Encoding.UTF8.GetBytes(errorMessage);
            response.ContentLength64 = errorData.Length;
            response.OutputStream.Write(errorData, 0, errorData.Length);
        }
        finally {
            // Закрываем выходной поток
            response.OutputStream.Close();
            // Закрываем ответ
            response.Close();
        }
    }

    // Обработка HTTP GET запросов через WebClient
    static async Task HandleWebRequest(HttpListenerContext context, string remote, string userName, string password) {
        var request = context.Request;
        var response = context.Response;
        // Выводим информацию запросов в консоль (лог)
        Console.WriteLine(
            $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + 
            $"{request.HttpMethod}: {request.RawUrl}"
        );
        // Используем проверку аутентификации, если переданы соответствующие параметры
        if (userName != null && password != null) {
            if (request.Headers["Authorization"] != null) {
                string authHeader = request.Headers["Authorization"];
                string encodedUsernamePassword = authHeader.Split(' ')[1];
                byte[] decodedBytes = Convert.FromBase64String(encodedUsernamePassword);
                string usernamePassword = Encoding.UTF8.GetString(decodedBytes);
                string[] parts = usernamePassword.Split(':');
                string userName_remote = parts[0];
                string password_remote = parts[1];
                if (userName_remote == userName && password_remote == password) {
                    // Продолжаем обработку запроса
                }
                else {
                    Console.WriteLine(
                        $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + 
                        $"{request.HttpMethod}: Authorization error"
                    );
                    response.StatusCode = 401;
                    response.StatusDescription = "Unauthorized";
                    response.OutputStream.Close();
                    return;
                }
            }
            else {
                Console.WriteLine(
                    $"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} " + 
                    $"{request.HttpMethod}: Authorization form sent"
                );
                response.StatusCode = 401;
                response.Headers.Add("WWW-Authenticate", "Basic realm=\"Secure Area\"");
                response.OutputStream.Close();
                return;
            }
        }
        // Создаем WebClient для выполнения запроса к удаленному серверу
        #pragma warning disable SYSLIB0014 // Тип или член устарел
        using (var client = new WebClient()) {
            // Добавляем заголовок User-Agent из запроса клиента
            client.Headers.Add("User-Agent", request.UserAgent);
            try {
                // Выполняем запрос к удаленному серверу асинхронно
                var requestData = await client.DownloadDataTaskAsync(remote + request.RawUrl);
                // Отправляем полученные данные обратно клиенту
                response.OutputStream.Write(requestData, 0, requestData.Length);
                // Устанавливаем статус успешного выполнения
                response.StatusCode = 200;
            }
            catch (WebException ex) {
                if (ex.Response != null) {
                    // Если возникла ошибка, отправляем соответствующий статус и сообщение об ошибке обратно клиенту
                    var errorResponse = (HttpWebResponse)ex.Response;
                    response.StatusCode = (int)errorResponse.StatusCode;
                    using (var errorStream = errorResponse.GetResponseStream()) {
                        errorStream.CopyTo(response.OutputStream);
                    }
                }
                else {
                    // Если возникла другая ошибка, отправляем статус 500 и сообщение об ошибке
                    response.StatusCode = 500;
                    var errorMessage = $"Error occurred: {ex.Message}";
                    var errorData = System.Text.Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorData, 0, errorData.Length);
                }
            }
        }
        #pragma warning restore SYSLIB0014 // Тип или член устарел
        // Логируем код ответа, только если есть ошибка
        if (response.StatusCode != 200) {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] Error: {response.StatusCode}");
        }
        // Закрываем поток ответа
        response.Close();
    }
}
