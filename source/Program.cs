using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;

class Program {
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
                Console.WriteLine("  -h, --help                  Show help.");
                Console.WriteLine("  -l, --local <address:port>  Address and port of the interface through which requests will be proxy.");
                Console.WriteLine("  -r, --remote <url>          HTTP/HTTPS address of the remote resource to which requests will be proxy.");
                Console.WriteLine("  -u, --userName <admin>      User name for authorization.");
                Console.WriteLine("  -p, --password <admin>      User password for authorization.\n");
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
            Console.WriteLine("Usage: rpnet --local <address:port> --remote <url>");
            return;
        }
        // Проверяем протокол для доступа к удаленному ресурсу
        // TCP
        if (!remote.StartsWith("http://") && !remote.StartsWith("https://")) {
            Console.WriteLine("TCP protocol is used");
            // Забираем порт
            string[] addr_split = local_addr.Split(':');
            string string_port = addr_split[1];
            int local_port = int.Parse(string_port);
            string[] addr_split_remote = remote.Split(':');
            string remote_addr = addr_split_remote[0];
            string string_port_remote = addr_split_remote[1];
            int remote_port = int.Parse(string_port_remote);
            // Создаем экземпляр TCP сокета для прослушивания подключений на всех адресах (Any)
            TcpListener listener = new TcpListener(IPAddress.Any, local_port);
            listener.Start();
            Console.WriteLine($"Listening on port {local_port} for forwarding to {remote}");
            while (true) {
                TcpClient client = await listener.AcceptTcpClientAsync();
                TcpClient remoteClient = new TcpClient();
                await remoteClient.ConnectAsync(remote_addr, remote_port);
                var clientStream = client.GetStream();
                var remoteStream = remoteClient.GetStream();
                Task clientReadTask = clientStream.CopyToAsync(remoteStream);
                Task remoteReadTask = remoteStream.CopyToAsync(clientStream);
                await Task.WhenAll(clientReadTask, remoteReadTask);
                client.Close();
                remoteClient.Close();
            }
        }
        // HTTP
        else {
            try {
                Console.WriteLine("HTTP protocol is used");
                // Создаем экземпляр HTTP сокета для прослушивания подключений
                var server = new HttpListener();
                // Передаем параметр локального url (на всех адресах: *) и запускаем
                server.Prefixes.Add(local);
                server.Start();
                Console.WriteLine($"Listening on {local_addr} for forwarding to {remote}");
                if (userName != null && password != null) {
                    Console.WriteLine("Authorization is used");
                }
                else {
                    Console.WriteLine("Not authorization is used");
                }
                // Бесконечный цикл для обработки запросов
                while (true) {
                    // Читаем контекст запроса асинхронно. Оператор await ожидает завершения асинхронной операции без блокировки основного потока выполнения
                    var context = await server.GetContextAsync();
                    // Метод HandleRequest выполняется параллельно, что позволяет серверу обрабатывать несколько запросов одновременно
                    _ = HandleRequest(context, remote, userName, password);
                }
            }
            // Возврощать ошибку в случае проблемы с запуском, например, нет доступа
            catch (Exception ex) {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    static async Task HandleRequest(HttpListenerContext context, string remote, string userName, string password) {
        var request = context.Request;
        var response = context.Response;
        // Выводим информацию запроса в консоль
        Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} {request.HttpMethod}: {request.RawUrl}");
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
                    // Возвращаем клиенту статус ошибки авторизации
                    response.StatusCode = 401;
                    response.StatusDescription = "Unauthorized";
                    response.OutputStream.Close();
                    return;
                }
            }
            else {
                // Если заголовок Authorization отсутствует, отправляем клиенту запрос авторизации
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
