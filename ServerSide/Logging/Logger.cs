﻿namespace JournalNetCode.ServerSide.Logging;

public static class Logger
{
    private static readonly List<Log> Logs = [];
    private static readonly Action<Log> AddLog = log => Logs.Add(log);
    
    public static void ToFile(string directory = "./Logs/")
    {
        var filename = $"{DateTime.Now.ToString("yyyy-MM-dd_HHmm")}.log";
        var path = directory + filename;
        using var writeStream = File.CreateText(path);
        foreach (var log in Logs)
        {
            writeStream.WriteLine(log.ToString());
        }
        writeStream.WriteLine($"Concluded at {DateTime.Now}");
        Console.WriteLine($"Logs --> {path}");
    }

    public static void AppendMessage(string message)
    {
        AddLog(new Log(message));
    }
}