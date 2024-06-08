namespace JournalNetCode.ServerSide.Logging;

public sealed class WarnLog : Log
{
    public WarnLog(string message) : base(message)
    {
        Message += $"<-- Warning";
    }
}