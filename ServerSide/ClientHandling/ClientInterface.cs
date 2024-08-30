using System.Net;
using System.Text.Json;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public class ClientInterface
{
    public readonly IPEndPoint RemoteEndPoint;
    private HttpListenerContext? _context;
    private string? _email;

    public ClientInterface(IPEndPoint endPoint)
    {
        RemoteEndPoint = endPoint;
    }
    
    // If the client is logged in, this method returns their email and endpoint.
    // Otherwise, just their endpoint is returned (as the client is not yet logged in)
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

    // This method executes the request provided by the client
    public async Task Process(HttpListenerContext context)
    {
        _context = context;
        var request = _context.Request;
        
        // Always expects an HTTP POST request that contains the actual client request
        if (request.HttpMethod != "POST")
        {
            DispatchError(ServerResponseType.InvalidRequest, "Unsupported request; send a POST request with a valid ClientRequest JSON string instead");
            return;
        }
        
        // Retrieving the ClientRequest object that the client includes in the POST request
        var clientRequest = await GetClientRequest(request);
        if (clientRequest == null)
        {   
            DispatchError(ServerResponseType.InvalidRequest, "Please provide a valid [ClientRequest JSON] in your POST request"); 
            return;
        }
        Logger.AppendMessage($"[{clientRequest.RequestType.ToString()}] <-- RX -- {GetIdentifier()}");
        
        ServerResponse response;
        switch (clientRequest.RequestType)
        {   /////////////////////////////////////// SIGNUP AND LOGIN ///////////////////////////////////////////////////
            case ClientRequestType.SignUp:
            case ClientRequestType.LogIn:
                if (!clientRequest.TryGetLoginDetails(out var loginDetails)) // loginDetails null check performed here
                {
                    DispatchError(ServerResponseType.InvalidRequest, "Unable to deserialise your LoginDetails");
                    return;
                }
                
                // Performing login/signup (loginDetails is deserialised successfully by now)
                // loginDetails null check is also performed here (todo redundant)
                response = clientRequest.RequestType == ClientRequestType.SignUp
                    ? RequestHandler.HandleSignUp(loginDetails)
                    : RequestHandler.HandleLogIn(loginDetails);
                    
                // Setting email to imply that client is logged in
                // loginDetails has null checks in ClientRequest.TryGetLoginDetails and RequestHandler.cs
                if (response.ResponseType == ServerResponseType.Success) 
                    _email = loginDetails.Email; // loginDetails cannot be null here
                break;
            /////////////////////////////////////// LOGIN STATUS CHECK /////////////////////////////////////////////////
            case ClientRequestType.LoginStatus:
                response = RequestHandler.LoginStatus(_email, RemoteEndPoint.ToString());
                break;
            /////////////////////////////////////// SERVER <-- RX -- CLIENT NOTE ///////////////////////////////////////
            case ClientRequestType.PostNote:
                if (clientRequest.Body == null)
                {
                    DispatchError(ServerResponseType.InvalidRequest, "Please include a Note JSON string");
                    return;
                }
                var note = JsonSerializer.Deserialize<Note>(clientRequest.Body);
                response = RequestHandler.PostNote(note, _email); // _email null check performed by method LoginCheck()
                break;
            /////////////////////////////////////// NOTE -- TX --> CLIENT //////////////////////////////////////////////
            case ClientRequestType.GetNote:
                response = RequestHandler.GetNote(clientRequest.Body, _email);
                break;
            /////////////////////////////////////// NOTE TITLES -- TX --> CLIENT ///////////////////////////////////////
            case ClientRequestType.GetNoteTitles:
                response = RequestHandler.GetNoteTitles(_email);
                break;
            /////////////////////////////////////// DELETE NOTE ////////////////////////////////////////////////////////
            case ClientRequestType.DeleteNote:
                response = RequestHandler.DeleteNote(clientRequest.Body, _email);
                break;
            /////////////////////////////////////// ACCOUNT DELETION ///////////////////////////////////////////////////
            case ClientRequestType.DeleteAccount:
                response = RequestHandler.DeleteAccount(_email);
                _email = null; // Logging out the client
                break;
            /////////////////////////////////////// ERRONEOUS ///////////////////////////
            default: 
                DispatchError(ServerResponseType.InvalidRequest, "Unsupported ClientRequestType");
                return;
        }
        DispatchResponse(response);
    }

    // This method retrieves the data that the client included in the POST request and 
    // deserialises it into a ClientRequest object that is used to process that request
    private static async Task<ClientRequest?> GetClientRequest(HttpListenerRequest postRequest)
    {
        using var reader = new StreamReader(postRequest.InputStream, postRequest.ContentEncoding);
        
        // Deserialising the JSON to a ClientRequest object
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
            Logger.AppendMessage($"[{serverResponse.ResponseType}] -- TX --> {GetIdentifier()}");
            return true;
        }
        catch (Exception ex)
        {
            Logger.AppendError($"Error while sending ServerResponse JSON to {GetIdentifier()}", ex.Message);
            return false;
        }
    }
    
    private void DispatchError(ServerResponseType errorType, string addendum = "None")
    {
        var serverResponse = new ServerResponse(errorType, addendum);
        Logger.AppendWarn($"{GetIdentifier()} Invalid POST request ({errorType}): {addendum}");
        DispatchResponse(serverResponse);
    }
}