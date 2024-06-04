using System.Net;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Requests;

public class ClientRequest : CommunicationObject
{
    public ClientRequestType RequestType { get; set; }
}