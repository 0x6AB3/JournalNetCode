using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Communication.Containers;

public class CommunicationContainer
{
    public string? Body { get; set; }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }
}