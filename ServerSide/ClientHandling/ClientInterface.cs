using System.Net;
using System.Text.Json;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    public readonly IPEndPoint remoteEndPoint;
    private HttpListenerContext? _context;
    private string? _email;

    private string Identifier()
    {
        var identifier = _email ?? "";
        if (identifier.Length != 0)
        {
            identifier += "/";
        }
        identifier += remoteEndPoint;
        return identifier;
    }
    
    public ClientInterface(IPEndPoint endPoint)
    {
        remoteEndPoint = endPoint;
    }

    public async Task Process(HttpListenerContext context)
    {
        _context = context;
        var request = _context.Request;
        if (request.HttpMethod != "POST")
        {
            DispatchError("Unsupported request; POST [ClientRequest JSON] instead");
            return;
        }
        
        // Null check on ClientRequest data received with POST
        var clientRequest = await GetClientRequest(request);
        if (clientRequest == null || clientRequest.Body == null)
        { DispatchError("Please provide a valid [ClientRequest JSON] with your POST request"); return; }
        Logger.AppendMessage($"{Identifier()} requests {clientRequest.RequestType.ToString().ToLower()}");
        ServerResponse response;
        switch (clientRequest.RequestType)
        {
            case ClientRequestType.SignUp:
            case ClientRequestType.LogIn:
                if (!clientRequest.TryGetLoginDetails(out var loginDetails))
                {
                    Logger.AppendError($"Error during {clientRequest.RequestType.ToString().ToLower()}", "Unable to deserialise LoginDetails JSON string");
                    DispatchError("Unable to deserialise the body of ClientRequest (LoginDetails JSON)");
                }
                response = clientRequest.RequestType == ClientRequestType.SignUp
                    ? RequestHandler.HandleSignUp(loginDetails)
                    : RequestHandler.HandleLogIn(loginDetails);
                DispatchResponse(response);
                    
                // Logger output
                if (response.ResponseType == ServerResponseType.Success) 
                {
                    _email = loginDetails.Email; // Null check in RequestHandler.cs
                    Logger.AppendMessage($"{Identifier()} Successful {clientRequest.RequestType.ToString().ToLower()}");
                }
                else { Logger.AppendWarn($"{Identifier()} {clientRequest.RequestType.ToString().ToLower()} error: {response.Body}"); } 
                break;
            case ClientRequestType.PostNote:
            case ClientRequestType.GetNote:
                break; // TODO
            case ClientRequestType.GetLoggedIn:
                response = RequestHandler.GetLoggedIn(_email, remoteEndPoint.ToString());
                DispatchResponse(response);
                break;
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
    private bool DispatchResponse(ServerResponse serverResponse) // TODO revise this
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
            Logger.AppendMessage($"{serverResponse.Body} --> {Identifier()}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.AppendError($"Error while sending ServerResponse JSON to {Identifier()}", ex.Message);
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