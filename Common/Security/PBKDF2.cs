using System.Security.Cryptography;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Security;

// PBKDF2 employing SHA3 with 64 byte hash
public class Pbkdf2
{
    private int Iterations { get; set; } = 10^5; // default: 100,000 iterations
    private readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA3_512; // Can't be const as '_algorithm' is assigned at runtime
    private const int OutputLength = 512; // (bits) SHA3-512 output hash length
    public byte[] GetHash(string base64Password, string base64Salt)
    {
        return GetHash(Cast.Base64ToBytes(base64Password), Cast.Base64ToBytes(base64Salt));
    }
    
    private byte[] GetHash(byte[] password, byte[] salt)
    {
        return Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, _algorithm, OutputLength);
    }
}