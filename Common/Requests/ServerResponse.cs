using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Requests;

public class ServerResponse : CommunicationObject
{
    public ServerResponseType ResponseType { get; set; }
}