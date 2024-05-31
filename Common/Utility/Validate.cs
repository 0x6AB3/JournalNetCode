using System.Net.Mail;

namespace JournalNetCode.Common.Utility;

public static class Validate
{
    public static readonly Func<string, bool> EmailAddress = emailAddress => MailAddress.TryCreate(emailAddress, out _);
}