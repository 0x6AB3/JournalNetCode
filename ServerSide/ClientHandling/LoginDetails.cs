using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ServerSide.ClientHandling;

public sealed class LoginDetails
{
    public string Email { get; set; }
    public string PasswordHashBase64 { get; set; }
    public string SaltBase64 { get; set; }

    public LoginDetails(string emailAddress, string password)
    {
        if (!Validate.EmailAddress(emailAddress))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        Email = emailAddress;
        var hashingAlgorithm = new PasswordHashing();
        var hashAndSalt = hashingAlgorithm.GetBase64Hash(Cast.StringToBytes(password));
        PasswordHashBase64 = hashAndSalt.Split("/")[0];
        SaltBase64 = hashAndSalt.Split("/")[1];
    }

    public override string ToString()
    {
        return $"Email: {Email}\n" +
               $"Password: {PasswordHashBase64}\n" +
               $"Salt: {SaltBase64}\n";
    }
}