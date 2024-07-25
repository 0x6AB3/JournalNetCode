using System.Security.Cryptography;
using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task SimulateTraffic()
    {
        PasswordHashing argon = new PasswordHashing();
        for (var i = 0; i < 200; i++) // todo test on two threads creating connecting clients
        {
            var demoClient = new Client("127.0.0.1", 9600);
            var random = new Random();
            var id = random.Next(1000000, 1000010);
            var email = $"{id}@gmail.com";
            var password = $"{id}";
            var note = new Note($"{id.ToString()} Note {i}:{random.Next(50)}");
            var textBytes = new byte[1024];
            RandomNumberGenerator.Fill(textBytes);
            var text = Cast.BytesToBase64(textBytes);
            var encryptionKey = argon.GetEncryptionKey(password, email);
            note.SetText(text, encryptionKey);
            if (random.Next(1, 4) == 1)
                await demoClient.SignUp(email, password);
            if (random.Next(1, 4) == 1)
                await demoClient.LogIn(email, password);
            if (random.Next(1, 4) == 1)
                await demoClient.PostNote(note);
            if (random.Next(1, 4) == 1)
            {
                var titles = await demoClient.GetNoteTitles();
                await demoClient.GetNote(titles != null && titles.Length > 0 ? titles[random.Next(titles.Length)] : note.Title);
            }
            if (random.Next(1, 4) == 1)
                await demoClient.GetLoggedIn();
            if (random.Next(1, 4) == 1)
            {
                await demoClient.DeleteNote(note.Title);
                note.Delete();
            }
            if (random.Next(1, 4) == 1)
                await demoClient.DeleteAccount();

            if (random.Next(1, 5) == 1)
            {
                await demoClient.SignUp("root@test.com", "admin");
                await demoClient.LogIn("root@test.com", "admin");
                await demoClient.PostNote(note);
            }
        }
    }
    
    static async Task Main()
    {
        /*
        var email = $"{new Random().Next(100000, 1000000)}@test.com";
        const string password = "password123";
*/
        var journalServer = new Server("127.0.0.1", 9600, true);
        journalServer.Start();
        Console.ReadLine();
        var traffic1 = new Thread(() => SimulateTraffic());
        var traffic2 = new Thread(() => SimulateTraffic());
        var traffic3 = new Thread(() => SimulateTraffic());
        var traffic4 = new Thread(() => SimulateTraffic());
        traffic1.Start();
        traffic2.Start();
        traffic3.Start();
        traffic4.Start();
        Console.WriteLine("ALL TRAFFIC THREADS STARTED");
        Console.ReadLine();
        
        journalServer.Stop();
    }
}