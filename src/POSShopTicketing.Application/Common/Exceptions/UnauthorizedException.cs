namespace POSShopTicketing.Application.Common.Exceptions;

/// <summary>Authentication failures (bad credentials, expired/invalid
/// refresh token, inactive account) - mapped to 401.</summary>
public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
