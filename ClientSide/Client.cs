using System.Text;
using System.Text.Json;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ClientSide;

public class Client
{
    private readonly string _ip;
    private readonly int _port; 
    private readonly HttpClient _client;

    ~Client()
    {
        _client.Dispose();
    }
    
    public Client(string ipAddress = "127.0.0.1", int port = 80)
    {
        _ip = ipAddress;
        _port = port;
        _client = new HttpClient();
    }

    private async Task<ServerResponse> SendLoginDetails(string emailAddress, string password, ClientRequestType type)
    {
        var details = new LoginDetails(emailAddress, password, out var encryptionKey);
        var detailsJson = Cast.ObjectToJson(details);
        
        var request = new ClientRequest(type, detailsJson);
        return await SendRequest(request);
    }

    public async Task<(bool, string?)> SignUp(string emailAddress, string password)
    {
        var response = await SendLoginDetails(emailAddress, password, ClientRequestType.SignUp);
        return (response.ResponseType == ServerResponseType.Success, response.Body);
    }

    public async Task<(bool, string?)> LogIn(string emailAddress, string password)
    {
        var response = await SendLoginDetails(emailAddress, password, ClientRequestType.LogIn);
        return (response.ResponseType == ServerResponseType.Success, response.Body);
    }
    
    public async Task<ServerResponse> GetLoggedIn()
    {
        var request = new ClientRequest(ClientRequestType.LoginStatus);
        return await SendRequest(request);
    }
    
    public async Task<ServerResponse> PostNote(Note note)
    {
        var noteJson = note.Serialise();
        var request = new ClientRequest(ClientRequestType.PostNote, noteJson);
        return await SendRequest(request);
    }

    public async Task<Note?> GetNote(string name)
    {
        var request = new ClientRequest(ClientRequestType.GetNote, name);
        var response = await SendRequest(request);

        if (string.IsNullOrEmpty(response.Body))
        {
            return null;
        }
        
        var note = JsonSerializer.Deserialize<Note>(response.Body);
        return note;
    }

    public async Task<string[]?> GetNoteTitles() // Grabs all note names that belong to the user in the database
    {
        var request = new ClientRequest(ClientRequestType.GetNoteTitles);
        var response = await SendRequest(request);
        
        if (response.ResponseType != ServerResponseType.Success)
        {
            return null;
        }

        return response.Body?.Split('`');
    }

    public async Task<bool> DeleteNote(string name)
    {
        var request = new ClientRequest(ClientRequestType.DeleteNote, name);
        var response = await SendRequest(request);
        return response.ResponseType == ServerResponseType.Success;
    }
    
    public async Task<ServerResponse> DeleteAccount()
    {
        var request = new ClientRequest(ClientRequestType.DeleteAccount);
        return await SendRequest(request);
    }

    private ServerResponse GenerateNullResponse()
    {
        var nullResponse = new ServerResponse(ServerResponseType.NullResponse);
        return nullResponse;
    }

    private async Task<ServerResponse> SendRequest(ClientRequest request)
    {
        var requestJson = request.Serialise();
        var responseJson = await SendContent(requestJson);

        if (responseJson == null)
        {
            return GenerateNullResponse();
        }
        
        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
        
        if (response == null)
        {
            return GenerateNullResponse();
        }

        // todo
        if (request.RequestType == ClientRequestType.GetNote && response.ResponseType == ServerResponseType.Success)
        {
            if (response.Body == null)
            {
                return new ServerResponse(ServerResponseType.InvalidParameters, "Note is empty");
            }
            JsonSerializer.Deserialize<Note>(response.Body).ToFile(); // Saving to non-volatile location (Notes/)
        }
        
        return response;
    }

    private async Task<string?> ReceiveContent() // used during first demo, not needed for now as POST is used for sending requests
    {
        try
        {
            var response = await _client.GetAsync($"http://{_ip}:{_port}/");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"DEBUG: CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> SendContent(string message) // client -- TX -- > server
    {
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync($"http://{_ip}:{_port}/", content);

        try
        {
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            //Console.WriteLine($"DEBUG: CLIENT POST [{message}] to {_ip}:{_port} GOT [{responseContent}]");
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"DEBUG: CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }
}