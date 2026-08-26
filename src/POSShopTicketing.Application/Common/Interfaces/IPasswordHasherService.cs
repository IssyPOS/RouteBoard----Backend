namespace POSShopTicketing.Application.Common.Interfaces;

public interface IPasswordHasherService
{
    string Hash(string password);
    bool Verify(string hash, string providedPassword);
}
