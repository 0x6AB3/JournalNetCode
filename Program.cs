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
        
        var client = new Client("127.0.0.1", 9600);
        
        // Listen loop
        var running = true;
        while (running)
        {
            /*
            Console.WriteLine("Email address: example@test.com");
            const string email = "example@test.com";
            Console.WriteLine("Password: example123");
            const string password = "example123";
            */
            var option = Console.ReadLine().Split(" ");
            var command = option[0];
            
            if (command == "exit")
            {
                break;
            }

            switch (command)
            {
                case "exit":
                    running = false;
                    break;
                case "signup":
                    string email, password;
                    try
                    {
                        email = option[1];
                        password = option[2];
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Invalid parameters: {ex.Message}");
                        break;
                    }
                    
                    var signupSuccess = await client.SignUp(email, password);
                    Console.WriteLine($"Sign up success: {signupSuccess}");
                    break;
                case "login":
                    break;
            }
            
            
        }
        
        journalServer.Stop();
    }
}
