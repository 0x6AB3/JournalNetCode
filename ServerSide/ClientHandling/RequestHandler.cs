using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Database;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleLogIn(LoginDetails? loginDetails)
    {
        if (loginDetails == null) 
            return new ServerResponse() { Body = "Please supply login details", ResponseType = ServerResponseType.Failure };

        if (!DatabaseHandler.AccountExists(loginDetails.Email))
            return new ServerResponse() { Body = $"Account doesn't exist for {loginDetails.Email}", ResponseType = ServerResponseType.Failure };
        
        return DatabaseHandler.LogIn(loginDetails.Email, loginDetails.AuthHashB64)
            ? new ServerResponse() { Body = $"Successful login to {loginDetails.Email}", ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = "Unable to log in, please ensure the password is correct", ResponseType = ServerResponseType.Failure };
    }
    
    public static ServerResponse HandleSignUp(LoginDetails? loginDetails)
    {
        if (loginDetails == null)
            return new ServerResponse() { Body = "Please supply login details", ResponseType = ServerResponseType.Failure };
        
        return !DatabaseHandler.AccountExists(loginDetails.Email) && DatabaseHandler.SignUp(loginDetails.Email, loginDetails.AuthHashB64)
            ? new ServerResponse() { Body = $"Account created for {loginDetails.Email}", ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = $"Account for {loginDetails.Email} already exists", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse LoginStatus(string? email, string endPoint)
    {
        return Validate.EmailAddress(email)
            ? new ServerResponse() { Body = $"You are logged in to {email}", ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = $"You are not logged in to an account {endPoint}", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse PostNote(Note? note, string email)
    {
        if (note == null) 
            return new ServerResponse() { Body = "Invalid note structure", ResponseType = ServerResponseType.Failure };
        
        return DatabaseHandler.PostNote(note, email)
            ? new ServerResponse() { Body = "Note successfully uploaded", ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = "The server is unable to save this note", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse GetNote(string? title, string? email)
    {
        if (email == null)
            return new ServerResponse() { Body = $"You are not logged in to an account", ResponseType = ServerResponseType.Failure };
        if (title == null)
            return new ServerResponse() { Body = $"Please include the title of the requested note", ResponseType = ServerResponseType.Failure };
        
        var noteJson = DatabaseHandler.GetNoteJson(email, title);

        return noteJson != null
            ? new ServerResponse() { Body = noteJson, ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = $"Unable to retrieve note", ResponseType = ServerResponseType.Failure };
    }

    public static ServerResponse GetNoteTitles(string? email)
    {
        if (email == null)
            return new ServerResponse() { Body = $"You are not logged in to an account", ResponseType = ServerResponseType.Failure };

        var titles = DatabaseHandler.GetNoteTitles(email);

        return titles != null
            ? new ServerResponse() { Body = titles, ResponseType = ServerResponseType.Success }
            : new ServerResponse() { Body = $"Unable to retrieve note titles", ResponseType = ServerResponseType.Failure };
    }
}