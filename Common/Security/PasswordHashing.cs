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

    // Once server receives an authentication hash from user, a random salt is applied by the server and it is stored
    // This is done because a random salt is not used before this step and so the hashes may be precomputed by an attacker
    public byte[] PrepareAuthForStorage(string authHashToStoreB64, string saltToStoreB64)
    {
        var authHash = Cast.Base64ToBytes(authHashToStoreB64);
        var salt = Cast.Base64ToBytes(saltToStoreB64);
        var saltedAuthHash = DeriveHash(authHash, salt);
        return saltedAuthHash;
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