namespace POSShopTicketing.Domain.Exceptions;

/// <summary>Thrown when an operation would violate a domain invariant
/// (e.g. re-closing an already-closed ticket, a duplicate slug).</summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }
}
