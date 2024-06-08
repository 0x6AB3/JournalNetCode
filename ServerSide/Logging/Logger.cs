namespace JournalNetCode.ServerSide.Logging;

public static class Logger
{
    public static bool ConsoleOutput = false;
    private static readonly List<Log> Logs = [];
    private static readonly Action<Log> AddLog = log => Logs.Add(log);
    
    public static void ToFile(string? directory = null)
    {
        directory ??= Directory.GetCurrentDirectory() + "/";
        var filename = $"{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.log";
        var path = directory + filename;
        using var fileStream = File.CreateText(path);
        foreach (var log in Logs)
        {
            fileStream.WriteLine(log.ToString());
        }
        fileStream.WriteLine($"Concluded at {DateTime.Now}");
        Console.WriteLine($"Logs --> {path}");
    }

    public static void AppendMessage(string message)
    {
        var log = new Log(message);
        PrintLog(log);
    }

    public static void AppendError(string message, string addendum)
    {
        var error = new ErrorLog(message, addendum);
        PrintLog(error);
    }

    public static void AppendWarn(string message)
    {
        var warn = new WarnLog(message);
        PrintLog(warn);
    }

    private static void PrintLog(Log log)
    {
        AddLog(log);
        
        if (ConsoleOutput)
        {
            Console.WriteLine(log.ToString());
        }
    }
}