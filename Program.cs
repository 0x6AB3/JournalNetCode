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
            var id = random.Next(100000, 1000000);
            var email = $"{id}@gmail.com";
            var password = $"{id}";
            var note = new Note($"{id.ToString()} Note");
            var textBytes = new byte[1024];
            RandomNumberGenerator.Fill(textBytes);
            var text = Cast.BytesToBase64(textBytes);
            var encryptionKey = argon.GetEncryptionKey(password, email);
            note.SetText(text, encryptionKey);
            await demoClient.SignUp(email, password);
            demoClient = new Client("127.0.0.1", 9600); // Connecting from a new endpoint
            await demoClient.LogIn(email, password);
            await demoClient.PostNote(note);
            demoClient = new Client("127.0.0.1", 9600); // Connecting from a new endpoint
            await demoClient.LogIn(email, password);
            await demoClient.GetNote(note.Title);
            await demoClient.DeleteNote(note.Title);
            await demoClient.DeleteAccount();
            await demoClient.GetLoggedIn();
            note.Delete();
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
        Console.WriteLine("BOTH TRAFFIC THREADS STARTED");
        Console.ReadLine();
        
        /*
        var client = new Client("127.0.0.1", 9600);
        await client.GetLoggedIn(); // not logged in yet
        await client.LogIn(email, password); // account doesn't exist
        await client.GetLoggedIn(); // not logged in yet
        await client.SignUp(email, password); // account created (hopefully)
        await client.GetLoggedIn(); // logged in
        await client.LogIn(email, password + " wrong noise"); // incorrect password
        await client.GetLoggedIn(); // logged in (because of signup)
        await client.LogIn(email, password); // success
        await client.GetLoggedIn(); // logged in
        
        // Creating a note and sending it to the server
        Console.WriteLine("Generating encryption key...");
        var hashingAlgorithm = new PasswordHashing();
        var encryptionKey = hashingAlgorithm.GetEncryptionKey(password, email);
        Console.WriteLine($"Generated encryption key: {Cast.BytesToBase64(encryptionKey)}");

        Console.Write("Note title: ");
        var note = new Note(Console.ReadLine());
        Console.WriteLine($"Note created on {note.LastModified}");
        Console.Write("Note content: ");
        note.SetText(Console.ReadLine(), encryptionKey);

        Console.WriteLine($"Writing to Notes/{note.Title}.json");
        var path = note.ToFile();

        Console.WriteLine($"Reading from {path} ...");
        var content = File.ReadAllText(path);

        Console.WriteLine($"Deserialising: {content} ...");
        var recoveredNote = JsonSerializer.Deserialize<Note>(content);

        Console.WriteLine($"Deserialised note:");
        Console.WriteLine(recoveredNote.ToString());
        Console.WriteLine($"Decrypted text: {recoveredNote.GetText(encryptionKey)}");

        Console.WriteLine($"Sending note to server...");
        if (await client.PostNote(recoveredNote))
            Console.WriteLine($"success!");
        else
            Console.WriteLine($"failure.");
        
        
        Console.WriteLine("Deleting note locally...");
        File.Delete(path); // deleting the note
        Console.WriteLine("Deleted!");
        
        Console.WriteLine("Retrieving note titles:");
        var titles = await client.GetNoteTitles();
        for (var i = 0; i < titles.Length; i++)
        {
            Console.WriteLine($"{i+1}\t{titles[i]}");
        }

        if (titles.Contains(note.Title) && await client.GetNote(note.Title))
        {
            Console.WriteLine($"Retrieved note: {note.Title}");
        }
        Console.WriteLine("Deserialising note...");   
        var serverNote = JsonSerializer.Deserialize<Note>( File.ReadAllText(path));
        Console.WriteLine("Deserialised!");
        Console.WriteLine(serverNote.ToString());
        Console.WriteLine($"Decrypted text: {serverNote.GetText(encryptionKey)}");
        
        Console.WriteLine("Requesting server to delete their copy of the note");
        var success = await client.DeleteNote(note.Title);
        Console.WriteLine($"Success: {success}");
        
        Console.WriteLine("Requesting server to delete the account");
        success = await client.DeleteAccount();
        Console.WriteLine($"Success: {success}");
        await client.GetLoggedIn(); // not logged in
        */
        // add graceful logout
        journalServer.Stop();
    }
}
/* flood demo
        for (var i = 0; i < 100; i++)
        {
            var demoClient = new Client("127.0.0.1", 9600);
            var random = new Random();
            var id = random.Next(100000, 1000000);
            var email = $"{id}@gmail.com";
            var password = $"{id}";
            await demoClient.SignUp(email, password);
            await demoClient.LogIn(email, password);
        }*/