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
            Console.WriteLine("Email address: example@test.com");
            const string email = "example@test.com";
            Console.WriteLine("Password: example123");
            const string password = "example123";
            if (Console.ReadLine() == "exit")
            {
                break;
            }
            
            var client = new Client("127.0.0.1", 9600);

            var signupSuccess = await client.SignUp(email, password);
            if (signupSuccess)
            {
                Console.WriteLine("Successful signup");
            }
            else
            {
                Console.WriteLine("Unable to sign up with these credentials");
            }
        }
        
        journalServer.Stop();
    }
}
