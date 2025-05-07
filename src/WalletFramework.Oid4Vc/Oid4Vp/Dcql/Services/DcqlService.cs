using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.ClaimPaths;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;

public class DcqlService(
    IAgentProvider agentProvider,
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
            case Constants.SdJwtVcFormat:
            case Constants.SdJwtDcFormat:
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

            var vctMatches = credentialQuery.Meta?.Vcts?.Any(vct => record.Vct == vct) ?? true;
            var pathSuccess = credentialQuery.Claims?.All(requestedClaim =>
            {
                return ClaimPath
                    .FromObjects([.. requestedClaim.Path!.Cast<object>()])
                    .OnSuccess(path => path.ProcessWith(doc))
                    .OnSuccess(selection =>
                    {
                        if (requestedClaim.Values != null)
                        {
                            var values = selection.GetValues().Select(v => v.ToString());
                            return requestedClaim.Values.Any(requestedValue => values.Contains(requestedValue));
                        }
                
                        return true;
                    })
                    .Fallback(false);
            }) ?? true;
            
            return vctMatches && pathSuccess;
        });
    }
    
    private async Task<IEnumerable<ICredential>> GetMatchingMdocCredentials(
        CredentialQuery credentialQuery)
    {
        var mdocRecords = await mdocStorage.ListAll();
        return mdocRecords.Match(
            records => records.Where(record =>
            {
                // Filter by doctype if specified
                var doctypeMatches = credentialQuery.Meta?.Doctype == null || credentialQuery.Meta?.Doctype == record.DocType;
                if (!doctypeMatches)
                    return false;

                // If no claims specified, accept the record
                if (credentialQuery.Claims == null || credentialQuery.Claims.Length == 0)
                    return true;

                // All claims must match
                return credentialQuery.Claims.All(requestedClaim =>
                {
                    // Build claim path
                    object[] pathObjects = [.. requestedClaim.Path.Cast<object>()];

                    return ClaimPath
                        .FromObjects(pathObjects)
                        .OnSuccess(path => path.ProcessWith(record.Mdoc))
                        .OnSuccess(selection =>
                        {
                            if (requestedClaim.Values != null)
                            {
                                var values = selection.GetValues().Select(v => v.ToString());
                                return requestedClaim.Values.Any(requestedValue => values.Contains(requestedValue));
                            }
                            return true;
                        })
                        .Fallback(false);
                });
            }),
            () => []
        );
    }
}
