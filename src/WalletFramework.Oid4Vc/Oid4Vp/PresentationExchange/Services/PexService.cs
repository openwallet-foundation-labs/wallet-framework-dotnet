using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Oid4Vc.Oid4Vp.Exceptions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models.Records;

namespace WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services
{
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
                    FindMatchingCredentialsForFields(credentials, inputDescriptor.Constraints.Fields);
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
        
        private static SdJwtRecord[] FindMatchingCredentialsForFields(
            SdJwtRecord[] records, Field[] fields)
        {
            return (from sdJwtRecord in records
                let claimsJson = JsonConvert.SerializeObject(sdJwtRecord.Claims)
                let claimsJObject = JObject.Parse(claimsJson)
                let isFound =
                    (from field in fields
                        let candidate = claimsJObject.SelectToken(field.Path[0])
                        where candidate != null && (field.Filter == null ||
                                                    string.Equals(field.Filter.Const, candidate.ToString()))
                        select field).Count() == fields.Length
                where isFound
                select sdJwtRecord).ToArray();
        }
    }
}
