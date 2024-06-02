using System.Text;
using System.Text.Json;
using JournalNetCode.ServerSide.ClientHandling;

namespace JournalNetCode.ClientSide;

public class Client
{
    private readonly string _ip;
    private readonly int _port;

    public Client(string ipAddress = "127.0.0.1", int port = 80)
    {
        _ip = ipAddress;
        _port = port;
    }

    public async Task<bool> SignUp(string emailAddress, string password)
    {

        LoginDetails details = new LoginDetails(emailAddress, password);
        string json = JsonSerializer.Serialize(details);
        var response = await SendContent($"SIGNUP {json}");
        if (response == "SUCCESS")
        {
            return true;
        }

        return false;
    }

    private async Task<string?> RetrieveContent()
    {
        using var client = new HttpClient();
        try
        {
            var response = await client.GetAsync($"http://{_ip}:{_port}/");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"CLIENT REQUEST ERROR: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> SendContent(string message = "test")
    {
        using var client = new HttpClient();
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        var response = await client.PostAsync($"http://{_ip}:{_port}/", content);

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