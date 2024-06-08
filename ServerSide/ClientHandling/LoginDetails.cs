using System.Text.Json.Serialization;
using JournalNetCode.Common.Security;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.ServerSide.ClientHandling;

public sealed class LoginDetails
{
    [JsonInclude]
    public string Email { get; private set; }
    [JsonInclude]
    public string AuthHashB64 { get; private set; }
    [JsonInclude]
    public string AuthSaltB64 { get; private set; }
    
    [JsonConstructor]
    public LoginDetails(string email, string authHashB64, string authSaltB64)
    {
        Email = email;
        AuthHashB64 = authHashB64;
        AuthSaltB64 = authSaltB64;
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
        AuthHashB64 = hashingAlgorithm.GetBase64Hash(Cast.StringToBytes(password), out var saltTemp);
        AuthSaltB64 = saltTemp;
    }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }

    public override string ToString()
    {
        return $"Email: {Email}\n" +
               $"Authentication hash: {AuthHashB64}\n" +
               $"Salt: {AuthSaltB64}\n";
    }
}