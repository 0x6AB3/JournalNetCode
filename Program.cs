using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;
using JournalNetCode.Common.Security;
using JournalNetCode.ServerSide.ClientHandling;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        float passwordAvgLength = 0;
        float saltAvgLength = 0;
        int n = 100;
        for (int i = 0; i < n; i++)
        {
            Console.Write($"{i} ");
            var random = new Random();
            var data = random.Next(10000, 1000000000).ToString();
            var details = new LoginDetails($"{data}@gmail.com", data);
            passwordAvgLength += details.PasswordHashB64.Length;
            saltAvgLength += details.SaltB64.Length;
        }

        passwordAvgLength /= n;
        saltAvgLength /= n;
        
        Console.WriteLine($"\nAverage password length: {passwordAvgLength}\nAverage salt length: {saltAvgLength}");
        return;
        
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

            var result = await client.SignUp(email, password);
            Console.WriteLine($"Login success = {result}");
        }
        
        journalServer.Stop();
    }
}
