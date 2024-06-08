using JournalNetCode.Common.Communication;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;
using JournalNetCode.ServerSide.Database;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleSignup(LoginDetails? loginDetails)
    {
        if (loginDetails == null)
        {
            return new ServerResponse() { Body = "Please supply login details", ResponseType = ServerResponseType.Error };
        }
        
        // Short-circuit evaluation <3
        if (!DatabaseHandler.AccountExists(loginDetails.Email) && DatabaseHandler.SignUp(loginDetails.Email, loginDetails.AuthHashB64, loginDetails.AuthSaltB64))
        {
            return new ServerResponse() { Body = "Account created", ResponseType = ServerResponseType.Success };
        }
        else
        {
            return new ServerResponse() { Body = "Account with this email already exists", ResponseType = ServerResponseType.Negative };
        }
    }
}