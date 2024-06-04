using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Requests;

public class CommunicationObject
{
    public string? Body { get; set; }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }
}