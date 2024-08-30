namespace JournalNetCode.Common.Communication.Types;

public enum ServerResponseType
{
    Success,
    AccountExistenceError,
    InvalidPrivileges,
    NoteDeletionError,
    InvalidParameters,
    InvalidLogin,
    ServersideError,
    InvalidRequest, // used in Client.cs outside of RequestHandler (client request json checks)
    NullResponse // Used only by client during null check
}