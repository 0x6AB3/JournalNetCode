using System.Security.Cryptography;
using JournalNetCode.Common.Utility;
using JournalNetCode.ServerSide.Logging;
using Konscious.Security.Cryptography;

namespace JournalNetCode.Common.Security;

// Argon2
public class PasswordHashing
{
    private readonly int _iterations;
    private readonly byte[] _salt;
    private const int OutputLength = 32; // (bits) output hash length
    
    private readonly Action<byte[]> _generateSalt = saltBytes => RandomNumberGenerator.Fill(saltBytes);
    
    public PasswordHashing(int iterations = 10^5, int saltLengthBytes = 16)
    {
        _salt = new byte[saltLengthBytes];
        _iterations = iterations;
    }
    
    // [HASH/SALT] output string
    public string GetBase64Hash(byte[] password)
    {
        DateTime start = DateTime.Now;
        _generateSalt(_salt);
        
        var argon2 = new Argon2id(password)
        {
            Salt = _salt,
            DegreeOfParallelism = 4, // 4 threads
            Iterations = _iterations,
            MemorySize = 512 // 512 MB
        };
        
        byte[] hash = argon2.GetBytes(OutputLength);
        
        DateTime finish = DateTime.Now;
        Logger.AppendMessage($"Took {(finish-start).TotalSeconds}s to hash password");
        return $"{Cast.BytesToBase64(hash)}/{Cast.BytesToBase64(_salt)}";
    }
}