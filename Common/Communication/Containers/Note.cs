using System.Text.Json.Serialization;
using System.Security.Cryptography;
using JournalNetCode.Common.Utility;

namespace JournalNetCode.Common.Communication.Containers;

public sealed class Note // AES GCM with 256-bit key used for encryption/decryption of notes (key is passed, never stored)
{
    [JsonInclude]
    public string Title { get; private set; }
    [JsonInclude]
    public byte[] InternalData { get; private set; }
    [JsonInclude]
    public byte[] InitVector { get; private set; }
    [JsonInclude]
    public byte[] SecurityTag { get; private set; }
    [JsonInclude]
    public DateTime TimeOfCreation { get; private set; }
    
    [JsonConstructor]
    public Note(string title, byte[] internalData, byte[] initVector, byte[] securityTag, DateTime timeOfCreation)
    {
        Title = title;
        InternalData = internalData;
        InitVector = initVector;
        SecurityTag = securityTag;
        TimeOfCreation = timeOfCreation;
    }
    
    public Note(string title = "New note")
    {
        Title = title;
        InternalData = new byte[1];
        InitVector = new byte[AesGcm.NonceByteSizes.MaxSize];
        SecurityTag = new byte[AesGcm.TagByteSizes.MaxSize];
        TimeOfCreation = DateTime.Now;
    }

    public void SetText(string plaintext, byte[] encryptionKey) 
    {
        RandomNumberGenerator.Fill(InitVector); // generating new iv/nonce (reusing poses security risk)
        var plaintextBytes = Cast.StringToBytes(plaintext); // encoding plaintext
        InternalData = new byte[plaintextBytes.Length]; // preparing to hold ciphertext
        using var encryptor = new AesGcm(encryptionKey, SecurityTag.Length);
        encryptor.Encrypt(InitVector, plaintextBytes, InternalData, SecurityTag);
    }

    public string GetText(byte[] encryptionKey)
    {
        var plaintextBytes = new byte[InternalData.Length];
        using var decryptor = new AesGcm(encryptionKey, SecurityTag.Length);
        decryptor.Decrypt(InitVector, InternalData, SecurityTag, plaintextBytes);
        var plaintext = Cast.BytesToString(plaintextBytes);
        return plaintext;
    }

    public string ToFile() // will overwrite the file // todo check for modifications, modified data, etc
    {
        var path = Path.Join(Directory.GetCurrentDirectory(), $"Notes/{Title}.json");
        Directory.CreateDirectory("./Notes");
        File.WriteAllText(path, Serialise());
        return path;
    }

    public string Serialise()
    {
        return Cast.ObjectToJson(this);
    }
}