using JournalNetCode.Common.Communication;
using JournalNetCode.Common.Communication.Types;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class RequestHandler
{
    public static ServerResponse HandleSignup(LoginDetails? loginDetails)
    {
        // TODO CHECK IN DATABASE BEFORE HERE
        ServerResponse response;
        return loginDetails == null ? 
            new ServerResponse() { ResponseType = ServerResponseType.Error } : 
            new ServerResponse() { ResponseType = ServerResponseType.Success };
    }
}