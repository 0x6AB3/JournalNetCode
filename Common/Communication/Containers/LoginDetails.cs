using System.ComponentModel.Design.Serialization;
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
        var passwordBytes = Cast.StringToBytes(password);
        var emailBytes = Cast.StringToBytes(emailAddress);
        var authHashBytes = hashingAlgorithm.GetAuthHash(passwordBytes, emailBytes, out encryptionKey);
        AuthHashB64 = Cast.BytesToBase64(authHashBytes);
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