using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using static LanguageExt.Option<WalletFramework.Oid4Vc.Oid4Vp.Models.PresentationCandidate>;
using WalletFramework.Storage;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Core.Credentials;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public class DcqlService(
    IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId> mdocCredentialRepository,
    IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> sdJwtCredentialRepository) : IDcqlService
{
    public async Task<CandidateQueryResult> Query(DcqlQuery query)
    {
        var sdJwtRecords = await sdJwtCredentialRepository.ListAll();
        var mdocsOption = await mdocCredentialRepository.ListAll();

        var sdJwts = sdJwtRecords.ToNullable() ?? [];
        var mdocs = mdocsOption.ToNullable() ?? [];
        
        var credentials = sdJwts.Cast<ICredential>().Concat(mdocs);
        return query.ProcessWith(credentials);
    }

    public async Task<Option<PresentationCandidate>> QuerySingle(CredentialQuery query)
    {
        var sdJwtRecords = await sdJwtCredentialRepository.ListAll();
        var sdJwtCandidate = 
            from record in sdJwtRecords
            from candidate in query.FindMatchingCandidate(record)
            select candidate;

        var mdocRecords = await mdocCredentialRepository.ListAll();
        var mdocCandidate = 
            from record in mdocRecords
            from candidate in query.FindMatchingCandidate(record)
            select candidate;

        return sdJwtCandidate.Match(Some, () => mdocCandidate);
    }

    public AuthorizationResponse CreateAuthorizationResponse(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        var dcqlVpToken = DcqlVpTokenFun.FromPresentationMaps(presentationMaps);
        var vpToken = new VpToken(dcqlVpToken);
        
        return new AuthorizationResponse
        {
            VpToken = vpToken,
            State = authorizationRequest.State
        };
    }
}
