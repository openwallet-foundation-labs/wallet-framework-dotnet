using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;

/// <inheritdoc />
public class PexService : IPexService
{
    /// <inheritdoc />
    public Task<PresentationSubmission> CreatePresentationSubmission(PresentationDefinition presentationDefinition, DescriptorMap[] descriptorMaps)
    {
        var inputDescriptorIds = presentationDefinition.InputDescriptors.Select(x => x.Id);
        if (!descriptorMaps.Select(x => x.Id).All(inputDescriptorIds.Contains))
            throw new ArgumentException("Missing descriptors for given input descriptors in presentation definition.", nameof(descriptorMaps));
            
        var presentationSubmission = new PresentationSubmission
        {
            Id = Guid.NewGuid().ToString(),
            DefinitionId = presentationDefinition.Id,
            DescriptorMap = descriptorMaps.Cast<DescriptorMap>().ToArray()
        };
            
        return Task.FromResult(presentationSubmission);
    }

    /// <inheritdoc />
    public virtual Task<CredentialCandidates[]> FindCredentialCandidates(SdJwtRecord[] credentials,
        InputDescriptor[] inputDescriptors)
    {
        var result = new List<CredentialCandidates>();

        foreach (var inputDescriptor in inputDescriptors)
        {
            if (!inputDescriptor.Formats.Keys.Contains("vc+sd-jwt"))
            {
                throw new NotSupportedException("Only vc+sd-jwt format is supported");
            }

            if (inputDescriptor.Constraints.Fields == null || inputDescriptor.Constraints.Fields.Length == 0)
            {
                throw new InvalidOperationException("Fields cannot be null or empty");
            }

            var matchingCredentials =
                _filterMatchingCredentialsForFields(credentials, inputDescriptor.Constraints.Fields);
            if (matchingCredentials.Length == 0)
            {
                continue;
            }

            var limitDisclosuresRequired = string.Equals(inputDescriptor.Constraints.LimitDisclosure, "required");

            var credentialCandidates = new CredentialCandidates(inputDescriptor.Id,
                matchingCredentials, limitDisclosuresRequired);

            result.Add(credentialCandidates);
        }
            
        if (result.IsNullOrEmpty())
        {
            throw new Oid4VpNoCredentialCandidateException();
        }

        return Task.FromResult(result.ToArray());
    }

    private static SdJwtRecord[] _filterMatchingCredentialsForFields(SdJwtRecord[] records, Field[] fields) 
    {
        var candidateRecords = new List<SdJwtRecord>();
        foreach (var record in records)
        {
            var doc = _toSdJwtDoc(record);
            var isAMatch = fields.All(field =>
            {
                try
                {
                    if (doc.UnsecuredPayload.SelectToken(field.Path.First(), true) is JValue value && field.Filter?.Const != null)
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

            if (isAMatch)
                candidateRecords.Add(record);
        }

        return candidateRecords.ToArray();
    }


    private static SdJwtDoc _toSdJwtDoc(SdJwtRecord record)
    {
        return new SdJwtDoc(record.EncodedIssuerSignedJwt + "~" + string.Join("~", record.Disclosures) + "~");
    }
}
