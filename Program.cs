using System.Collections.Concurrent;
using System.Text.Json;
using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        var email = $"{new Random().Next(100000, 1000000)}@test.com";
        const string password = "password123";

        var journalServer = new Server("127.0.0.1", 9600, true);
        journalServer.Start();
        Console.ReadLine();
        var client = new Client("127.0.0.1", 9600);
        await client.GetLoggedIn();
        await client.LogIn(email, password);
        await client.GetLoggedIn();
        await client.SignUp(email, password);
        await client.GetLoggedIn();
        await client.LogIn(email, password + " wrong noise");
        await client.GetLoggedIn();
        await client.LogIn(email, password);
        await client.GetLoggedIn();
        
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
        Console.WriteLine($"Title = {recoveredNote.Title}");
        Console.WriteLine($"Content = {Cast.BytesToBase64(recoveredNote.InternalData)}");
        Console.WriteLine($"IV = {Cast.BytesToBase64(recoveredNote.InitVector)}");
        Console.WriteLine($"Tag = {Cast.BytesToBase64(recoveredNote.SecurityTag)}");
        Console.WriteLine($"Last modified = {recoveredNote.LastModified}");
        Console.WriteLine($"Decrypted text: {recoveredNote.GetText(encryptionKey)}");

        Console.WriteLine($"Sending note to server...");
        if (await client.PostNote(recoveredNote))
            Console.WriteLine($"success!");
        else
            Console.WriteLine($"failure.");
        
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