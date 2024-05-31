using System.Net;
using JournalNetCode.ServerSide.ClientHandling;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide;

public class Server
{
    private readonly HttpListener _listener;
    private readonly Thread _processThread;
    private readonly CancellationTokenSource _processCancelTokenSrc;
    
    ~Server()
    {
        Stop();
    }
    
    public Server(string ipAddress = "127.0.0.1", int port = 80, bool printLogs = false)
    {
        Logger.ConsoleOutput = printLogs;
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{ipAddress}:{port}/");
        Logger.AppendMessage($"SERVER LISTENING AT http://{ipAddress}:{port}/");
        
        _processCancelTokenSrc = new CancellationTokenSource();
        _processThread = new Thread(() => ProcessRequests(_processCancelTokenSrc.Token));
    }
    
    public void Start()
    {/*                         ADD CERTIFICATE
        X509Certificate2 certificate = new X509Certificate2(
            "Localhost-Certificate.pfx",
            "PGCT2024",
            X509KeyStorageFlags.MachineKeySet);

        _listener.AuthenticationSchemes = AuthenticationSchemes.Digest;
        _listener.*/
        _processThread.Start();
    }

    public void Stop()
    {
        _processCancelTokenSrc.Cancel(); // Graceful shutdown
        Logger.ToFile(); // Saving logs to file
    }

    private async Task ProcessRequests(CancellationToken cancelToken)
    {
        Logger.AppendMessage("STARTING LISTENER...");
        _listener.Start();
        Logger.AppendMessage("LISTENER STARTED");
        
        while (!cancelToken.IsCancellationRequested)
        {
            try
            {
                Logger.AppendMessage("WAITING FOR CONTEXT");
                var context = await _listener.GetContextAsync();
                Logger.AppendMessage("CONTEXT RECEIVED");

                var client = new ClientInterface(context);
                ClientCollection.AddClient(client);

                Logger.AppendMessage("RESPONSE SENT OUT");
            }
            catch (Exception ex)
            {
                Logger.AppendMessage($"Listen thread error: {ex.Message}");
            }
        }
        
        _listener.Stop();
    }
}