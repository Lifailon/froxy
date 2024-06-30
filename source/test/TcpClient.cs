using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program {
    static async Task Main(string[] args) {
        // Проверяем, что количество аргументов командной строки не меньше трех
        if (args.Length < 3) {
            Console.WriteLine("Usage: tcp <localPort> <remoteHost> <remotePort>");
            return;
        }
        // Парсим аргументы командной строки для получения локального порта, удаленного хоста и его порта
        int localPort = int.Parse(args[0]);
        string remoteHost = args[1];
        int remotePort = int.Parse(args[2]);
        // Создаем TCP-сервер, который будет прослушивать все IP-адреса на указанном локальном порту
        TcpListener listener = new TcpListener(IPAddress.Any, localPort);
        listener.Start(); // Запускаем сервер для прослушивания входящих соединений
        Console.WriteLine($"Listening for incoming connections on port {localPort}...");
        while (true) {
            // Асинхронно ждем входящего соединения от клиента
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Incoming connection accepted.");
            // Создаем TCP-клиента для подключения к удаленному серверу
            TcpClient remoteClient = new TcpClient();
            // Подключаемся к удаленному серверу
            await remoteClient.ConnectAsync(remoteHost, remotePort);
            // Получаем сетевые потоки для передачи данных между клиентом и удаленным сервером
            var clientStream = client.GetStream();
            var remoteStream = remoteClient.GetStream();
            // Копируем данные из клиентского потока в поток удаленного сервера и наоборот
            Task clientReadTask = clientStream.CopyToAsync(remoteStream);
            Task remoteReadTask = remoteStream.CopyToAsync(clientStream);
            // Ждем завершения обеих задач копирования данных
            await Task.WhenAll(clientReadTask, remoteReadTask);
            // Закрываем соединения с клиентом и удаленным сервером
            client.Close();
            remoteClient.Close();
        }
    }
}