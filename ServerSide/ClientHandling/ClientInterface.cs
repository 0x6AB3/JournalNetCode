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
    private string _email;
    private string Identifier => _email == null ? _endPoint.ToString() : _email;

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
            // Null check on ClientRequest data received with POST
            var clientRequest = await GetClientRequest(request);
            if (clientRequest == null || clientRequest.Body == null)
            { DispatchError("Please provide a valid ClientRequest Json"); return; }
            Logger.AppendMessage($"{Identifier} requests {clientRequest.RequestType.ToString().ToLower()}");
            
            switch (clientRequest.RequestType)
            {
                case ClientRequestType.SignUp:
                case ClientRequestType.LogIn:
                    try
                    {
                        var loginDetails = JsonSerializer.Deserialize<LoginDetails>(clientRequest.Body);
                        var serverResponse = clientRequest.RequestType == ClientRequestType.SignUp ? 
                            RequestHandler.HandleSignUp(loginDetails) : 
                            RequestHandler.HandleLogIn(loginDetails);
                        
                        DispatchResponse(serverResponse);
                        
                        if (serverResponse.ResponseType == ServerResponseType.Success)
                        {
                            _email = loginDetails.Email; // Null check in RequestHandler.cs
                            Logger.AppendMessage($"{Identifier} Successful {clientRequest.RequestType}");
                        }
                        else { Logger.AppendWarn($"{Identifier} {clientRequest.RequestType} error: {serverResponse.Body}"); }
                    }
                    catch (Exception ex) { Logger.AppendError($"Error during {clientRequest.RequestType}", ex.Message); }
                    break;
                case ClientRequestType.PostNote:
                    break; // TODO
                case ClientRequestType.GetNote:
                    break; // TODO
                default: 
                    DispatchError("Unsupported request; POST [valid sub-request] instead");
                    break;
            }
        }
        else DispatchError("Unsupported request; POST [sub-request] instead");
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
    private void DispatchResponse(ServerResponse serverResponse) // TODO revise this
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
    }
    
    private void DispatchError(string addendum = "None")
    {
        var serverResponse = new ServerResponse()
        {
            Body = "Error with your request/sub-request (request should look like: POST [ClientRequest Json string])"
                   + $"Additional information: {addendum}",
            ResponseType = ServerResponseType.Failure
        };
        DispatchResponse(serverResponse);
    }
}