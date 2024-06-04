using System.Net;
using System.Text.Json;
using JournalNetCode.Common.Requests;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    private readonly IPEndPoint _endPoint;
    private readonly HttpListenerContext _context;
    private bool _authenticated; // TODO implement this
    private string _email;

    public ClientInterface(HttpListenerContext context)
    {
        _context = context;
        var request = context.Request;
        _endPoint = request.RemoteEndPoint;
        HandleRequest(request); // TODO sort this out
    }

    private async Task HandleRequest(HttpListenerRequest request)
    {
        if (request.HttpMethod == "POST")
        {
            var requestJson = await RetrieveMessage(request);
            var subRequest = JsonSerializer.Deserialize<ClientRequest>(requestJson);
            if (subRequest == null) { SendSubRequestError(); return; } // null check
            switch (subRequest.RequestType)
            {
                case ClientRequestType.SignUp:
                    if (subRequest.Body == null) { SendSubRequestError(); return; } // null check
                    var details = JsonSerializer.Deserialize<LoginDetails>(subRequest.Body);
                    if (details == null) { SendSubRequestError(); return; } // null check
                    HandleSignup(details); // TODO add logs
                    break;
                case ClientRequestType.LogIn:
                    break; // TODO
                case ClientRequestType.PostNote:
                    break; // TODO
                case ClientRequestType.GetNote:
                    break; // TODO
                case ClientRequestType.Unknown:
                    break; // TODO
                default:
                    break; // TODO
            }
        }
        else
        {
            SendSubRequestError("GET is not supported");
        }
    }

    private void HandleLogin()
    {
        // TODO
    }

    private void HandleSignup(LoginDetails details)
    {
        // TODO CHECK IN DATABASE BEFORE HERE
        var response = new ServerResponse()
        {
            Body = "SUCCESS",
            ResponseType = ServerResponseType.Success
        };
        var responseJson = Cast.ObjectToJson(response);
        _email = details.Email;
        _authenticated = true;
        SendMessage(responseJson);
        Logger.AppendMessage($"{_endPoint} logged into {_email}");
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
        response.ContentType = "application/base64";
        response.StatusDescription = "OK";
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength64 = messageOut.LongLength;
        response.OutputStream.Write(messageOut);
        response.OutputStream.Close();
        response.Close();
    }
    
    private void SendSubRequestError(string addendum = "None")
    {
        var subRequestError = new ServerResponse()
        {
            Body = "Please provide a valid sub-request (example: SIGNUP [LOGIN-DETAILS JSON])"
                   + $"Additional information: {addendum}",
            ResponseType = ServerResponseType.Error
        };
        SendMessage(subRequestError.Serialise());
    }
}