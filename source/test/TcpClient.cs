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
        // Создаем TCP-сокет, который будет прослушивать все IP-адреса на указанном локальном порту
        TcpListener listener = new TcpListener(IPAddress.Any, localPort);
        listener.Start(); // Запускаем сервер для прослушивания входящих соединений
        Console.WriteLine($"Listening for incoming connections on port {localPort}...");
        while (true) {
            // Асинхронно ожидаем входящего соединения от TCP клиента на сокете
            TcpClient client = await listener.AcceptTcpClientAsync();
            // Создаем новый TCP клиент для подключения к удаленному хосту
            TcpClient remoteClient = new TcpClient();
            // Асинхронно подключаемся к удаленному хосту с помощью клиента
            await remoteClient.ConnectAsync(remoteHost, remotePort);
            // Получаем сетевые потоки с данными от клиента и сервера
            var clientStream = client.GetStream();
            var remoteStream = remoteClient.GetStream();
            // В асинхронных задачах копируем данные из клиентского потока в поток удаленного сервера и наоборот
            // clientStream - это поток данных, которые клиент отправляет на ваш сервер и которые ваш сервер отправляет клиенту
            // remoteStream - это поток данных, которые ваш сервер отправляет удаленному серверу и которые удаленный сервер отправляет вашему серверу
            Task clientReadTask = clientStream.CopyToAsync(remoteStream);
            Task remoteReadTask = remoteStream.CopyToAsync(clientStream);
            // Ожидаем завершения всех асинхронных задач копирования данных
            await Task.WhenAll(clientReadTask, remoteReadTask);
            // Закрываем соединения с клиентом и удаленным сервером
            client.Close();
            remoteClient.Close();
        }
    }
}