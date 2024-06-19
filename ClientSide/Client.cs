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

    public async Task<bool> UploadNote(StreamReader fileStream)
    {
        // todo
        return false;
    }

    public async Task<bool> GetLoggedIn()
    {
        //Console.WriteLine($"Asking server whether client is logged in");
        var request = new ClientRequest() { Body = "empty", RequestType = ClientRequestType.GetLoggedIn };
        var requestJson = Cast.ObjectToJson(request);
        
        var responseJson = await SendContent(requestJson);
        if (responseJson == null) { return false; } // null check
        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
        //Console.WriteLine($"Server response: {response.Body}");
        
        return response.ResponseType == ServerResponseType.Success;
    }

    public async Task<bool> LogIn(string emailAddress, string password)
    {
        Console.WriteLine($"Attempting login with {emailAddress}:{password}");
        var details = new LoginDetails(emailAddress, password, out var encryptionKey);
        var detailsJson = Cast.ObjectToJson(details);
        
        var request = new ClientRequest() { Body = detailsJson, RequestType = ClientRequestType.LogIn };
        var requestJson = Cast.ObjectToJson(request);
        
        var responseJson = await SendContent(requestJson);
        if (responseJson == null) { return false; } // null check
        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
        //Console.WriteLine($"Server response: {response.Body}");
        
        return response.ResponseType == ServerResponseType.Success;
    }
    
    public async Task<bool> SignUp(string emailAddress, string password)
    {
        //Console.WriteLine($"Attempting signup with {emailAddress}:{password}");
        var details = new LoginDetails(emailAddress, password, out var encryptionKey);
        var detailsJson = Cast.ObjectToJson(details);
        
        var request = new ClientRequest() { Body = detailsJson, RequestType = ClientRequestType.SignUp };
        var requestJson = Cast.ObjectToJson(request);
        
        var responseJson = await SendContent(requestJson);
        if (responseJson == null) { return false; } // null check
        var response = JsonSerializer.Deserialize<ServerResponse>(responseJson);
        //Console.WriteLine($"Server response: {response.Body}");
        
        return response.ResponseType == ServerResponseType.Success;
    }

    private async Task<string?> RetrieveContent()
    {
        try
        {
            var response = await _client.GetAsync($"http://{_ip}:{_port}/");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }

    private async Task<string?> SendContent(string message)
    {
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        var response = await _client.PostAsync($"http://{_ip}:{_port}/", content);

        try
        {
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CLIENT POST [{message}] to {_ip}:{_port} GOT [{responseContent}]");
            return responseContent;
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }
}