namespace TMS.Application
{
    internal static class ProjectAuditHelper
    {
        internal static Guid? TryParseAuditUserId(string userId) =>
            Guid.TryParse(userId, out var guid) ? guid : null;
    }
}
