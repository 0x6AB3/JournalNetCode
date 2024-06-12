using JournalNetCode.Common.Communication.Types;
using JournalNetCode.ServerSide.ClientHandling;
using JournalNetCode.ServerSide.Logging;
using System.Text.Json;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class ClientRequest : CommunicationContainer
{
    public ClientRequestType RequestType { get; set; }

    public bool TryGetLoginDetails(out LoginDetails? loginDetails)
    {
        if (Body != null)
        {
            try
            {
                loginDetails = JsonSerializer.Deserialize<LoginDetails>(Body);
                return true; // success
            }
            catch (JsonException ex)
            {
                Logger.AppendError("Error while deserialising LoginDetails JSON", ex.Message);
            }
        }
        loginDetails = null;
        return false;
    }
}