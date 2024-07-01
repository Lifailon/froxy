using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program {
    static async Task Main(string[] args) {
        // Проверяем, что количество аргументов командной строки не меньше трех (например, dotnet run 8443 192.168.3.101 80)
        if (args.Length < 3) {
            Console.WriteLine("Usage: tcp <localPort> <remoteHost> <remotePort>");
            return;
        }
        // Парсим аргументы командной строки для получения локального порта, удаленного хоста и порта
        int localPort = int.Parse(args[0]);
        string remoteHost = args[1];
        int remotePort = int.Parse(args[2]);
        // создаем экземпляр TCP сокета, который будет прослушивать все IP-адреса на указанном локальном порту
        TcpListener listener = new TcpListener(IPAddress.Any, localPort);
        // Запускаем сокет для прослушивания входящих соединений
        listener.Start();
        Console.WriteLine($"Listening for incoming connections on port {localPort}...");
        while (true) {
            // Асинхронно ожидаем входящего соединения от TCP клиента на сокете
            var client = await listener.AcceptTcpClientAsync();
            // Обрабатываем входящее соединение в отдельной задаче
            _ = HandleTcpRequest(client, remoteHost, remotePort);
        }
    }
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
            // Закрываем соединения с клиентом и сервером
            client.Close();
            remoteClient?.Close();
        }
    }
}
