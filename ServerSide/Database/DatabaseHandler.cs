using System.Collections.Specialized;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;
using Microsoft.Data.Sqlite;
using JournalNetCode.ServerSide.Logging;

namespace JournalNetCode.ServerSide.Database;

public static class DatabaseHandler // Parameterised sql
{
    private static string? DBPath { get; set; } // Reconsider this if class is used in multithreaded env
    private static void GetPath()
    {
        // Skip if DBPath already found
        if (DBPath != null)
        {
            return;
        }
        
        const string dbFileName = "JournalDB.db";
        const string targetDir = "JournalNetCode"; // The project parent directory
        var dir = Directory.GetCurrentDirectory();

        try
        {
            var end = dir.IndexOf(targetDir) + targetDir.Length; // Parent directory (e.g: /test/example/parent)
            DBPath = "Data Source=" + Path.Combine(dir[0..end], dbFileName); // Merging parent directory and db file
        }
        catch (Exception ex)
        {
            Logger.AppendError("Error: Cannot locate SQLite database file",
                $"Ensure JournalDB.db is located in JournalNetCode/\n{ex.Message}");
            throw new Exception("Unable to locate SQLite database");
        }
    }

    private static SqliteConnection GetConnection()
    {
        GetPath();
        var connection = new SqliteConnection(DBPath);
        connection.Open();
        return connection;
    }

    private static void DisposeConnection(SqliteConnection connection)
    {
        connection.Close();
        connection.Dispose();
    }
    
    // Paramaterised SQL is used to prevent SQL attacks
    // All below return 'true' on success
    public static void AddNote()
    {
        GetPath();
    }
    public static void GetNote()
    {
        GetPath();
    }
    public static bool LogIn(string email, string receivedAuthHashB64) // Auth hash sent by client (before hashing for storage)
    {
        const string action = "SELECT AuthHashB64, AuthSaltB64 FROM tblAccounts " +
                              "WHERE Email = @email";
        
        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);

        var success = false;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            var storedAuthHashB64 = reader.GetString(0);
            var storedAuthSalt = reader.GetString(1);
            
            var hashingAlgorithm = new PasswordHashing();
            var generatedAuthHashBytes = hashingAlgorithm.PrepareAuthForStorage(storedAuthHashB64, storedAuthSalt);
            var generatedAuthHash = Cast.BytesToBase64(generatedAuthHashBytes);
            
            success = generatedAuthHash == storedAuthHashB64;
        }
        else
        {
            throw new Exception();
        }
        
        DisposeConnection(connection);
        return success;
    }
    public static bool SignUp(string email, string receivedAuthHashB64) // Auth hash to be stored (sent by client)
    {
        const string action = "INSERT INTO tblAccounts (Email, AuthHashB64, AuthSaltB64) " +
                              "VALUES (@email, @authHashB64, @authSaltB64)";

        // Preparing data for storage
        var generatedSaltBytes = new byte[16]; // 16 byte salt
        RandomNumberGenerator.Fill(generatedSaltBytes);
        var generatedSalt = Cast.BytesToBase64(generatedSaltBytes);
        
        // Hashing the authentication hash with a random salt to prevent rainbow table attacks
        // Random salts are not used before this step (email is used as a unique salt for encryption key and auth hash)
        var hashingAlgorithm = new PasswordHashing();
        var authHashToStoreBytes = hashingAlgorithm.PrepareAuthForStorage(receivedAuthHashB64, generatedSalt);
        var storedAuthHashB64 = Cast.BytesToBase64(authHashToStoreBytes);
        var storedSaltB64 = Cast.BytesToBase64(generatedSaltBytes);

        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@authHashB64", storedAuthHashB64);
        command.Parameters.AddWithValue("@authSaltB64", storedSaltB64);
        
        var success = false;
        try
        {
            success = command.ExecuteNonQuery() > 0; // Checks if any rows have been changed (true if inserted)
        }
        catch (SqliteException ex)
        {
            Logger.AppendError($"SQL error during signup", ex.Message);
        }

        DisposeConnection(connection);
        return success;
    }
    public static bool AccountExists(string email)
    {
        const string action = "SELECT Email FROM tblAccounts " +
                              "WHERE Email = @email";
        
        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);
        
        using var reader = command.ExecuteReader();
        var exists = reader.Read() && reader.GetString(0) == email;
        
        DisposeConnection(connection);
        return exists;
    }
}