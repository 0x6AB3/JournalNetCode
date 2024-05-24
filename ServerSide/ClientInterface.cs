using System.Text;
using System.Net;

namespace JournalNetCode.ServerSide;

public class ClientInterface
{
    private IPEndPoint _endPoint;
    private byte[] _messageOut;
    private readonly Func<string, byte[]> _stringToByte = plaintext => Encoding.UTF8.GetBytes(plaintext);

    public ClientInterface(HttpListenerRequest request, string message)
    {
        _endPoint = request.RemoteEndPoint;
        _messageOut = _stringToByte(message);
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