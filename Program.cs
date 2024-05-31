using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        var journalServer = new Server("127.0.0.1", 9600, true);
        journalServer.Start();
        
        // Listen loop
        while (true)
        {
            var input = Console.ReadLine();
            if (input == "exit") { break; }
            
            var client = new Client("127.0.0.1", 9600);

            client.SendContent(input == null ? "im sending nothing" : input);
        }
        
        journalServer.Stop();
    }
}
