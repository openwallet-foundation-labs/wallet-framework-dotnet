using WalletFramework.SdJwtLib.Models;

namespace WalletFramework.SdJwtLib.Roles;

public interface IIssuer
{
    public string Issue(List<Claim> claims, string issuerJwk);
}
