using System.Net;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace JournalNetCode.ServerSide;

public class Server
{
    private readonly HttpListener _listener;
    private readonly Thread _processThread;
    private readonly CancellationTokenSource _processCancelTokenSrc;
    
    private readonly Func<string, byte[]> _stringToByte = plaintext => Encoding.UTF8.GetBytes(plaintext);
    
    ~Server()
    {
        Stop();
    }
    
    public Server(string ipAddress = "127.0.0.1", int port = 80)
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add($"http://{ipAddress}:{port}/");
        
        _processCancelTokenSrc = new CancellationTokenSource();
        _processThread = new Thread(() => ProcessRequests(_processCancelTokenSrc.Token));
    }
    
    public void Start()
    {/*
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
        //_listener.Stop();
    }

    private async Task ProcessRequests(CancellationToken cancelToken)
    {
        Console.WriteLine("THREAD: WAITING FOR CONTEXT");
        _listener.Start();
        
        while (!cancelToken.IsCancellationRequested)
        {
            try
            {
                const string messageOut = "<HTML><BODY><p>Test1, Test2, Test3, end.</p></BODY></HTML>";
                var messageBuffer = _stringToByte(messageOut);

                Console.WriteLine("THREAD: WAITING FOR CONTEXT");
                var context = await _listener.GetContextAsync();
                Console.WriteLine("THREAD: CONTEXT RECEIVED");
                var response = context.Response;
                response.StatusCode = (int)HttpStatusCode.OK;
                response.StatusDescription = "OK";
                response.ContentLength64 = messageBuffer.LongLength;
                response.ContentType = "text/html";
                response.OutputStream.Write(messageBuffer);
                response.OutputStream.Close();
                response.Close();
                Console.WriteLine("THREAD: RESPONSE SENT OUT");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"THREAD: HTTP server error: {ex.Message}");
            }
            finally
            {
                _listener.Stop();
            }
        }
        
        _listener.Stop();
    }
}