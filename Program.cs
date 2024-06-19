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
        var password = "password123";
        /*
        var hashing = new PasswordHashing();
        var (authHash1, encryptionKey1) = hashing.GetAuthHash(email, password);
        var (authHash2, encryptionKey2) = hashing.GetAuthHash(email, password);
        var salt = PasswordHashing.GenerateSalt(16);
        var hashPrep1 = hashing.DeriveHash(authHash1, salt);
        var hashPrep2 = hashing.DeriveHash(authHash2, salt);
        Console.WriteLine(hashing.CompareAuthHash(hashPrep1, authHash2, salt));
        */
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
        journalServer.Stop();
/*
        Console.Write("Email address: ");
        var email = Console.ReadLine();

        Console.Write("Password: ");
        var password = Console.ReadLine();

        Console.WriteLine("Generating encryption key...");
        var hashingAlgorithm = new PasswordHashing();
        var encryptionKey = hashingAlgorithm.GetEncryptionKey("password", "name@email.com");
        Console.WriteLine($"Generated encryption key: {Cast.BytesToBase64(encryptionKey)}");

        Console.Write("Note title: ");
        var note = new Note(Console.ReadLine());
        Console.WriteLine($"Note creation at = {note.TimeOfCreation}");
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
        Console.WriteLine($"Creation date = {recoveredNote.TimeOfCreation}");
        Console.WriteLine($"Decrypted text: {recoveredNote.GetText(encryptionKey)}");

        /*
        for (var i = 0; i < 100; i++)
        {
            var demoClient = new Client("127.0.0.1", 9600);
            var random = new Random();
            var id = random.Next(100000, 1000000);
            var email = $"{id}@gmail.com";
            var password = $"{id}";
            await demoClient.SignUp(email, password);
            await demoClient.LogIn(email, password);
        }
        journalServer.Stop();*/
    }
}
