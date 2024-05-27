namespace JournalNetCode.ServerSide;

public sealed class ClientCollection
{
    private static readonly List<ClientInterface> ClientInterfaces = [];
    public readonly Action<ClientInterface> AppendClient = client => ClientInterfaces.Add(client);
    public readonly Action<ClientInterface> RemoveClient = client => ClientInterfaces.Remove(client);
    public readonly Func<int, ClientInterface> GetClientAt = index => ClientInterfaces[index];
    public readonly Func<ClientInterface, bool> Contains = client => ClientInterfaces.Contains(client);
}