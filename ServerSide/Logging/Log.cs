namespace JournalNetCode.ServerSide.Logging;

public class Log
{
    protected string Message;
    private readonly DateTime _time;
    
    public Log(string message)
    {
        Message = message;
        _time = DateTime.Now;
    }

    public override string ToString()
    {
        return $"[{_time}] LOG:\t{Message}";
    }
}