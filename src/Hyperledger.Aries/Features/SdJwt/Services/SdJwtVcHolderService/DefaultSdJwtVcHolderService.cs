using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Features.OpenId4Vc.KeyStore.Services;
using Hyperledger.Aries.Features.OpenId4Vc.Vci.Models.Metadata.Issuer;
using Hyperledger.Aries.Features.OpenId4Vc.Vp.Models;
using Hyperledger.Aries.Features.Pex.Models;
using Hyperledger.Aries.Features.SdJwt.Models.Records;
using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Abstractions;
using SD_JWT.Models;

namespace Hyperledger.Aries.Features.SdJwt.Services.SdJwtVcHolderService
{
    /// <inheritdoc />
    public class DefaultSdJwtVcHolderService : ISdJwtVcHolderService
    {
        /// <summary>
        ///     The service responsible for holder operations.
        /// </summary>
        protected readonly IHolder Holder;

        /// <summary>
        ///     The key store responsible for key operations.
        /// </summary>
        protected readonly IKeyStore KeyStore;

        /// <summary>
        ///     The service responsible for wallet record operations.
        /// </summary>
        protected readonly IWalletRecordService RecordService;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DefaultSdJwtVcHolderService" /> class.
        /// </summary>
        /// <param name="keyStore">The key store responsible for key operations.</param>
        /// <param name="recordService">The service responsible for wallet record operations.</param>
        /// <param name="holder">The service responsible for holder operations.</param>
        public DefaultSdJwtVcHolderService(
            IHolder holder,
            IKeyStore keyStore,
            IWalletRecordService recordService)
        {
            Holder = holder;
            KeyStore = keyStore;
            RecordService = recordService;
        }

        /// <inheritdoc />
        public async Task<string> CreatePresentation(SdJwtRecord credential, string[]? disclosureNames,
            string? audience = null,
            string? nonce = null)
        {
            var disclosureNamesDict = new Dictionary<int, string>();
            for (var i = 0; i < credential.Disclosures.Length; i++)
            {
                var disclosureName = Disclosure.Deserialize(credential.Disclosures[i]).Name;
                disclosureNamesDict.Add(i, disclosureName);
            }

            var disclosures = disclosureNames
                .Select(disclosureName => disclosureNamesDict.FirstOrDefault(x => x.Value == disclosureName).Key)
                .Select(index => Disclosure.Deserialize(credential.Disclosures[index])).ToArray();

            string? keybindingJwt = null;
            if (!string.IsNullOrEmpty(credential.KeyId) && !string.IsNullOrEmpty(nonce) &&
                !string.IsNullOrEmpty(audience))
            {
                keybindingJwt =
                    await KeyStore.GenerateProofOfPossessionAsync(credential.KeyId, audience, nonce, "kb+jwt");
            }

            return Holder.CreatePresentation(credential.EncodedIssuerSignedJwt, disclosures, keybindingJwt);
        }

        /// <inheritdoc />
        public virtual async Task<bool> DeleteAsync(IAgentContext context, string recordId)
        {
            return await RecordService.DeleteAsync<SdJwtRecord>(context.Wallet, recordId);
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

            return Task.FromResult(result.ToArray());
        }

        /// <inheritdoc />
        public virtual async Task<SdJwtRecord> GetAsync(IAgentContext context, string credentialId)
        {
            var record = await RecordService.GetAsync<SdJwtRecord>(context.Wallet, credentialId);
            if (record == null)
                throw new AriesFrameworkException(ErrorCode.RecordNotFound, "SD-JWT Credential record not found");

            return record;
        }

        /// <inheritdoc />
        public virtual Task<List<SdJwtRecord>> ListAsync(IAgentContext context, ISearchQuery query = null,
            int count = 100,
            int skip = 0)
        {
            return RecordService.SearchAsync<SdJwtRecord>(context.Wallet, query, null, count, skip);
        }

        /// <inheritdoc />
        public virtual async Task<string> StoreAsync(
            IAgentContext context, 
            string combinedIssuance,
            string keyId, 
            OidIssuerMetadata issuerMetadata,
            string credentialMetadataId)
        {
            var sdJwtDoc = Holder.ReceiveCredential(combinedIssuance);
            var record = SdJwtRecord.FromSdJwtDoc(sdJwtDoc);
            record.Id = Guid.NewGuid().ToString();

            record.SetDisplayFromIssuerMetadata(issuerMetadata, credentialMetadataId);
            record.KeyId = keyId;

            await RecordService.AddAsync(context.Wallet, record);

            return record.Id;
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
