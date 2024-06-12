using System.Text;
using System.Text.Json;

namespace JournalNetCode.Common.Utility;

public static class Cast
{
    private static readonly Encoding EncodingType = Encoding.UTF8; // UTF-8 encoding for string operations
    public static readonly Func<string, byte[]> StringToBytes = plaintext => EncodingType.GetBytes(plaintext);
    public static readonly Func<byte[], string> BytesToString = bytes => EncodingType.GetString(bytes);
    public static readonly Func<string, byte[]> Base64ToBytes = base64 => Convert.FromBase64String(base64);
    public static readonly Func<byte[], string> BytesToBase64 = bytes => Convert.ToBase64String(bytes);
    public static readonly Func<object, string> ObjectToJson = obj => JsonSerializer.Serialize(obj);
}