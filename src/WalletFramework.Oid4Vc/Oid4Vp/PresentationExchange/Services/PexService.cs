using System.IdentityModel.Tokens.Jwt;
using Hyperledger.Aries.Agents;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Issuer;
using WalletFramework.MdocLib.Security.Cose;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Query;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

/// <inheritdoc />
public class PexService(
    IAgentProvider agentProvider,
    IMdocStorage mdocStorage,
    ISdJwtVcHolderService sdJwtVcHolderService) : IPexService
{
    public async Task<CandidateQueryResult> FindPresentationCandidatesAsync(PresentationDefinition presentationDefinition, Option<Formats> supportedFormatSigningAlgorithms)
    {
        var candidates = await FindCandidates(
            presentationDefinition.InputDescriptors,
            supportedFormatSigningAlgorithms);

        var candidateList = candidates.ToList();
        var candidatesOption = candidateList.Count == 0
            ? Option<List<PresentationCandidateSet>>.None
            : new List<PresentationCandidateSet> { candidateList.ToSet() };

        // Find missing credentials: input descriptors with no candidates
        var missing = presentationDefinition.InputDescriptors
            .Where(inputDescriptor => candidateList.All(c => c.Identifier != inputDescriptor.Id))
            .Select(inputDescriptor => new CredentialRequirement(inputDescriptor))
            .ToList();
        var missingOption = missing.Count > 0 ? missing : Option<List<CredentialRequirement>>.None;

        return new CandidateQueryResult(
            candidatesOption,
            missingOption
        );
    }

    public async Task<Option<PresentationCandidate>> FindPresentationCandidateAsync(InputDescriptor inputDescriptor)
    {
        var candidates = await FindCandidates(
            [inputDescriptor],
            Option<Formats>.None);

        var candidatesList = candidates.ToList();

        return candidatesList.Count == 0
            ? Option<PresentationCandidate>.None
            : candidatesList.First();
    }
    
    /// <inheritdoc />
    public async Task<AuthorizationResponse> CreateAuthorizationResponseAsync(
        AuthorizationRequest authorizationRequest,
        PresentationMap[] presentationMaps)
    {
        var descriptorMaps = new List<DescriptorMap>();
        var vpToken = new List<string>();
            
        for (var index = 0; index < presentationMaps.Length; index++)
        {
            vpToken.Add(presentationMaps[index].Presentation);

            var descriptorMap = new DescriptorMap
            {
                Format = presentationMaps[index].Format.ToString(),
                Path = presentationMaps.Length > 1 ? "$[" + index + "]" : "$",
                Id = presentationMaps[index].Identifier,
                PathNested = null
            };
            descriptorMaps.Add(descriptorMap);
        }

        var presentationSubmission = await CreatePresentationSubmission(
            authorizationRequest.PresentationDefinition,
            descriptorMaps.ToArray());
        
        return new AuthorizationResponse
        {
            PresentationSubmission = presentationSubmission,
            VpToken = new VpToken(vpToken[0]),
            State = authorizationRequest.State
        };
    }

    private async Task<IEnumerable<PresentationCandidate>> FindCandidates(
        IEnumerable<InputDescriptor> inputDescriptors,
        Option<Formats> supportedFormatSigningAlgorithms)
    {
        var result = new List<PresentationCandidate>();

        foreach (var inputDescriptor in inputDescriptors)
        {
            if (inputDescriptor.Constraints.Fields == null || inputDescriptor.Constraints.Fields.Length == 0)
            {
                throw new InvalidOperationException("Fields cannot be null or empty");
            }
            
            var matchingCredentialsOption = await GetMatchingCredentials(
                inputDescriptor,
                supportedFormatSigningAlgorithms);

            matchingCredentialsOption.IfSome(matchingCredentials =>
            {
                var limitDisclosuresRequired = string.Equals(inputDescriptor.Constraints.LimitDisclosure, "required");

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
                    inputDescriptor.Id,
                    credentialSetCandidates,
                    limitDisclosuresRequired);

                result.Add(credentialCandidates);
            });
        }

        return result;
    }

    private async Task<Option<IEnumerable<ICredential>>> GetMatchingCredentials(
        InputDescriptor inputDescriptor,
        Option<Formats> supportedFormatSigningAlgorithms)
    {
        var context = await agentProvider.GetContextAsync();
        
        var sdJwtRecords = await sdJwtVcHolderService.ListAsync(context);
        var mdocRecords = await mdocStorage.ListAll();
        
        var filteredSdJwtRecords = sdJwtRecords.Where(record =>
        {
            var doc = record.ToSdJwtDoc();
            
            var handler = new JwtSecurityTokenHandler();
            var issuerSignedJwt = handler.ReadJwtToken(doc.IssuerSignedJwt);

            return issuerSignedJwt.Header.TryGetValue("alg", out var alg)
                   && supportedFormatSigningAlgorithms.Match(
                       formats => (formats.SdJwtVcFormat?.IssuerSignedJwtAlgValues?.Contains(alg.ToString()) ?? true) 
                                  || (formats.SdJwtDcFormat?.IssuerSignedJwtAlgValues?.Contains(alg.ToString()) ?? true),
                       () => (inputDescriptor.Formats?.SdJwtVcFormat?.IssuerSignedJwtAlgValues?.Contains(alg.ToString()) ?? true) 
                           || (inputDescriptor.Formats?.SdJwtDcFormat?.IssuerSignedJwtAlgValues?.Contains(alg.ToString()) ?? true))
                   && inputDescriptor.Constraints.Fields!.All(field =>
                   {
                       try
                       {
                           var value = doc.UnsecuredPayload.SelectToken(field.Path.First(), true)!;
                    
                           if (field.Filter?.Const != null || field.Filter?.Enum != null)
                           {
                               return field.Filter?.Const == value.ToString() || field.Filter?.Enum?.Contains(value.ToString()) == true;
                           }

                           return true;
                       }
                       catch (Exception)
                       {
                           return false;
                       }
                   });
        }).Cast<ICredential>().AsOption();

        var filteredMdocRecords = mdocRecords.OnSome(records => records
            .Where(record =>
            {
                return record.DocType == inputDescriptor.Id
                       && record.Mdoc.IssuerSigned.IssuerAuth.ProtectedHeaders.Value.TryGetValue(new CoseLabel(1), out var alg)
                       && supportedFormatSigningAlgorithms.Match(
                           formats => formats.MDocFormat?.Alg?.Contains(alg.ToString()) ?? true,
                           () => inputDescriptor.Formats?.MDocFormat?.Alg?.Contains(alg.ToString()) ?? true)
                       && inputDescriptor.Constraints.Fields!.All(field =>
                       {
                           try
                           {
                               var jObj = record.Mdoc.IssuerSigned.IssuerNameSpaces.ToJObject();

                               if (jObj.SelectToken(field.Path.First(), true) is not JValue value)
                                   return false;

                               if (field.Filter?.Const != null)
                               {
                                   return field.Filter?.Const == value.Value?.ToString();
                               }

                               return true;
                           }
                           catch (Exception)
                           {
                               return false;
                           }
                       });
            }).Cast<ICredential>().AsOption());

        var candidates = filteredSdJwtRecords.Match(
            credentials =>
            {
                filteredMdocRecords.IfSome(mdocCredentials => 
                    credentials = credentials.Concat(mdocCredentials)
                );

                return credentials;
            },
            () => filteredMdocRecords.Match(
                mdocCredentials => mdocCredentials,
                () => []
            ))
            .ToList();

        return candidates.Any() 
            ? candidates 
            : Option<IEnumerable<ICredential>>.None;
    }
    
    private Task<PresentationSubmission> CreatePresentationSubmission(
        PresentationDefinition presentationDefinition,
        DescriptorMap[] descriptorMaps)
    {
        var inputDescriptorIds = presentationDefinition.InputDescriptors.Select(x => x.Id);
        if (!descriptorMaps.Select(x => x.Id).All(inputDescriptorIds.Contains))
            throw new ArgumentException("Missing descriptors for given input descriptors in presentation definition.",
                nameof(descriptorMaps));

        var presentationSubmission = new PresentationSubmission
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = presentationDefinition.Id,
            DescriptorMap = descriptorMaps.ToArray()
        };

        return Task.FromResult(presentationSubmission);
    }
}
