using System.Net;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Requests;

public sealed class ClientRequest : CommunicationObject
{
    public ClientRequestType RequestType { get; set; }
}