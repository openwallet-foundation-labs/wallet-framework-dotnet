using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Query;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record CandidateQueryResult(
    Option<List<PresentationCandidate>> Candidates,
    Option<List<CredentialRequirement>> MissingCredentials)
{
    public static CandidateQueryResult FromDcqlQuery(DcqlQuery query, IEnumerable<ICredential> credentials)
    {
        var pairs = query.CredentialQueries
            .Select(q => (Query: q, Candidate: q.FindMatchingCandidate(credentials)))
            .ToList();

        var candidates = pairs
            .Choose(x => x.Candidate)
            .ToList();

        var missing = pairs
            .Where(x => x.Candidate.IsNone)
            .Select(x => new CredentialRequirement(x.Query))
            .ToList();

        return new CandidateQueryResult(
            candidates.Count > 0 ? candidates : Option<List<PresentationCandidate>>.None,
            missing.Count > 0 ? missing : Option<List<CredentialRequirement>>.None
        );
    }
} 
