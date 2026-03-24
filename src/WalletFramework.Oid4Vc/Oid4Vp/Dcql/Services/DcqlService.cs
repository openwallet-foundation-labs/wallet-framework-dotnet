using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using static LanguageExt.Option<WalletFramework.Oid4Vc.Oid4Vp.Models.PresentationCandidate>;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Core.Credentials;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

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

    public async Task<Option<PresentationCandidate>> QuerySingle(CredentialQuery query)
    {
        var sdJwtRecords = await sdJwtCredentialStore.List();
        var sdJwtCandidate = query.FindMatchingCandidate(sdJwtRecords);

        var mdocRecords = await mdocCredentialStore.List();
        var mdocCandidate = query.FindMatchingCandidate(mdocRecords);

        return sdJwtCandidate.Match(Some, () => mdocCandidate);
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
