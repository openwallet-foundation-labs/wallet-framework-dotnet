using WalletFramework.Oid4Vc.CredentialSet.Models;

namespace WalletFramework.Oid4Vc.CredentialSet;

public static class CredentialDataSetExtensions
{
    public static bool IsExpired(this CredentialDataSet credentialDataSet) =>
        credentialDataSet.ExpiresAt.Match(
            expiresAt => expiresAt < DateTime.UtcNow,
            () => false);

    public static bool IsRevoked(this CredentialDataSet credentialDataSet) =>
        credentialDataSet.RevokedAt.Match(
            revokedAt => revokedAt < DateTime.UtcNow,
            () => false);
}
