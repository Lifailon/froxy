using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program {
    static async Task Main(string[] args) {
        if (args.Length < 3) {
            Console.WriteLine("Usage: tcp <localPort> <remoteHost> <remotePort>");
            return;
        }
        int localPort = int.Parse(args[0]);
        string remoteHost = args[1];
        int remotePort = int.Parse(args[2]);
        TcpListener listener = new TcpListener(IPAddress.Any, localPort);
        listener.Start();
        Console.WriteLine($"Listening for incoming connections on port {localPort}...");
        while (true) {
            TcpClient client = await listener.AcceptTcpClientAsync();
            Console.WriteLine("Incoming connection accepted.");
            TcpClient remoteClient = new TcpClient();
            await remoteClient.ConnectAsync(remoteHost, remotePort);
            var clientStream = client.GetStream();
            var remoteStream = remoteClient.GetStream();
            Task clientReadTask = clientStream.CopyToAsync(remoteStream);
            Task remoteReadTask = remoteStream.CopyToAsync(clientStream);
            await Task.WhenAll(clientReadTask, remoteReadTask);
            client.Close();
            remoteClient.Close();
        }
    }
}
