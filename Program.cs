using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        Console.WriteLine("Creating server...");
        var journalServer = new Server("127.0.0.1", 80);
        
        Console.WriteLine("Starting server...");
        journalServer.Start();

        // Listen loop
        while (true)
        {
            if (Console.ReadLine() == "exit")
            {
                break;
            }
            Console.WriteLine("Creating client...");
            var client = new Client("127.0.0.1", 80);
        
            Console.WriteLine($"Retrieving content...");
            var msg = await client.RetrieveContent();
            var response = msg == null ? "Client received no data" : $"Client has received: {msg}";
            Console.WriteLine(response);
        }

        Console.WriteLine("Stopping server...");
        journalServer.Stop();
    }
}
