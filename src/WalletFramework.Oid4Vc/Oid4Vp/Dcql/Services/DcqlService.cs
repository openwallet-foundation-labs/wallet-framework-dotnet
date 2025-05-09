using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
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
    public async Task<Option<IEnumerable<PresentationCandidate>>> Query(DcqlQuery query)
    {
        var context = await agentProvider.GetContextAsync();
        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context);
        var sdJwtCandidates = query.FindMatchingCandidates(sdJwtRecords);

        var mdocRecords = await mdocStorage.ListAll();
        var mdocCandidates = 
            from record in mdocRecords
            from candidate in query.FindMatchingCandidates(record)
            select candidate;

        return from s in sdJwtCandidates
               from m in mdocCandidates
               select s.Concat(m);
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

    public AuthorizationResponse CreateAuthorizationResponse(AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        var vpToken = presentationMaps.ToDictionary(
            presentationItem => presentationItem.Identifier,
            presentationItem => presentationItem.Presentation);
        
        return new AuthorizationResponse
        {
            VpToken = JsonConvert.SerializeObject(vpToken),
            State = authorizationRequest.State
        };
    }
}
