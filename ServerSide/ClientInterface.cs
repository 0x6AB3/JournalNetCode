using System.Net;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ServerSide;

public class ClientInterface
{
    private IPEndPoint _endPoint;
    private readonly byte[] _messageOut;

    public ClientInterface(HttpListenerRequest request, string message)
    {
        _endPoint = request.RemoteEndPoint;
        _messageOut = Cast.StringToBytes(message);
    }

    public void GetRequest()
    {
        
    }

    public void SendResponse(HttpListenerResponse response)
    {
        response.ContentType = "text/html";
        response.StatusDescription = "OK";
        response.StatusCode = (int)HttpStatusCode.OK;
        response.ContentLength64 = _messageOut.LongLength;
        response.OutputStream.Write(_messageOut);
        response.OutputStream.Close();
        response.Close();
    }
}