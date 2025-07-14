using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using static LanguageExt.Option<WalletFramework.Oid4Vc.Oid4Vp.Models.PresentationCandidate>;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public class DcqlService(
    IAgentProvider agentProvider,
    IMdocStorage mdocStorage,
    ISdJwtVcHolderService sdJwtVcHolderService) : IDcqlService
{
    public async Task<CandidateQueryResult> Query(DcqlQuery query)
    {
        var context = await agentProvider.GetContextAsync();
        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context);
        var mdocsOption = await mdocStorage.ListAll();
        var mdocs = mdocsOption.ToNullable() ?? [];
        
        var credentials = sdJwtRecords.Cast<ICredential>().Concat(mdocs);
        return query.ProcessWith(credentials);
    }

    public async Task<Option<PresentationCandidate>> QuerySingle(CredentialQuery query)
    {
        var context = await agentProvider.GetContextAsync();
        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context);
        var sdJwtCandidate = query.FindMatchingCandidate(sdJwtRecords);

        var mdocRecords = await mdocStorage.ListAll();
        var mdocCandidate = from record in mdocRecords
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
