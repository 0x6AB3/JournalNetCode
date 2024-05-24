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

    public async Task<string?> RetrieveContent()
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
}