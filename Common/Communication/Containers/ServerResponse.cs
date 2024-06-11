using JournalNetCode.Common.Communication.Types;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class ServerResponse : CommunicationContainer
{
    public ServerResponseType ResponseType { get; set; }
}