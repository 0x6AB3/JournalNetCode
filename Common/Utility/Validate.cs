using System.Net.Mail;

namespace JournalNetCode.Common.Utility;

public static class Validate
{ // TODO more objects to reduce clutter outside
    public static readonly Func<string, bool> EmailAddress = emailAddress => MailAddress.TryCreate(emailAddress, out _);
}