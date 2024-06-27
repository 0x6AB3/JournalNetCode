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
            var end = dir.IndexOf(targetDir) + targetDir.Length; // Project parent directory (e.g: /test/example/parent)
            DBPath = "Data Source=" + Path.Combine(dir[0..end], dbFileName); // Merging parent directory and db file (e.g: /test/example/parent/database.db)
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

    public static string? GetGuid(string email)
    {
        const string action = "SELECT GUID FROM tblAccounts " +
                              "WHERE Email = @email";
        
        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);

        string? guid = null;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            guid = reader.GetString(0);
        }
        
        DisposeConnection(connection);
        return guid;
    }
    
    public static bool LogIn(string email, string b64ReceivedAuthHash) // Auth hash sent by client (before hashing for storage)
    {
        const string action = "SELECT B64AuthHash, B64Salt FROM tblAccounts" +
                              " WHERE Email = @email";
        
        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);

        var success = false;
        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            // Once the server receives an authentication hash from a client, a random salt is applied to and stored alongside the new hash
            // This is done to prevent precomputed hash attacks because a random salt is not used before this step
            var b64AuthHash = reader.GetString(0);
            var storedAuthHash = Cast.Base64ToBytes(b64AuthHash);
            var b64Salt = reader.GetString(1);
            var storedAuthSalt = Cast.Base64ToBytes(b64Salt);

            var authHashToCompare = Cast.Base64ToBytes(b64ReceivedAuthHash);
            
            var hashingAlgorithm = new PasswordHashing();
            success = hashingAlgorithm.CompareAuthHash(storedAuthHash, authHashToCompare, storedAuthSalt);
        }
        else
        {
            throw new Exception();
        }
        
        DisposeConnection(connection);
        return success;
    }
    public static bool SignUp(string email, string b64ReceivedAuthHash) // Auth hash to be stored (sent by client)
    {
        const string action = "INSERT INTO tblAccounts (GUID, Email, B64AuthHash, B64Salt) " +
                              "VALUES (@guid, @email, @b64AuthHash, @b64Salt)";

        var guid = Guid.NewGuid().ToString();
        
        var receivedAuthHash = Cast.Base64ToBytes(b64ReceivedAuthHash);
        var salt = PasswordHashing.GenerateSalt(16);
        var b64Salt = Cast.BytesToBase64(salt);
        
        // Hashing the authentication hash with a random salt to prevent rainbow table attacks
        // Random salts are not used before this step (email is used as a unique salt for encryption key and auth hash)
        var hashingAlgorithm = new PasswordHashing();
        var authHashPrepared = hashingAlgorithm.DeriveHash(receivedAuthHash, salt);
        var b64AuthHashPrepared = Cast.BytesToBase64(authHashPrepared);

        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@guid", guid);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@b64AuthHash", b64AuthHashPrepared);
        command.Parameters.AddWithValue("@b64Salt", b64Salt);
        
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