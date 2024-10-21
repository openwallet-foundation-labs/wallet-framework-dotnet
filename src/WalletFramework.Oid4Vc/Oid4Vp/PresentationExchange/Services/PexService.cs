using Hyperledger.Aries.Agents;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Issuer;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Records;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

/// <inheritdoc />
public class PexService : IPexService
{
    public PexService(
        IAgentProvider agentProvider,
        IMdocStorage mdocStorage,
        ISdJwtVcHolderService sdJwtVcHolderService)
    {
        _agentProvider = agentProvider;
        _mdocStorage = mdocStorage;
        _sdJwtVcHolderService = sdJwtVcHolderService;
    }

    private readonly IAgentProvider _agentProvider;
    private readonly IMdocStorage _mdocStorage;
    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;
    
    /// <inheritdoc />
    public Task<PresentationSubmission> CreatePresentationSubmission(
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

    /// <inheritdoc />
    public virtual async Task<CredentialCandidates[]> FindCredentialCandidates(IEnumerable<InputDescriptor> inputDescriptors)
    {
        var result = new List<CredentialCandidates>();

        foreach (var inputDescriptor in inputDescriptors)
        {
            if (!(inputDescriptor.Formats.Keys.Contains("vc+sd-jwt") || inputDescriptor.Formats.Keys.Contains("mso_mdoc")))
            {
                throw new NotSupportedException("Only vc+sd-jwt or mso_mdoc format are supported");
            }

            if (inputDescriptor.Constraints.Fields == null || inputDescriptor.Constraints.Fields.Length == 0)
            {
                throw new InvalidOperationException("Fields cannot be null or empty");
            }

            var matchingCredentials = await GetMatchingCredentials(inputDescriptor);

            if (matchingCredentials.Count == 0)
                continue;

            var limitDisclosuresRequired = string.Equals(inputDescriptor.Constraints.LimitDisclosure, "required");

            var credentialSetGroups = matchingCredentials.GroupBy(x => x.GetCredentialSetId());
                
            var credentialSetCandidates = credentialSetGroups.Select(group =>
            {
                var credentialSetId = group.Key;
                var credentials = group.First();
                return new CredentialSetCandidate(credentialSetId, [credentials]);
            });
            
            var credentialCandidates = new CredentialCandidates(
                inputDescriptor.Id,
                credentialSetCandidates,
                limitDisclosuresRequired);

            result.Add(credentialCandidates);
        }

        return result.ToArray();
    }

    private async Task<List<ICredential>> GetMatchingCredentials(InputDescriptor inputDescriptor)
    {
        var context = await _agentProvider.GetContextAsync();
        
        var sdJwtRecords = await _sdJwtVcHolderService.ListAsync(context);
        var mdocRecords = await _mdocStorage.List();
        
        var filteredSdJwtRecords = sdJwtRecords.Where(record =>
        {
            var doc = _toSdJwtDoc(record);
            return inputDescriptor.Formats.ContainsKey("vc+sd-jwt") && inputDescriptor.Constraints.Fields!.All(field =>
            {
                try
                {
                    if (doc.UnsecuredPayload.SelectToken(field.Path.First(), true) is JValue value 
                        && field.Filter?.Const != null)
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
        }).Cast<ICredential>().AsOption();

        var filteredMdocRecords = mdocRecords.OnSome(records => records
            .Where(record => inputDescriptor.Formats.ContainsKey("mso_mdoc") && inputDescriptor.Constraints.Fields!.All(field =>
            {
                try
                {
                    var jObj = record.Mdoc.IssuerSigned.IssuerNameSpaces.ToJObject();
                    if (jObj.SelectToken(field.Path.First(), true) is JValue value
                        && field.Filter?.Const != null)
                    {
                        return field.Filter?.Const == value.Value?.ToString();
                    }

                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }))
            .Cast<ICredential>()
            .AsOption());

        var credentialCandidates = filteredSdJwtRecords.Match(
            credentials =>
            {
                filteredMdocRecords.IfSome(mdocCredentials => 
                    credentials = credentials.Concat(mdocCredentials)
                );

                return credentials;
            },
            () => filteredMdocRecords.Match(
                mdocCredentials => mdocCredentials,
                () => Enumerable.Empty<ICredential>()
            ));

        return credentialCandidates.ToList();
    }

    private static SdJwtDoc _toSdJwtDoc(SdJwtRecord record) =>
        new(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
}
