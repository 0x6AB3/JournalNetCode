using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ServerSide.ClientHandling;

public sealed class LoginDetails
{
    private readonly string _email;
    private readonly byte[] _passwordHash;
    private readonly byte[] _salt;

    public LoginDetails(string emailAddress, string password)
    {
        if (!Validate.EmailAddress(emailAddress))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        _email = emailAddress;
        var hashingAlgorithm = new PBKDF2();
        _passwordHash = hashingAlgorithm.GetHash(Cast.StringToBytes(password), out _salt);
    }
}