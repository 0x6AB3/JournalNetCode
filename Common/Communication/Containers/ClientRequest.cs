using JournalNetCode.Common.Communication.Types;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class ClientRequest : CommunicationContainer
{
    public ClientRequestType RequestType { get; set; }
}