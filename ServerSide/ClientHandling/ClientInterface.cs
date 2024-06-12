using System.Net;
using System.Text.Json;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    private readonly IPEndPoint _endPoint;
    private readonly HttpListenerContext _context;
    private string? _email;
    private string Identifier => _email ?? _endPoint?.ToString() ?? "Unable to retrieve client identifier";

    public ClientInterface(HttpListenerContext context)
    {
        _context = context;
        var request = context.Request;
        _endPoint = request.RemoteEndPoint;
        HandleRequest(request); // TODO sort this out
    }

    private async Task HandleRequest(HttpListenerRequest request)
    {
        if (request.HttpMethod != "POST")
        {
            DispatchError("Unsupported request; POST [ClientRequest JSON] instead");
            return;
        }
        
        // Null check on ClientRequest data received with POST
        var clientRequest = await GetClientRequest(request);
        if (clientRequest == null || clientRequest.Body == null)
        { DispatchError("Please provide a valid [ClientRequest JSON] with your POST request"); return; }
        Logger.AppendMessage($"{Identifier} requests {clientRequest.RequestType.ToString().ToLower()}");
        
        switch (clientRequest.RequestType)
        {
            case ClientRequestType.SignUp:
            case ClientRequestType.LogIn:
                if (!clientRequest.TryGetLoginDetails(out var loginDetails))
                {
                    Logger.AppendError($"Error during {clientRequest.RequestType}", "Unable to deserialise LoginDetails JSON string");
                    DispatchError("Unable to deserialise the body of ClientRequest (LoginDetails JSON)");
                }
                var serverResponse = clientRequest.RequestType == ClientRequestType.SignUp
                    ? RequestHandler.HandleSignUp(loginDetails)
                    : RequestHandler.HandleLogIn(loginDetails);
                DispatchResponse(serverResponse);
                    
                // Logger output
                if (serverResponse.ResponseType == ServerResponseType.Success) 
                {
                    _email = loginDetails.Email; // Null check in RequestHandler.cs
                    Logger.AppendMessage($"{Identifier} Successful {clientRequest.RequestType}");
                }
                else { Logger.AppendWarn($"{Identifier} {clientRequest.RequestType} error: {serverResponse.Body}"); } 
                break;
            case ClientRequestType.PostNote:
            case ClientRequestType.GetNote:
                break; // TODO
            default: 
                DispatchError("Unsupported request; POST [valid sub-request] instead");
                break;
        }
    }

    // Gets ClientRequest object from the JSON that is included in POST
    private static async Task<ClientRequest?> GetClientRequest(HttpListenerRequest post)
    {
        using var reader = new StreamReader(post.InputStream, post.ContentEncoding);
        var clientRequestJson = await reader.ReadToEndAsync();
        var clientRequest = JsonSerializer.Deserialize<ClientRequest>(clientRequestJson);
        return clientRequest;
    }    
    
    // Server -- TX --> Client
    private bool  DispatchResponse(ServerResponse serverResponse) // TODO revise this
    {
        try
        {
            var json = serverResponse.Serialise();
            var messageOut = Cast.StringToBytes(json);
            var response = _context.Response;
            response.ContentType = "application/base64";
            response.StatusDescription = "OK";
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = messageOut.LongLength;
            response.OutputStream.Write(messageOut);
            response.OutputStream.Close();
            response.Close();
            Logger.AppendMessage($"[{serverResponse.Body}] --> {Identifier}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.AppendError($"Error while sending ServerResponse JSON to {Identifier}", ex.Message);
            return false;
        }
    }
    
    private void DispatchError(string addendum = "None")
    {
        var serverResponse = new ServerResponse()
        {
            Body = "Error with your request/sub-request (request should look exactly like: POST [ClientRequest JSON string])"
                   + $"Additional information: {addendum}",
            ResponseType = ServerResponseType.Failure
        };
        DispatchResponse(serverResponse);
    }
}