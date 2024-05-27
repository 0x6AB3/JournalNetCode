using System.Text;

namespace JournalNetCode.Common.Utility;

// UTF-8 encoding
public static class Cast
{
    
    private static readonly Encoding EncodingType = Encoding.UTF8;
    public static readonly Func<string, byte[]> StringToBytes = plaintext => EncodingType.GetBytes(plaintext);
    public static readonly Func<byte[], string> BytesToString = bytes => EncodingType.GetString(bytes);
    public static readonly Func<string, byte[]> Base64ToBytes = base64 => Convert.FromBase64String(base64);
    public static readonly Func<byte[], string> BytesToBase64 = bytes => Convert.ToBase64String(bytes);
}