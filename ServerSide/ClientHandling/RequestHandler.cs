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
            return new ServerResponse(ServerResponseType.InvalidParameters, "Please supply login details");

        if (!DatabaseHandler.AccountExists(loginDetails.Email))
            return new ServerResponse(ServerResponseType.AccountExistenceError, $"Account doesn't exist for {loginDetails.Email}");
        
        return DatabaseHandler.LogIn(loginDetails.Email, loginDetails.AuthHashB64)
            ? new ServerResponse(ServerResponseType.Success, $"Successful login to {loginDetails.Email}")
            : new ServerResponse(ServerResponseType.InvalidLogin, "Unable to log in, please ensure the password is correct");
    }
    
    public static ServerResponse HandleSignUp(LoginDetails? loginDetails)
    {
        if (loginDetails == null)
            return new ServerResponse(ServerResponseType.InvalidParameters, "Please supply login details");
        
        return !DatabaseHandler.AccountExists(loginDetails.Email) && DatabaseHandler.SignUp(loginDetails.Email, loginDetails.AuthHashB64)
            ? new ServerResponse(ServerResponseType.Success, $"Account created for {loginDetails.Email}")
            : new ServerResponse(ServerResponseType.AccountExistenceError, $"Account for {loginDetails.Email} already exists");
    }

    public static ServerResponse LoginStatus(string? email, string endPoint)
    {
        return Validate.EmailAddress(email)
            ? new ServerResponse(ServerResponseType.Success, $"You are logged in to {email}")
            : new ServerResponse(ServerResponseType.InvalidPrivileges, $"You are not logged in to an account {endPoint}");
    }

    public static ServerResponse PostNote(Note? note, string email)
    {
        if (note == null) 
            return new ServerResponse(ServerResponseType.InvalidParameters, "Invalid note structure");
        
        return DatabaseHandler.PostNote(note, email)
            ? new ServerResponse(ServerResponseType.Success, "Note successfully uploaded")
            : new ServerResponse(ServerResponseType.ServersideError, "The server is unable to save this note");
    }

    public static ServerResponse GetNote(string? title, string? email)
    {
        if (email == null)
            return new ServerResponse(ServerResponseType.InvalidPrivileges, "You are not logged in to an account");
        if (title == null)
            return new ServerResponse(ServerResponseType.InvalidParameters, "Please include the title of the requested note");
        
        var noteJson = DatabaseHandler.GetNoteJson(email, title);

        return noteJson != null
            ? new ServerResponse(ServerResponseType.Success, noteJson)
            : new ServerResponse(ServerResponseType.ServersideError, "Unable to retrieve note");
    }

    public static ServerResponse GetNoteTitles(string? email)
    {
        if (email == null)
            return new ServerResponse(ServerResponseType.InvalidPrivileges, "You are not logged in to an account");

        var titles = DatabaseHandler.GetNoteTitles(email);

        return titles != null
            ? new ServerResponse(ServerResponseType.Success, titles) // todo (check note todo)
            : new ServerResponse(ServerResponseType.ServersideError, "Unable to retrieve note titles");
    }

    public static ServerResponse DeleteNote(string? title, string? email)
    {
        if (email == null)
            return new ServerResponse(ServerResponseType.InvalidPrivileges, "You are not logged in to an account");
        
        if (title == null)
            return new ServerResponse(ServerResponseType.InvalidParameters, "Please provide a title of the target note");
        
        return DatabaseHandler.DeleteNote(email, title)
            ? new ServerResponse(ServerResponseType.Success, "Delete success")
            : new ServerResponse(ServerResponseType.ServersideError, "Unable to delete this note"); // todo (check txt note)
    }
    
    public static ServerResponse DeleteAccount(string? email)
    {
        if (email == null)
            return new ServerResponse(ServerResponseType.InvalidPrivileges, "You are not logged in to an account");
         
        return DatabaseHandler.DeleteAccount(email)
            ? new ServerResponse(ServerResponseType.Success, "Your account has been deleted")
            : new ServerResponse(ServerResponseType.ServersideError, "Unable to delete your account");
    }
}