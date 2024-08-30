using System.Text.Json.Serialization;
using JournalNetCode.Common.Communication.Types;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class ServerResponse : CommunicationContainer
{
    public ServerResponseType ResponseType { get; private set; }

    [JsonConstructor]
    public ServerResponse(ServerResponseType ResponseType, string? Body = null)
    {
        this.ResponseType = ResponseType;
        this.Body = Body;
    }
}