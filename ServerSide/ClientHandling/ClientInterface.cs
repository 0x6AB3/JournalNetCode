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
            DispatchError("Unsupported request; send a POST request with a valid ClientRequest JSON string instead");
            return;
        }
        
        // Retrieving the ClientRequest object that the client includes in the POST request
        var clientRequest = await GetClientRequest(request);
        if (clientRequest == null)
        {   
            DispatchError("Please provide a valid [ClientRequest JSON] in your POST request"); 
            return;
        }
        Logger.AppendMessage($"Received {clientRequest.RequestType.ToString().ToLower()} request from {GetIdentifier()}");
        
        ServerResponse response;
        switch (clientRequest.RequestType)
        {   /////////////////////////////////////// SIGNUP AND LOGIN ///////////////////////////////////////////////////
            case ClientRequestType.SignUp:
            case ClientRequestType.LogIn:
                if (!clientRequest.TryGetLoginDetails(out var loginDetails)) // loginDetails null check performed here
                {
                    DispatchError("Unable to deserialise your LoginDetails");
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
            /////////////////////////////////////// CLIENT NOTE --> SERVER /////////////////////////////////////////////
            case ClientRequestType.PostNote:
                if (!LoginCheck()) // Checks if client is logged in (client is informed and request is cancelled if not)
                    return;
                if (clientRequest.Body == null)
                {
                    DispatchError("Please include a Note JSON string");
                    return;
                }
                var note = JsonSerializer.Deserialize<Note>(clientRequest.Body);
                response = RequestHandler.PostNote(note, _email); // _email null check performed by method LoginCheck()
                break;
            /////////////////////////////////////// SERVER NOTE --> CLIENT /////////////////////////////////////////////
            case ClientRequestType.GetNote:
                if (!LoginCheck())
                    return;
                response = RequestHandler.GetNote(clientRequest.Body, _email);
                break;
            
            /////////////////////////////////////// STORED NOTE TITLES --> CLIENT //////////////////////////////////////
            case ClientRequestType.GetNoteTitles:
                if (!LoginCheck())
                    return;
                response = RequestHandler.GetNoteTitles(_email);
                break;
            /////////////////////////////////////// NOTE DELETION //////////////////////////////////////////////////////
            case ClientRequestType.DeleteNote:
                if (!LoginCheck())
                    return;
                response = RequestHandler.DeleteNote(clientRequest.Body, _email);
                break;
            /////////////////////////////////////// ACCOUNT DELETION ///////////////////////////////////////////////////
            case ClientRequestType.DeleteAccount:
                if (!LoginCheck())
                    return;
                response = RequestHandler.DeleteAccount(_email);
                _email = null; // Logging out the client
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
        if (_email != null) // Check if client is logged into an account
        {
            return true;
        }
        DispatchError("You are not logged in to an account in this session; " +
                      "sign-up/log-in to account before requesting PostNote");
        return false;
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
            // Limit output to 100 chars
            var outputMessage = serverResponse.Body?.Length < 100 ? serverResponse.Body : "JSON STRING";
            Logger.AppendMessage($"{outputMessage} --> {GetIdentifier()}");
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
            Body = "Error with your request... "
                   + $"Additional information: {addendum}",
            ResponseType = ServerResponseType.Failure
        };
        Logger.AppendWarn($"{GetIdentifier()} Invalid POST request: {addendum}");
        DispatchResponse(serverResponse);
    }
}