using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

class Program {
    static async Task Main(string[] args) {
        if (args.Length < 3) {
            Console.WriteLine("Usage: udp <localPort> <remoteHost> <remotePort>");
            return;
        }
        int localPort = int.Parse(args[0]);
        string remoteHost = args[1];
        int remotePort = int.Parse(args[2]);
        UdpClient udpClient = new UdpClient(localPort);
        IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Parse(remoteHost), remotePort);
        Console.WriteLine($"Listening for incoming UDP packets on port {localPort}...");
        while (true) {
            var receivedResult = await udpClient.ReceiveAsync();
            byte[] receivedData = receivedResult.Buffer;
            await udpClient.SendAsync(receivedData, receivedData.Length, remoteEndPoint);
        }
    }
}
