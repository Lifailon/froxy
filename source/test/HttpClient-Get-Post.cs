using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

class Program {
    // Создаем единственный экземпляр HttpClient, чтобы использовать его для всех запросов, который не будет пересоздаваться при каждом использовании метода
    private static readonly HttpClient client = new HttpClient();

    static async Task Main(string[] args) {
        var local = "http://*:8443/";
        var remote = "https://kinozal.tv";
        var server = new HttpListener(); // Создаем HttpListener для обработки входящих HTTP запросов
        server.Prefixes.Add(local); // Добавляем префикс, который будет прослушиваться
        server.Start(); // Запускаем HttpListener
        Console.WriteLine($"Listening on {local}, forwarding to {remote}"); // Логируем параметры запуска
        ServicePointManager.DefaultConnectionLimit = 1000; // Устанавливаем максимальное количество одновременных соединений
        while (true) {
            var context = await server.GetContextAsync(); // Ожидаем входящий запрос
            _ = Task.Run(() => HandleRequest(context, remote)); // Получение данных от клиента в отдельном асинхронном потоке с целью освобождения основного потока для обработки других клиентов
        }
    }

    static async Task HandleRequest(HttpListenerContext context, string remote) {
        var request = context.Request; // Получаем объект запроса от клиента
        var response = context.Response; // Получаем объект ответа, который будет отправлен клиенту
        Console.WriteLine($"{request.RemoteEndPoint} {request.HttpMethod} {request.Url} {request.ProtocolVersion}"); // Логируем информацию о запросе
        try {
            HttpResponseMessage remoteResponse; // Объявляем переменную для хранения ответа от удаленного сервера
            // Обрабатываем GET-запрос
            if (request.HttpMethod == "GET") {
                remoteResponse = await client.GetAsync(remote + request.RawUrl); // Перенаправляем GET-запрос на удаленный сервер (параллельно без блокировки основного потока)
            }
            // Обрабатываем POST-запрос
            else if (request.HttpMethod == "POST") {
                var requestData = await new StreamReader(request.InputStream).ReadToEndAsync(); // Читаем содержимое тела запроса от клиента
                var content = new StringContent(requestData, Encoding.UTF8, request.ContentType ?? "application/json"); // Создаем объект с содержимым запроса для отправки на удаленный сервер
                remoteResponse = await client.PostAsync(remote + request.RawUrl, content); // Перенаправляем POST-запрос на удаленный сервер
            }
            // Обрабатываем другие типы запросов (PUT, DELETE и т.д.)
            else {
                // Отвечаем (Method Not Allowed)
                response.StatusCode = 405;
                response.Close();
                return;
            }
            response.StatusCode = (int)remoteResponse.StatusCode; // Устанавливаем статус-код ответа для клиента, полученный от удаленного сервера
            response.ContentType = remoteResponse.Content.Headers.ContentType?.ToString() ?? "application/octet-stream"; // Устанавливаем тип содержимого ответа, полученный от удаленного сервера
            if (remoteResponse.IsSuccessStatusCode) { // Если запрос к удаленному серверу успешен
                var responseData = await remoteResponse.Content.ReadAsByteArrayAsync(); // Читаем содержимое ответа
                response.ContentLength64 = responseData.Length; // Устанавливаем длину содержимого
                response.OutputStream.Write(responseData, 0, responseData.Length); // Отправляем содержимое клиенту в выходной поток ответа
            }
            else {
                var errorMessage = $"Error occurred: {remoteResponse.ReasonPhrase}"; // Создаем сообщение об ошибке
                var errorData = Encoding.UTF8.GetBytes(errorMessage); // Кодируем сообщение об ошибке в байты
                response.ContentLength64 = errorData.Length; // Устанавливаем длину содержимого
                response.OutputStream.Write(errorData, 0, errorData.Length); // Отвечаем клиенту с содержимым ошибки
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
            response.OutputStream.Close(); // Закрываем выходной поток
            response.Close(); // Закрываем ответ
        }
    }
}
