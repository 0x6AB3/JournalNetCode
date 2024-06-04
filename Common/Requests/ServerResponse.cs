using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Requests;

public sealed class ServerResponse : CommunicationObject
{
    public ServerResponseType ResponseType { get; set; }
}