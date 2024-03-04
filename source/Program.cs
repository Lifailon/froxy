using System;
using System.Net;
using System.Threading.Tasks;
using System.Text;

class Program {
    static async Task Main(string[] args) {
        // -UserName
        // -Password
        string port = "1111"; // null;
        string bind = $"http://*:{port}/"; // null;
        string remote = "https://kinozal.tv"; // null;
        // Анализируем аргументы командной строки
        // for (int i = 0; i < args.Length; i += 2) {
        //     // Проверяем содержимое аргументов на соответствие
        //     // Следующий номер индекса (1/3) меньше общего количества аргументов (4) и этот аргумент не содержит ключ
        //     if (args[i] == "-local" && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
        //         port = args[i + 1];
        //         bind = $"http://*:{args[i + 1]}/";
        //     }
        //     else if (args[i] == "-remote" && i + 1 < args.Length && !args[i + 1].StartsWith("-")) {
        //         remote = args[i + 1];
        //     }
        // }
        // Проверяем, что были переданы все необходимые аргументы
        if (bind == null || remote == null) {
            Console.WriteLine("Usage: Program -local <port> -remote <url>");
            return;
        }
        // Создаем экземпляр сервера для прослушивания подключений
        var server = new HttpListener();
        // Передаем параметр url и запускаем
        server.Prefixes.Add(bind);
        server.Start();
        Console.WriteLine($"Listening on {port} and forwarding to {remote}");
        // Бесконечный цикл для обработки запросов
        while (true) {
            // Читаем контекст запроса асинхронно. Оператор await ожидает завершения асинхронной операции без блокировки основного потока выполнения.
            var context = await server.GetContextAsync();
            // Метод HandleRequest выполняется параллельно, что позволяет серверу обрабатывать несколько запросов одновременно.
            _ = HandleRequest(context, remote); // Игнорировать возвращаемое значение метода, так как оно не используется дальше в коде.
        }
    }

    static async Task HandleRequest(HttpListenerContext context, string remote) {
        var request = context.Request;
        var response = context.Response;
        // Выводим информацию запроса в консоль
        Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] {request.RemoteEndPoint.Address} {request.HttpMethod}: {request.RawUrl}");
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
            string username = parts[0];
            string password = parts[1];
            // Проверка данных
            if (username == "proxy" && password == "net") {
                // Продолжаем обработку запроса
            } else {
                // Возвращаем клиенту статус ошибки авторизации
                response.StatusCode = 401;
                response.StatusDescription = "Unauthorized";
                response.OutputStream.Close();
                return;
            }
        } else {
            // Если заголовок Authorization отсутствует, отправляем клиенту запрос авторизации
            response.StatusCode = 401;
            response.Headers.Add("WWW-Authenticate", "Basic realm=\"Secure Area\"");
            response.OutputStream.Close();
            return;
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
        // Логируем код ответа с ошибкой
        if (response.StatusCode != 200) {
            Console.WriteLine($"[{DateTime.Now.ToString("HH:mm:ss")}] ERROR: {response.StatusCode}");
        }
        // Закрываем поток ответа
        response.Close();
    }
}
