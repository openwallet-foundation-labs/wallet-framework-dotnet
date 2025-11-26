using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record CandidateQueryResult(
    Option<List<PresentationCandidateSet>> Candidates,
    Option<List<CredentialQuery>> MissingCredentials);
