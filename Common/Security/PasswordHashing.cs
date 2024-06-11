using System.Security.Cryptography;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;
using Konscious.Security.Cryptography;

namespace JournalNetCode.Common.Security;

// Argon2
public class PasswordHashing
{
    private readonly int _iterations;
    private const int OutputLength = 32; // (bytes) output hash length
    
    public PasswordHashing(int iterations = 10^5)
    {
        _iterations = iterations;
    }

    public byte[] PrepareAuthForStorage(byte[] authHash, byte[] salt)
    {
        var saltedAuthHash = DeriveHash(authHash, salt);
        return saltedAuthHash;
    }

    public byte[] GetAuthHash(byte[] password, byte[] email, out byte[] encryptionKey)
    {
        encryptionKey = GetEncryptionKeyB64(password, email);
        var authenticationHash = DeriveHash(encryptionKey, password);
        return authenticationHash;
    }
    
    private byte[] GetEncryptionKeyB64(byte[] password, byte[] email)
    {
        var encryptionKey = DeriveHash(password, email);
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