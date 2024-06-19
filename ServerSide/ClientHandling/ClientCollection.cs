using System.Net;

namespace JournalNetCode.ServerSide.ClientHandling;

public static class ClientCollection
{
    private static readonly List<ClientInterface> ClientInterfaces = [];
    public static readonly Action<ClientInterface> AddClient = client => ClientInterfaces.Add(client);
    public static readonly Action<ClientInterface> RemoveClient = client => ClientInterfaces.Remove(client);
    public static readonly Func<int, ClientInterface> GetClientAt = index => ClientInterfaces[index];

    public static ClientInterface? GetClient(IPEndPoint endPoint) // null if not found
    {
        var client = ClientInterfaces.FirstOrDefault(connected => connected.remoteEndPoint.Equals(endPoint));
        return client;
    }
}