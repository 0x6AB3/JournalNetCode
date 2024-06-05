using JournalNetCode.Common.Communication.Types;

namespace JournalNetCode.Common.Communication;

public sealed class ClientRequest : CommunicationObject
{
    public ClientRequestType RequestType { get; set; }
}