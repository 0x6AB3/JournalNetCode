using JournalNetCode.Common.Communication.Containers;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.ServerSide.Database;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleLogIn(LoginDetails? loginDetails)
    {
        if (loginDetails == null ) 
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
}