using JournalNetCode.Common.Communication.Types;

namespace JournalNetCode.Common.Communication;

public sealed class ServerResponse : CommunicationObject
{
    public ServerResponseType ResponseType { get; set; }
}