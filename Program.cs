using System.Diagnostics;
using System.Security.Cryptography;
using JournalNetCode.ServerSide;
using JournalNetCode.ClientSide;
using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode;

// Demo entry point
class Program
{
    static async Task Main()
    {
        /*
        var email = $"{new Random().Next(100000, 1000000)}@test.com";
        const string password = "password123";
*/
        var journalServer = new Server("127.0.0.1", 9600, true);
        journalServer.Start();

        // Example account and note creation
        var email = "t@t.com";
        var password = "testtest";
        var argon = new PasswordHashing();
        var encryptionKey = argon.GetEncryptionKey(password, email);

        var client = new Client("127.0.0.1", 9600); // Login + note title retrieval test
        (var loginSuccess, var message) = await client.LogIn(email, password);
        if (loginSuccess) // todo create signup alternative if login fails (check for serverresponsetype)
        {
            for (var i = 0; i < 3; i++)
            {
                var note = new Note($"{i + 1}");
                note.SetText($"Generated content GUID:\t{Guid.NewGuid().ToString()}", encryptionKey);
                var postResponse = await client.PostNote(note);
            }

            var retrievedNoteTitles = await client.GetNoteTitles();
            var titlesNewLine = "";
            foreach (var noteTitle in retrievedNoteTitles)
            {
                titlesNewLine += $"{noteTitle}\n";
            }

            titlesNewLine = titlesNewLine.Trim('\n');
            var titlesDebugMessage = "";
            titlesDebugMessage = titlesNewLine.Length == 0
                ? "No titles were retrieved"
                : $"Client retrieved note titles:\n{titlesNewLine}";
            Logger.AppendDebug(titlesDebugMessage);

            var titles = await client.GetNoteTitles();
            if (titles == null)
            {
                Logger.AppendDebug("No notes to retrieve");
            }
            else
            {
                foreach (var title in titles)
                {
                    (var note, var serverMessage) = await client.GetNote(title);
                    if (note != null)
                    {
                        Logger.AppendDebug($"Retrieved note {title}");
                        var success = await client.DeleteNote(title);
                        if (success)
                        {
                            Logger.AppendDebug($"Deleted note {title} from server");
                        }
                        else
                        {
                            Logger.AppendError($"Unable to delete note {title} from server");
                        }
                    }
                    else
                    {
                        Logger.AppendError($"Unable to retrieve note {title}");
                    }
                }
            }

            if (Console.ReadLine() == "x")
            {
                Logger.AppendWarn("Exit requested...");
                journalServer.Stop();
            }
        }
    }
}