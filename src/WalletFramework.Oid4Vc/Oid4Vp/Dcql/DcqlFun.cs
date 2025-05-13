using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql;

internal static class DcqlFun
{
    internal static Option<IEnumerable<PresentationCandidate>> FindMatchingCandidates(
        this DcqlQuery query,
        IEnumerable<ICredential> credentials)
    {
        if (query.CredentialQueries.Length == 0)
            return Option<IEnumerable<PresentationCandidate>>.None;

        return query.CredentialQueries
            .TraverseAll(credentialQuery => credentialQuery.FindMatchingCandidate(credentials))
            .ToOption();
    }
}
