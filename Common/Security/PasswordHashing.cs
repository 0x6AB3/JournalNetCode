using System.Security.Cryptography;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;
using Konscious.Security.Cryptography;

namespace JournalNetCode.Common.Security;

public class PasswordHashing // Argon2 is used as the key derivation function
{
    private readonly int _iterations;
    private const int OutputLength = 32; // (bytes) 256-bit key/hash
    
    public PasswordHashing(int iterations = 50) // todo change default iterations to a more cryptographically secure quantity
    {
        _iterations = iterations;
    }
    
    public bool CompareAuthHash(byte[] stored, byte[] received, byte[] salt)
    {
        var prepared = DeriveHash(received, salt);
        return stored.SequenceEqual(prepared);
    }

    // The authentication hash is sent to the server by the client
    // The encryption key may be used to encrypt/decrypt notes
    public (byte[] authenticationHash, byte[] encryptionKey) GetAuthHash(string password, string email)
    {
        var passwordBytes = Cast.StringToBytes(password);
        var encryptionKey = GetEncryptionKey(password, email);
        var authenticationHash = DeriveHash(encryptionKey, passwordBytes);
        return (authenticationHash, encryptionKey);
    }
    
    public byte[] GetEncryptionKey(string password, string email) // Used to generate AES GCM key for note security
    {
        var passwordBytes = Cast.StringToBytes(password);
        var emailBytes = Cast.StringToBytes(email);
        var encryptionKey = DeriveHash(passwordBytes, emailBytes);
        return encryptionKey;
    }

    public byte[] DeriveHash(byte[] plaintextBytes, byte[] saltBytes)
    {
        var argon2 = new Argon2id(plaintextBytes)
        {
            Salt = saltBytes,
            DegreeOfParallelism = 4, // 4 threads
            Iterations = _iterations,
            MemorySize = 512 // 512 MB
        };
        
        var hash = argon2.GetBytes(OutputLength);
        return hash;
    }
    
    public static byte[] GenerateSalt(int length)
    {
        var salt = new byte[length];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }
}