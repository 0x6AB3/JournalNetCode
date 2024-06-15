using System.Text.Json.Serialization;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class LoginDetails
{
    [JsonInclude]
    public string Email { get; private set; }
    [JsonInclude]
    public string AuthHashB64 { get; private set; }

    [JsonConstructor]
    public LoginDetails(string email, string authHashB64)
    {
        Email = email;
        AuthHashB64 = authHashB64;
    }
    
    public LoginDetails(string emailAddress, string password, out byte[] encryptionKey)
    {
        if (!Validate.EmailAddress(emailAddress))
        {
            throw new ArgumentException("Invalid email address");
        }
        
        Email = emailAddress;
        
        var hashingAlgorithm = new PasswordHashing();
        var (authHashBytes, encryptionKeyBytes) = hashingAlgorithm.GetAuthHash(password, emailAddress);
        AuthHashB64 = Cast.BytesToBase64(authHashBytes);
        encryptionKey = encryptionKeyBytes;
    }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }

    public override string ToString()
    {
        return $"Email: {Email}\n" +
               $"Authentication hash: {AuthHashB64}\n";
    }
}