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
        Logger.ToFile();
        Console.WriteLine("SERVER DESTRUCTOR END");
    }
    
    public Server(string ipAddress = "127.0.0.1", int port = 80)
    {
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
        Logger.AppendMessage("Starting thread...");
        _processThread.Start();
        Logger.AppendMessage("Thread started");
    }

    public void Stop()
    {
        _processCancelTokenSrc.Cancel(); // Graceful shutdown
    }

    private async Task ProcessRequests(CancellationToken cancelToken)
    {
        Logger.AppendMessage("THREAD STARTING LISTENER...");
        _listener.Start();
        Logger.AppendMessage("THREAD LISTENER STARTED");
        
        while (!cancelToken.IsCancellationRequested)
        {
            try
            {
                const string message = "<HTML><BODY><p>Test1, Test2, Test3, end.</p></BODY></HTML>";

                Logger.AppendMessage("THREAD WAITING FOR CONTEXT...");
                var context = await _listener.GetContextAsync();
                
                Logger.AppendMessage("THREAD CONTEXT RECEIVED");

                var newClient = new ClientInterface(context.Request, message);
                newClient.SendResponse(context.Response);
                ClientCollection.AddClient(newClient);

                Logger.AppendMessage("THREAD RESPONSE SENT OUT");
            }
            catch (Exception ex)
            {
                Logger.AppendMessage($"THREAD HTTP server error: {ex.Message}");
            }
        }
        
        _listener.Stop();
    }
}