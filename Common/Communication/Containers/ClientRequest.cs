using JournalNetCode.Common.Communication.Types;
using JournalNetCode.ServerSide.ClientHandling;
using JournalNetCode.ServerSide.Logging;
using System.Text.Json;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class ClientRequest : CommunicationContainer
{
    public ClientRequestType RequestType { get; set; }

    // Null check and deserialisation
    public bool TryGetLoginDetails(out LoginDetails? loginDetails)
    {
        loginDetails = null;
        if (Body == null) // Missing LoginDetails JSON string in body
        {
            return false;
        }
        
        try
        {
            loginDetails = JsonSerializer.Deserialize<LoginDetails>(Body);
            if (loginDetails != null)
                return true; // successful deserialisation
            
            Logger.AppendError("Unable to deserialise LoginDetails");
        }
        catch (JsonException ex)
        {
            Logger.AppendError("Error while deserialising LoginDetails JSON", ex.Message);
        }
        return false;
    }
}