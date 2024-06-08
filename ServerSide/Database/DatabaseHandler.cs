using System.Collections.Specialized;
using Microsoft.Data.Sqlite;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.Database;

public static class DatabaseHandler // Parameterised sql
{
    private static string? DBPath { get; set; } // Reconsider this if class is used in multithreaded env
    private static void GetPath()
    {
        if (DBPath != null)
        {
            return;
        }
        
        const string dbFileName = "JournalDB.db";
        const string targetDir = "JournalNetCode"; // Needs to be a parent directory
        var dir = Directory.GetCurrentDirectory();

        try
        {
            var end = dir.IndexOf(targetDir) + targetDir.Length;
            DBPath = "Data Source=" + Path.Combine(dir[0..end], dbFileName);
        }
        catch (Exception ex)
        {
            Logger.AppendError("Error: Cannot locate SQLite database file",
                "Ensure JournalDB.db is located in JournalNetCode/");
            throw new Exception("Unable to locate SQLite database");
        }
    }
    
    // Return 'true' on success
    public static void AddNote()
    {
        GetPath();
    }
    public static void GetNote()
    {
        GetPath();
    }
    public static void LogIn()
    {
        GetPath();
    }
    public static bool SignUp(string email, string authHashB64, string authSaltB64)
    {
        const string action = "INSERT INTO tblAccounts (Email, AuthHashB64, AuthSaltB64) " +
                              "VALUES (@email, @authHashB64, @authSaltB64)";
        
        GetPath();
        using var connection = new SqliteConnection(DBPath);
        connection.Open();
        
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@authHashB64", authHashB64);
        command.Parameters.AddWithValue("@authSaltB64", authSaltB64);
        
        var success = false;
        try
        {
            success = command.ExecuteNonQuery() > 0;
        }
        catch (SqliteException ex)
        {
            Logger.AppendError($"SQL error during signup", ex.Message);
        }
        
        connection.Close();
        return success;
    }
    public static bool AccountExists(string email)
    {
        const string action = "SELECT Email FROM tblAccounts " +
                              "WHERE Email = @email";
        
        GetPath();
        using var connection = new SqliteConnection(DBPath);
        connection.Open();
        
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);
        
        using var reader = command.ExecuteReader();
        bool exists = reader.Read() && reader.GetString(0) == email;
        
        connection.Close();
        return exists;
    }
}