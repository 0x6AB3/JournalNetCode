using System.Text.Json.Serialization;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ServerSide.ClientHandling;

public sealed class LoginDetails
{
    public string Email { get; private set; } // set for deserialisation
    public string PasswordHashB64 { get; private set; }
    public string SaltB64 { get; private set; }

    [JsonConstructor]
    public LoginDetails(string emailAddress, string passwordHashBase64, string saltBase64)
    {
        Email = emailAddress;
        PasswordHashB64 = passwordHashBase64;
        SaltB64 = saltBase64;
    }
    
    public LoginDetails(string emailAddress, string password)
    {
        if (!Validate.EmailAddress(emailAddress))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        Email = emailAddress;
        
        var hashingAlgorithm = new PasswordHashing();
        
        // Really confused why I can't just 'out SaltB64' but this will do
        PasswordHashB64 = hashingAlgorithm.GetBase64Hash(Cast.StringToBytes(password), out var saltTemp);
        SaltB64 = saltTemp;
    }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }

    public override string ToString()
    {
        return $"Email: {Email}\n" +
               $"Password: {PasswordHashB64}\n" +
               $"Salt: {SaltB64}\n";
    }
}