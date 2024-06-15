using System.Security.Cryptography;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;
using Konscious.Security.Cryptography;

namespace JournalNetCode.Common.Security;

// Argon2
public class PasswordHashing
{
    private readonly int _iterations;
    private const int OutputLength = 32; // (bytes) 256-bit key/hash
    
    public PasswordHashing(int iterations = 10^5)
    {
        _iterations = iterations;
    }

    public byte[] PrepareAuthForStorage(byte[] authHash, byte[] salt)
    {
        var saltedAuthHash = DeriveHash(authHash, salt);
        return saltedAuthHash;
    }

    public (byte[] authenticationHash, byte[] encryptionKey) GetAuthHash(string password, string email)
    {
        var passwordBytes = Cast.StringToBytes(password);
        var encryptionKey = GetEncryptionKey(password, email);
        var authenticationHash = DeriveHash(encryptionKey, passwordBytes);
        return (authenticationHash, encryptionKey);
    }
    
    public byte[] GetEncryptionKey(string password, string email)
    {
        var passwordBytes = Cast.StringToBytes(password);
        var emailBytes = Cast.StringToBytes(email);
        var encryptionKey = DeriveHash(passwordBytes, emailBytes);
        return encryptionKey;
    }

    private byte[] DeriveHash(byte[] plaintextBytes, byte[] saltBytes)
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
}