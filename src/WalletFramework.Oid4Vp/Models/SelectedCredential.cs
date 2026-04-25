using LanguageExt;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vp.TransactionDatas;

namespace WalletFramework.Oid4Vp.Models;

/// <summary>
///     Represents a credential that the Holder chose to present to the Verifier.
/// </summary>
public record SelectedCredential(
    string Identifier,
    ICredential Credential,
    Option<List<ClaimQuery>> ClaimsToDisclose,
    Option<List<TransactionData>> TransactionData);

public static class SelectedCredentialFun
{
    /// <summary>
    ///     Returns the string representations of claims to disclose for the given credential and format.
    /// </summary>
    public static IReadOnlyList<string> GetClaimsToDiscloseAsStrs(this SelectedCredential selectedCredential,
        CredentialQuery query)
    {
        return selectedCredential.ClaimsToDisclose.Match(
            claims => claims.AsStrings(query.Format),
            () => new List<string>()
        );
    }
}
