using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public class DcqlService(IAgentProvider agentProvider,
    IMdocStorage mdocStorage,
    ISdJwtVcHolderService sdJwtVcHolderService) : IDcqlService
{
    public async Task<Option<IEnumerable<PresentationCandidate>>> FindCandidates(DcqlQuery query)
    {
        var candidates = await FindCandidates(
            query.CredentialQueries);

        var list = candidates.ToList();

        return list.Count == 0
            ? Option<IEnumerable<PresentationCandidate>>.None
            : list;
    }

    public async Task<Option<PresentationCandidate>> FindCandidates(CredentialQuery credentialQuery)
    {
        var candidates = await FindCandidates(
            [credentialQuery]);

        var candidatesList = candidates.ToList();

        return candidatesList.Count == 0
            ? Option<PresentationCandidate>.None
            : candidatesList.First();
    }

    public AuthorizationResponse CreateAuthorizationResponse(AuthorizationRequest authorizationRequest,
        (string Identifier, string Presentation, Format Format)[] presentationMap)
    {
        var vpToken = presentationMap.ToDictionary(
            presentationItem => presentationItem.Identifier,
            presentationItem => presentationItem.Presentation);

        return new AuthorizationResponse
        {
            VpToken = JsonConvert.SerializeObject(vpToken),
            State = authorizationRequest.State
        };
    }

    private async Task<IEnumerable<PresentationCandidate>> FindCandidates(CredentialQuery[] credentialQueries)
    {
        var candidates = new List<PresentationCandidate>();
        
        foreach (var credentialQuery in credentialQueries)
        {
            var matchingCredentialsOption = await GetMatchingCredentials(credentialQuery);
            
            matchingCredentialsOption.IfSome(matchingCredentials =>
            {
                var credentialSetGroups = 
                    matchingCredentials.GroupBy(x => x.GetCredentialSetId());
                
                var credentialSetCandidates = credentialSetGroups
                    .Select(group =>
                    {
                        var credentialSetId = group.Key;
                        var credentials = group.First();
                        return new CredentialSetCandidate(credentialSetId, [credentials]);
                    });
            
                var credentialCandidates = new PresentationCandidate(
                    credentialQuery.Id,
                    credentialSetCandidates);
            
                candidates.Add(credentialCandidates);
            });
        }

        return candidates;
    }

    private async Task<Option<IEnumerable<ICredential>>> GetMatchingCredentials(
        CredentialQuery credentialQuery)
    {
        var candidates = new List<ICredential>();
        
        switch (credentialQuery.Format)
        {
            case Constants.SdJwtFormat:
                return await GetMatchingSdJwtCredentials(credentialQuery);
            case Constants.MdocFormat:
                return await GetMatchingMdocCredentials(credentialQuery);
        }

        return candidates;
    }

    private async Task<Option<IEnumerable<ICredential>>> GetMatchingSdJwtCredentials(
        CredentialQuery credentialQuery)
    {
        var context = await agentProvider.GetContextAsync();
        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context);
        return sdJwtRecords.Where(record =>
        { 
            var doc = record.ToSdJwtDoc();
            
            return (credentialQuery.Meta?.Vcts?.Any(vct => record.Vct == vct) ?? true)
                 && (credentialQuery.Claims?.All(requestedClaim =>
                 {
                     var claimPath = ClaimPath.ValidClaimPath(requestedClaim.Path);
                     return claimPath.Match(
                         path =>
                         {
                             try
                             {
                                 var jsonPath = path.ToJsonPath();

                                 var value = doc.UnsecuredPayload.SelectToken(jsonPath.Value, true)!;

                                 if (requestedClaim.Values != null)
                                 {
                                     return requestedClaim.Values!.Any(requestedValue =>
                                         requestedValue == value.ToString());
                                 }

                                 return true;
                             }
                             catch (Exception)
                             {
                                 return false;
                             }
                             
                         },
                         _ => false);
                 }) ?? true);
        }).Cast<ICredential>().AsOption();
    }
    
    private async Task<Option<IEnumerable<ICredential>>> GetMatchingMdocCredentials(
        CredentialQuery credentialQuery)
    {
        var mdocRecords = await mdocStorage.ListAll();
        return mdocRecords.OnSome(records => records.Where(record =>
        {
            return (credentialQuery.Meta?.Doctype == null || credentialQuery.Meta?.Doctype == record.DocType)
                   && (credentialQuery.Claims?.All(requestedClaim =>
                   {
                       var claimPath = ClaimPath.ValidClaimPath(requestedClaim.Path);
                       return claimPath.Match(
                           path => record.Mdoc.IssuerSigned.IssuerNameSpaces.Value.First().Value
                               .Select(x => x.ElementId.Value).Contains(path.ToJsonPath()),
                           _ => false);
                   }) ?? true);
        }).Cast<ICredential>());
    }
}
