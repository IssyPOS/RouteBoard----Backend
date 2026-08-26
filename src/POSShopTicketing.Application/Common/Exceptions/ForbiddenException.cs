namespace POSShopTicketing.Application.Common.Exceptions;

/// <summary>The caller is authenticated but not allowed to perform this
/// specific action - e.g. a Manager trying to touch another team's
/// routing rules, or a PlatformSuperAdmin trying to read ticket content.
/// Mapped to 403, distinct from UnauthorizedException's 401.</summary>
public class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
