using System.Net;
using System.Runtime.Intrinsics.Arm;
using System.Text.Json;
using System.Text.Json.Serialization;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Database;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    public readonly IPEndPoint RemoteEndPoint;
    private HttpListenerContext? _context;
    private string? _email;

    private string GetIdentifier()
    {
        var identifier = _email ?? "";
        if (identifier.Length != 0)
        {
            identifier += "/";
        }
        identifier += RemoteEndPoint;
        return identifier;
    }
    
    public ClientInterface(IPEndPoint endPoint)
    {
        RemoteEndPoint = endPoint;
    }

    public async Task Process(HttpListenerContext context)
    {
        _context = context;
        var request = _context.Request;
        if (request.HttpMethod != "POST") // Expecting a 'POST' followed by a JSON string that deserialises to ClientRequest
        {
            DispatchError("Unsupported request; POST [ClientRequest JSON] instead");
            return;
        }
        
        var clientRequest = await GetClientRequest(request); // Retrieving the POST request content provided by the client
        if (clientRequest == null) // NULL CHECK
        {   
            DispatchError("Please provide a valid [ClientRequest JSON] with your POST request"); 
            return;
        }
        Logger.AppendMessage($"Received {clientRequest.RequestType.ToString().ToLower()} request from {GetIdentifier()}");
        
        ServerResponse response;
        switch (clientRequest.RequestType)
        {   /////////////////////////////////////// SIGNUP AND LOGIN ////////////////////////
            case ClientRequestType.SignUp:
            case ClientRequestType.LogIn:
                if (!clientRequest.TryGetLoginDetails(out var loginDetails)) // null check on loginDetails performed here
                {
                    DispatchError("Unable to deserialise LoginDetails JSON");
                    return;
                }
                response = clientRequest.RequestType == ClientRequestType.SignUp
                    ? RequestHandler.HandleSignUp(loginDetails)
                    : RequestHandler.HandleLogIn(loginDetails);
                    
                // Logger output
                if (response.ResponseType == ServerResponseType.Success) 
                    _email = loginDetails.Email; // Null check in RequestHandler.cs
                break;
            /////////////////////////////////////// NOTE UPLOAD /////////////////////////
            case ClientRequestType.PostNote:
                if (!LoginCheck())
                    return;
                var note = JsonSerializer.Deserialize<Note>(clientRequest.Body);
                response = RequestHandler.PostNote(note, _email); // null check performed by method LoginCheck()
                break;
            /////////////////////////////////////// NOTE DOWNLOAD ///////////////////////
            case ClientRequestType.GetNote:
                if (!LoginCheck())
                    return;
                response = RequestHandler.GetNote(clientRequest.Body, _email);
                break;
            /////////////////////////////////////// STATUS CHECK ////////////////////////
            case ClientRequestType.LoginStatus:
                response = RequestHandler.LoginStatus(_email, RemoteEndPoint.ToString());
                break;
            /////////////////////////////////////// ALL OF USER'S NOTE TITLES ///////////////
            case ClientRequestType.GetNoteTitles:
                if (!LoginCheck())
                    return;
                response = RequestHandler.GetNoteTitles(_email);
                break;
            /////////////////////////////////////// DELETING USER NOTE ///////////////
            case ClientRequestType.DeleteNote:
                if (!LoginCheck())
                    return;
                response = RequestHandler.DeleteNote(clientRequest.Body, _email);
                break;
            /////////////////////////////////////// DELETING USER ACCOUNT ///////////////
            case ClientRequestType.DeleteAccount:
                if (!LoginCheck())
                    return;
                response = RequestHandler.DeleteAccount(_email);
                _email = null;
                break;
            /////////////////////////////////////// ERRONEOUS ///////////////////////////
            default: 
                DispatchError($"Unsupported ClientRequestType");
                return;
        }
        DispatchResponse(response);
    }

    private bool LoginCheck()
    {
        if (_email == null) // Check if client is logged into an account
        {
            DispatchError("You are not logged in to an account in this session; sign-up/log-in to account before requesting PostNote");
            return false;
        }
        return true;
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
            Logger.AppendMessage($"{serverResponse.Body} --> {GetIdentifier()}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.AppendError($"Error while sending ServerResponse JSON to {GetIdentifier()}", ex.Message);
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
        Logger.AppendWarn($"{GetIdentifier()} Invalid POST request: {addendum}");
        DispatchResponse(serverResponse);
    }
}