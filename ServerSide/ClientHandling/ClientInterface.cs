using System.Net;
using System.Text.Json;
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
        Logger.AppendMessage($"Received {request.HttpMethod} request from {_endPoint}");
        switch (request.HttpMethod)
        {
            case "POST":
                var requestContent = await RetrieveMessage(request);
                var subRequest = requestContent.Split(" ")[0];
                switch (subRequest)
                {
                    case "SIGNUP":
                        var loginDetails = JsonSerializer.Deserialize<LoginDetails>(requestContent.Split(" ")[1]);
                        Logger.AppendMessage($"{_endPoint} attempts to signup with {loginDetails}");
                        SendMessage("SUCCESS");
                        break;
                }
                break;
            case "GET":
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