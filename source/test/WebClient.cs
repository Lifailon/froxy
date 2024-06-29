using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;


class Program {

    static async Task Main(string[] args) {
        var local = "http://*:8443/";
        var remote = "https://kinozal.tv";
        var server = new HttpListener();
        server.Prefixes.Add(local);
        server.Start();
        Console.WriteLine($"Listening on {local}, forwarding to {remote}");
        
        while (true) {
            var context = await server.GetContextAsync();
            HandleRequest(context, remote);
        }
    }

    static async Task HandleRequest(HttpListenerContext context, string remote) {
        var request = context.Request;
        var response = context.Response;
        Console.WriteLine($"{request.RemoteEndPoint} {request.HttpMethod} {request.Url} {request.ProtocolVersion}");
        using (var client = new WebClient()) {
            client.Headers.Add("User-Agent", request.UserAgent);
            try {
                var requestData = await client.DownloadDataTaskAsync(remote + request.RawUrl);
                response.OutputStream.Write(requestData, 0, requestData.Length);
                response.StatusCode = 200;
            }
            catch (WebException ex) {
                if (ex.Response != null) {
                    var errorResponse = (HttpWebResponse)ex.Response;
                    response.StatusCode = (int)errorResponse.StatusCode;
                    using (var errorStream = errorResponse.GetResponseStream())
                    {
                        errorStream.CopyTo(response.OutputStream);
                    }
                }
                else {
                    response.StatusCode = 500;
                    var errorMessage = $"Error occurred: {ex.Message}";
                    var errorData = System.Text.Encoding.UTF8.GetBytes(errorMessage);
                    response.OutputStream.Write(errorData, 0, errorData.Length);
                }
            }
        }
        response.Close();
    }
}
