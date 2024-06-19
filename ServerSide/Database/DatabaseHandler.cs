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
            // Once the server receives an authentication hash from a client, a random salt is applied to and stored alongside the new hash
            // This is done to prevent precomputed hash attacks because a random salt is not used before this step
            var authHashB64 = reader.GetString(0);
            var storedAuthHash = Cast.Base64ToBytes(authHashB64);
            var authSaltB64 = reader.GetString(1);
            var storedAuthSalt = Cast.Base64ToBytes(authSaltB64);

            var authHashToCompare = Cast.Base64ToBytes(receivedAuthHashB64);
            
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
    public static bool SignUp(string email, string receivedAuthHashB64) // Auth hash to be stored (sent by client)
    {
        const string action = "INSERT INTO tblAccounts (Email, AuthHashB64, AuthSaltB64) " +
                              "VALUES (@email, @authHashB64, @authSaltB64)";

        var receivedAuthHash = Cast.Base64ToBytes(receivedAuthHashB64);
        var salt = PasswordHashing.GenerateSalt(16);
        var saltB64 = Cast.BytesToBase64(salt);
        
        // Hashing the authentication hash with a random salt to prevent rainbow table attacks
        // Random salts are not used before this step (email is used as a unique salt for encryption key and auth hash)
        var hashingAlgorithm = new PasswordHashing();
        var authHashPrepared = hashingAlgorithm.DeriveHash(receivedAuthHash, salt);
        var authHashPreparedB64 = Cast.BytesToBase64(authHashPrepared);

        var connection = GetConnection();
        using var command = new SqliteCommand(action, connection);
        command.Parameters.AddWithValue("@email", email);
        command.Parameters.AddWithValue("@authHashB64", authHashPreparedB64);
        command.Parameters.AddWithValue("@authSaltB64", saltB64);
        
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