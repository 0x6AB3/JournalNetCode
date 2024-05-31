using System.Text;
using System.Net.Mail;
using JournalNetCode.Common.Utility;

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

    public bool SignUp(string emailAddress, string password)
    {
        if (!Validate.EmailAddress(emailAddress))
        {
            throw new ArgumentException("Invalid email address");
        }
        
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

    public async Task SendContent(string message = "test")
    {
        using var client = new HttpClient();
        var content = new StringContent(message, Encoding.UTF8, "text/plain");
        var response = await client.PostAsync($"http://{_ip}:{_port}/", content);

        try
        {
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CLIENT POST [{message}] to {_ip}:{_port} GOT [{responseContent}]");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"CLIENT REQUEST ERROR: {ex.Message}");
        }
    }
}