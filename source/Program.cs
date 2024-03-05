using System;
using System.Net;
using System.Threading.Tasks;
using System.Text;

class Program {
    static async Task Main(string[] args) {
        string userName = null;
        string password = null;
        string local_addr = null;
        string local = null;
        string remote = "https://kinozal.tv"; // null;
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
        // Создаем экземпляр сервера для прослушивания подключений
        var server = new HttpListener();
        // Передаем параметр url и запускаем
        server.Prefixes.Add(local);
        server.Start();
        Console.WriteLine($"Listening on {local_addr} and forwarding to {remote}");
        if (userName != null && password != null) {
            Console.WriteLine("Authorization is used");
        }
        else {
            Console.WriteLine("Not authorization is used");
        }
        // Бесконечный цикл для обработки запросов
        while (true) {
            // Читаем контекст запроса асинхронно. Оператор await ожидает завершения асинхронной операции без блокировки основного потока выполнения.
            var context = await server.GetContextAsync();
            // Метод HandleRequest выполняется параллельно, что позволяет серверу обрабатывать несколько запросов одновременно.
            _ = HandleRequest(context, remote, userName, password); // Игнорировать возвращаемое значение метода, так как оно не используется дальше в коде.
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
