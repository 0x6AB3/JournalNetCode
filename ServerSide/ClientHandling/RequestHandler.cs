using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Database;
using System.Text.Json;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleLogIn(LoginDetails? loginDetails)
    {
        if (loginDetails == null) 
            return new ServerResponse() { Body = "Please supply login details", ResponseType = ServerResponseType.Failure };

        if (!DatabaseHandler.AccountExists(loginDetails.Email))
            return new ServerResponse() { Body = $"Account doesn't exist for {loginDetails.Email}", ResponseType = ServerResponseType.Failure };
        
        if (DatabaseHandler.LogIn(loginDetails.Email, loginDetails.AuthHashB64))
            return new ServerResponse() { Body = $"Successful login to {loginDetails.Email}", ResponseType = ServerResponseType.Success };
        
        return new ServerResponse() { Body = "Unable to log in, please ensure the password is correct", ResponseType = ServerResponseType.Failure };
    }
    
    public static ServerResponse HandleSignUp(LoginDetails? loginDetails)
    {
        if (loginDetails == null)
            return new ServerResponse() { Body = "Please supply login details", ResponseType = ServerResponseType.Failure };
        
        if (!DatabaseHandler.AccountExists(loginDetails.Email) && DatabaseHandler.SignUp(loginDetails.Email, loginDetails.AuthHashB64))
            return new ServerResponse() { Body = $"Account created for {loginDetails.Email}", ResponseType = ServerResponseType.Success };
        
        return new ServerResponse() { Body = $"Account for {loginDetails.Email} already exists", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse GetLoggedIn(string? email, string endPoint)
    {
        if (Validate.EmailAddress(email))
            return new ServerResponse() { Body = $"You are logged in to {email}", ResponseType = ServerResponseType.Success };
        
        return new ServerResponse() { Body = $"You are not logged in to an account {endPoint}", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse PostNote(string noteJson, string email)
    {
        var note = JsonSerializer.Deserialize<Note>(noteJson);
        if (note == null) 
            return new ServerResponse() { Body = "Invalid note structure", ResponseType = ServerResponseType.Failure };
        
        var guid = DatabaseHandler.GetGuid(email);
        var dir = Directory.GetCurrentDirectory() + $"/Notes/{guid}";
        Directory.CreateDirectory(dir);
        File.WriteAllText($"{dir}/{note.Title}.Json", note.Serialise());
        return new ServerResponse() { Body = "Note successfully uploaded", ResponseType = ServerResponseType.Success };
    }
}