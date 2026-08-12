using LanguageExt;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Persistence;

namespace WalletFramework.Oid4Vp.Dcql.Services;

public class DcqlService(
    IMdocCredentialStore mdocCredentialStore,
    ISdJwtCredentialStore sdJwtCredentialStore) : IDcqlService
{
    public async Task<CandidateQueryResult> Query(DcqlQuery query)
    {
        var sdJwts = await sdJwtCredentialStore.List();
        var mdocs = await mdocCredentialStore.List();
        
        var credentials = sdJwts.Cast<ICredential>().Concat(mdocs);
        return query.ProcessWith(credentials);
    }

    public async Task<Option<PresentationCandidate>> QuerySingle(CredentialQuery credentialQuery)
    {
        var sdJwtRecords = await sdJwtCredentialStore.List();
        var sdJwtCandidate = credentialQuery.FindMatchingCandidate(sdJwtRecords);

        var mdocRecords = await mdocCredentialStore.List();
        var mdocCandidate = credentialQuery.FindMatchingCandidate(mdocRecords);

        return sdJwtCandidate.Match(x => x, () => mdocCandidate);
    }

    public AuthorizationResponse CreateAuthorizationResponse(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        var vpToken = VpTokenFun.FromPresentationMaps(presentationMaps);
        
        return new AuthorizationResponse
        {
            VpToken = vpToken,
            State = authorizationRequest.State
        };
    }
}
