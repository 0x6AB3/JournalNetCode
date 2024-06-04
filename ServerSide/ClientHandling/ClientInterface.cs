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
            var clientRequest = await GetClientRequest(request);
            if (clientRequest == null || clientRequest.Body == null)
            { SendClientRequestError("Please provide a valid ClientRequest Json"); return; }
            
            switch (clientRequest.RequestType)
            {
                case ClientRequestType.SignUp:
                    try
                    {
                        var loginDetails = JsonSerializer.Deserialize<LoginDetails>(clientRequest.Body);
                        var serverResponse = RequestHandler.HandleSignup(loginDetails);
                        if (serverResponse.ResponseType == ServerResponseType.Success)
                        {
                            _email = loginDetails.Email; // Null check in RequestHandler.cs
                            SendServerResponse(serverResponse);
                            Logger.AppendMessage($"{_endPoint} Successful signup");
                        }
                        
                    }
                    catch (ArgumentNullException ex)
                    {

                    }
                    catch (JsonException ex)
                    {

                    }
                    catch (NotSupportedException ex)
                    {
                        Logger.AppendMessage($"Critical error: {ex.Message}");
                    }
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
            SendClientRequestError("Unsupported request; POST [sub-request] instead");
        }
    }

    // Gets ClientRequest object from the JSON that is included in POST
    private async Task<ClientRequest?> GetClientRequest(HttpListenerRequest post)
    {
        using var reader = new StreamReader(post.InputStream, post.ContentEncoding);
        var clientRequestJson = await reader.ReadToEndAsync();
        var clientRequest = JsonSerializer.Deserialize<ClientRequest>(clientRequestJson);
        return clientRequest;
    }    
    
    // Server -- TX --> Client
    private void SendServerResponse(ServerResponse serverResponse)
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
    }
    
    private void SendClientRequestError(string addendum = "None")
    {
        var serverResponse = new ServerResponse()
        {
            Body = "Error with your request/sub-request (request should look like: POST [ClientRequest Json string])"
                   + $"Additional information: {addendum}",
            ResponseType = ServerResponseType.Error
        };
        SendServerResponse(serverResponse);
    }
}