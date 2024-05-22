using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        Console.WriteLine("Creating server...");
        var journalServer = new Server("127.0.0.1", 9600);
        Console.WriteLine("[Server instantiated]");
        
        Console.WriteLine("Starting server...");
        journalServer.Start();
        Console.WriteLine("[Server started]");

        // Listen loop
        while (true)
        {
            var input = Console.ReadLine();
            if (input is "exit")
            {
                Console.WriteLine("Quitting...");
                break;
            }
            Console.WriteLine("Creating client...");
            var client = new Client("127.0.0.1", 9600);
            Console.WriteLine("[Client created]");
        
            Console.WriteLine($"Retrieving content...");
            var msg = await client.RetrieveContent();
            Console.WriteLine($"[Content received]");
            var response = msg == null ? "Client received no data" : $"Client has received: {msg}";
            Console.WriteLine(response);
        }

        Console.WriteLine("Stopping server...");
        journalServer.Stop();
    }
}
