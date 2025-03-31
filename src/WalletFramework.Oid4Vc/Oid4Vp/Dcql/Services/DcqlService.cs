using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Path;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public class DcqlService(IAgentProvider agentProvider,
    IMdocStorage mdocStorage,
    ISdJwtVcHolderService sdJwtVcHolderService) : IDcqlService
{
    public async Task<Option<IEnumerable<PresentationCandidate>>> FindPresentationCandidatesAsync(DcqlQuery query)
    {
        var candidates = await FindCandidates(
            query.CredentialQueries);

        var list = candidates.ToList();

        return list.Count == 0
            ? Option<IEnumerable<PresentationCandidate>>.None
            : list;
    }

    public async Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(CredentialQuery credentialQuery)
    {
        var candidates = await FindCandidates(
            [credentialQuery]);

        var candidatesList = candidates.ToList();

        return candidatesList.Count == 0
            ? Option<PresentationCandidate>.None
            : candidatesList.First();
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
                candidates.AddRange(await GetMatchingSdJwtCredentials(credentialQuery));
                break;
            case Constants.MdocFormat:
                candidates.AddRange(await GetMatchingMdocCredentials(credentialQuery));
                break;
        }

        return candidates.Any() 
            ? candidates 
            : Option<IEnumerable<ICredential>>.None;
    }

    private async Task<IEnumerable<ICredential>> GetMatchingSdJwtCredentials(
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
                     var claimPath = ClaimPath.ValidClaimPath(requestedClaim.Path!);
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
        });
    }
    
    private async Task<IEnumerable<ICredential>> GetMatchingMdocCredentials(
        CredentialQuery credentialQuery)
    {
        var mdocRecords = await mdocStorage.ListAll();
        return mdocRecords.Match(
            records => records.Where(record =>
            {
                return (credentialQuery.Meta?.Doctype == null || credentialQuery.Meta?.Doctype == record.DocType)
                       && (credentialQuery.Claims?.All(requestedClaim =>
                       {
                           // backward compatible Draft 24 & Draft 23
                           var nameSpace = requestedClaim.Path?[0] ?? requestedClaim.Namespace;
                           var claimName = requestedClaim.Path?[1] ?? requestedClaim.ClaimName;
                           
                           return record.Mdoc.IssuerSigned.IssuerNameSpaces.Value
                               .Any(x => x.Key == nameSpace
                                         && x.Value.Select(value => value.ElementId.Value).Contains(claimName)
                                         && x.Value.Select(value => value.Element.Value)
                                             .Any(value => value.Match(
                                                 elementValue => requestedClaim.Values?.Contains(elementValue.Value) ?? true,
                                                 elementArray => false,
                                                 elementMap => false)));
                       }) ?? true);
            }),
            () => []);
    }
}
