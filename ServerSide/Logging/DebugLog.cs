namespace JournalNetCode.ServerSide.Logging;

public class DebugLog : Log
{
    public DebugLog(string message) : base(message)
    {
        Message = $"DEBUG: {message}";
    }
}