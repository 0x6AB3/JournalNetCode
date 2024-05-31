namespace JournalNetCode.ServerSide.Logging;

public class Log
{
    private readonly string _message;
    private readonly DateTime _time;
    
    public Log(string message)
    {
        _message = message;
        _time = DateTime.Now;
    }

    public override string ToString()
    {
        return $"LOG: [{_time}]\t{_message}";
    }
}