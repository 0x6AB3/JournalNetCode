namespace JournalNetCode.ServerSide.Logging;

public sealed class ErrorLog : Log
{
    public ErrorLog(string message, string addendum = "None") : base(message)
    {
        Message += $"\nAdditional information: {addendum}";
    }
}