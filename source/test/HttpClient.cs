using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

class Program {
    private static readonly HttpClient client = new HttpClient(); // Создать один экземпляр для всего приложения, которое не будет пересоздаваться при каждом использовании метода
    static async Task Main(string[] args) {
        var local = "http://*:8443/";
        var remote = "https://kinozal.tv";
        var server = new HttpListener();
        server.Prefixes.Add(local);
        server.Start();
        Console.WriteLine($"Listening on {local}, forwarding to {remote}");
        ServicePointManager.DefaultConnectionLimit = 1000; // Максимальное количество одновременных соединений
        while (true) {
            var context = await server.GetContextAsync(); // Обработка только GET-запросов
            _ = Task.Run(() => HandleRequest(context, remote)); // Получение данных от клиента в отдельном асинхронном потоке с целью освобождения основного потока для обработки других задач
        }
    }
    
    static async Task HandleRequest(HttpListenerContext context, string remote) {
        var request = context.Request;
        var response = context.Response;
        Console.WriteLine($"{request.RemoteEndPoint} {request.HttpMethod} {request.Url} {request.ProtocolVersion}");
        try {
            HttpResponseMessage remoteResponse = await client.GetAsync(remote + request.RawUrl); // GetAsync для обработки несколько запросов параллельно без блокировки основного потока
            response.StatusCode = (int)remoteResponse.StatusCode;
            
            if (remoteResponse.IsSuccessStatusCode) {
                var responseData = await remoteResponse.Content.ReadAsByteArrayAsync();
                response.OutputStream.Write(responseData, 0, responseData.Length);
            } else {
                response.StatusCode = (int)remoteResponse.StatusCode;
                var errorMessage = $"Error occurred: {remoteResponse.ReasonPhrase}";
                var errorData = System.Text.Encoding.UTF8.GetBytes(errorMessage);
                response.OutputStream.Write(errorData, 0, errorData.Length);
            }
        }
        catch (HttpRequestException ex) {
            response.StatusCode = 500;
            var errorMessage = $"Error occurred: {ex.Message}";
            var errorData = System.Text.Encoding.UTF8.GetBytes(errorMessage);
            response.OutputStream.Write(errorData, 0, errorData.Length);
        }
        finally {
            response.Close();
        }
    }
}
