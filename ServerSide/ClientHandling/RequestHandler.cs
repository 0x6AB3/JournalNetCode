using JournalNetCode.Common.Requests;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleSignup(LoginDetails? loginDetails)
    {
        // TODO CHECK IN DATABASE BEFORE HERE
        ServerResponse response;
        if (loginDetails == null)
        {
            return new ServerResponse() { ResponseType = ServerResponseType.Error };
        }
        
        return new ServerResponse() { ResponseType = ServerResponseType.Success };
    }

    public static bool HandleSignUp()
    {
        // TODO implement this
        return false;
    }
}