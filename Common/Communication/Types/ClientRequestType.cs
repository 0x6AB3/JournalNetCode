namespace JournalNetCode.Common.Communication.Types;

public enum ClientRequestType
{
    SignUp,
    LogIn,
    PostNote,
    GetNote,
    LoginStatus,
    GetNoteTitles,
    DeleteNote,
    DeleteAccount,
    Proceed // todo implement for note overwritting in client software
}