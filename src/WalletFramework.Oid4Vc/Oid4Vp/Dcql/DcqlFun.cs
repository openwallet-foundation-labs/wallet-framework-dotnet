using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql;

internal static class DcqlFun
{
    internal static Option<IEnumerable<PresentationCandidate>> FindMatchingCandidates(
        this DcqlQuery query,
        IEnumerable<SdJwtRecord> sdJwts)
    {
        if (query.CredentialQueries.Length == 0)
            return Option<IEnumerable<PresentationCandidate>>.None;

        return query.CredentialQueries
            .TraverseAll(credentialQuery => credentialQuery.FindMatchingCandidate(sdJwts))
            .ToOption();
    }
    
    internal static Option<IEnumerable<PresentationCandidate>> FindMatchingCandidates(
        this DcqlQuery query,
        IEnumerable<MdocRecord> mdocs)
    {
        if (query.CredentialQueries.Length == 0)
            return Option<IEnumerable<PresentationCandidate>>.None;

        return query.CredentialQueries
            .TraverseAll(credentialQuery => credentialQuery.FindMatchingCandidate(mdocs))
            .ToOption();
    }
}
