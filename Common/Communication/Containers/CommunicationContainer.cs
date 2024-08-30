using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Communication.Containers;

public class CommunicationContainer
{
    public string? Body { get; set; }

    public CommunicationContainer(string? body = null)
    {
        Body = body;
    }
    
    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }
}