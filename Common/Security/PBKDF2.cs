using System.Security.Cryptography;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Security;

// PBKDF2 employing SHA3 with 64 byte hash
public class Pbkdf2
{
    private readonly int _iterations;
    private readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA3_512;
    private const int OutputLength = 512; // (bits) SHA3-512 output hash length
    
    public Pbkdf2(int iterations = 10^5)
    {
        _iterations = iterations;
    }
    
    public byte[] GetHash(string base64Password, string base64Salt)
    {
        return GetHash(Cast.Base64ToBytes(base64Password), Cast.Base64ToBytes(base64Salt));
    }
    
    private byte[] GetHash(byte[] password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, _algorithm, OutputLength);
    }
}