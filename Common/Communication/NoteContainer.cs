namespace JournalNetCode.Common.Communication;

public sealed class NoteContainer
{
    private bool _passwordProtected;
    private byte[] _encryptedContent;
    private byte[] _encryptionKeySalt;

}