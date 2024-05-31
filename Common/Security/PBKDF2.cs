using System.Security.Cryptography;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Security;

// PBKDF2 employing SHA3 with 64 byte hash
public class PBKDF2
{
    public int Iterations { get; set; } = 10^5; // default: 100,000 iterations
    private readonly HashAlgorithmName _algorithm = HashAlgorithmName.SHA3_512; // Can't be const as '_algorithm' is assigned at runtime
    private const int OutputLength = 512; // (bits) SHA3-512 output hash length
    private readonly byte[] _salt; //
    private readonly Action<byte[]> _generateSalt = saltBytes => RandomNumberGenerator.Fill(saltBytes);
    public PBKDF2(int saltLengthBytes = 16)
    {
        _salt = new byte[saltLengthBytes];
    }
    
    private byte[] GetHash(byte[] password, out byte[] salt)
    {
        _generateSalt(_salt);
        salt = _salt;
        return Rfc2898DeriveBytes.Pbkdf2(password, _salt, Iterations, _algorithm, OutputLength);
    }
}