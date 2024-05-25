using System.Net;
using JournalNetCode.Common.;

namespace JournalNetCode.ServerSide;

public class ClientInterface
{
    private IPEndPoint _endPoint;
    private readonly byte[] _messageOut;

    public ClientInterface(HttpListenerRequest request, string message)
    {
        _endPoint = request.RemoteEndPoint;
        _messageOut = StringToBytes.Convert(message);
    }

    public void ProcessRequest(HttpListenerResponse response)
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