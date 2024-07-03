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

    public async Task<bool> SignUp(string emailAddress, string password)
    {
        //Console.WriteLine($"Attempting signup with {emailAddress}:{password}");
        var details = new LoginDetails(emailAddress, password, out var encryptionKey);
        var detailsJson = Cast.ObjectToJson(details);
        
        var request = new ClientRequest() { Body = detailsJson, RequestType = ClientRequestType.SignUp };
        return await SendRequest(request);
    }

    public async Task<bool> LogIn(string emailAddress, string password)
    {
        Console.WriteLine($"Attempting login with {emailAddress}:{password}");
        var details = new LoginDetails(emailAddress, password, out var encryptionKey);
        var detailsJson = Cast.ObjectToJson(details);
        
        var request = new ClientRequest() { Body = detailsJson, RequestType = ClientRequestType.LogIn };
        return await SendRequest(request);
    }
    
    public async Task<bool> GetLoggedIn()
    {
        var request = new ClientRequest() { RequestType = ClientRequestType.LoginStatus };
        return await SendRequest(request);
    }
    
    public async Task<bool> PostNote(Note note)
    {
        var noteJson = note.Serialise();
        var request = new ClientRequest() {Body = noteJson, RequestType = ClientRequestType.PostNote };
        return await SendRequest(request);
    }

    public async Task<bool> GetNote(string name)
    {
        var request = new ClientRequest() { Body = name, RequestType = ClientRequestType.GetNote };
        return await SendRequest(request);
    }

    public async Task<string[]?> GetNoteTitles() // Grabs all note names that belong to the user in the database
    {
        var request = new ClientRequest() { RequestType = ClientRequestType.GetNoteTitles };
        await SendRequest(request);
        string[]? titles = null;
        
        try
        {
            titles = File.ReadAllText("./temp.txt").Trim('`').Split("`");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Unable to retrieve your notes from the server (are you logged in and have uploaded notes?)" +
                              $"\n{ex.Message}");
        }

        return titles;
    }

    public async Task<bool> DeleteNote(string name)
    {
        var request = new ClientRequest() { Body = name, RequestType = ClientRequestType.DeleteNote };
        return await SendRequest(request);
    }

    private async Task<bool> SendRequest(ClientRequest request)
    {
        var requestJson = request.Serialise();
        
        var responseJson = await SendContent(requestJson);
        if (responseJson == null) { return false; } // null check
        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
        var success = response.ResponseType == ServerResponseType.Success;
        if (request.RequestType == ClientRequestType.GetNote && success)
            JsonSerializer.Deserialize<Note>(response.Body).ToFile(); // Saving to non-volatile location (Notes/)
        else if (request.RequestType == ClientRequestType.GetNoteTitles && success)
            File.WriteAllText("./temp.txt", response.Body); // TODO REFACTOR CLIENT TO DO THIS EFFICIENTLY
        
        return success;
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
            Console.WriteLine($"DEBUG: CLIENT POST [{message}] to {_ip}:{_port} GOT [{responseContent}]");
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"DEBUG: CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }
}