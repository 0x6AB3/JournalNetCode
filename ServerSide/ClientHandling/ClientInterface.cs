using System.Net;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    private readonly IPEndPoint _endPoint;
    private readonly HttpListenerContext _context;
    private bool _loggedIn;
    private string _username;

    public ClientInterface(HttpListenerContext context)
    {
        _context = context;
        var request = context.Request;
        _endPoint = request.RemoteEndPoint;
        HandleRequest(request);
    }

    private async Task HandleRequest(HttpListenerRequest request)
    {
        var messageOut = $"Hello {_endPoint.ToString()}, this is the server";
        Logger.AppendMessage($"Received {request.HttpMethod} request from {_endPoint.ToString()}");
        switch (request.HttpMethod)
        {
            case "POST":
                var message = await RetrieveMessage(request);
                SendMessage(messageOut);
                Logger.AppendMessage($"Client sent: [{message}]");
                
                break;
            case "GET":
                SendMessage(messageOut);
                break;
        }
    }

    private async Task<string> RetrieveMessage(HttpListenerRequest postRequest)
    {
        using var reader = new StreamReader(postRequest.InputStream, postRequest.ContentEncoding);
        return await reader.ReadToEndAsync();
    }

    private void SendMessage(string message)
    {
        var messageOut = Cast.StringToBytes(message);
        var response = _context.Response;
        response.ContentType = "text/html";
        response.StatusDescription = "OK";
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength64 = messageOut.LongLength;
        response.OutputStream.Write(messageOut);
        response.OutputStream.Close();
        response.Close();
    }
}