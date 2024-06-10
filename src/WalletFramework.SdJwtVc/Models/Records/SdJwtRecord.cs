using System.Collections.Immutable;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Storage.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;
using WalletFramework.SdJwtVc.Models.Issuer;

namespace WalletFramework.SdJwtVc.Models.Records
{
    /// <summary>
    ///     A record that represents a Selective Disclosure JSON Web Token (SD-JWT) Credential with additional properties.
    ///     Inherits from base class RecordBase.
    /// </summary>
    public sealed class SdJwtRecord : RecordBase, ICredential
    {
        /// <summary>
        ///     Gets or sets the attributes that should be displayed.
        /// </summary>
        public Dictionary<string, ClaimMetadata>? DisplayedAttributes { get; set; }

        /// <summary>
        ///     Gets or sets the attributes that should be displayed.
        /// </summary>
        public List<string>? AttributeOrder { get; set; }

        /// <summary>
        ///     Gets or sets the flattened structure of the claims in the credential.
        ///     The key is a JSON path to the claim value in the decoded SdJwtVc.
        /// </summary>
        public Dictionary<string, string> Claims { get; set; }

        /// <summary>
        ///     Gets or sets the name of the issuer in different languages.
        /// </summary>
        public Dictionary<string, string>? IssuerName { get; set; }

        /// <summary>
        ///     Gets the disclosures.
        /// </summary>
        public ImmutableArray<string> Disclosures { get; set; }

        /// <summary>
        ///     Gets or sets the display of the credential.
        /// </summary>
        public List<CredentialDisplayMetadata>? Display { get; set; }

        /// <summary>
        ///     Gets the Issuer-signed JWT part of the SD-JWT.
        /// </summary>
        public string EncodedIssuerSignedJwt { get; set; } = null!;

        /// <summary>
        ///     Gets or sets the identifier for the issuer.
        /// </summary>
        [JsonIgnore]
        public string IssuerId
        {
            get => Get();
            set => Set(value, false);
        }

        /// <summary>
        ///     Gets or sets the key record ID.
        /// </summary>
        [JsonIgnore]
        public string KeyId
        {
            get => Get();
            set => Set(value);
        }

        /// <inheritdoc />
        public override string TypeName => "AF.SdJwtRecord";

        /// <summary>
        ///     Gets the verifiable credential type.
        /// </summary>
        [JsonIgnore]
        public string Vct
        {
            get => Get();
            set => Set(value, false);
        }
#pragma warning disable CS8618
        /// <summary>
        ///     Parameterless Default Constructor.
        /// </summary>
        public SdJwtRecord()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        ///     Constructor for Serialization.
        /// </summary>
        /// <param name="displayedAttributes">The attributes that should be displayed.</param>
        /// <param name="claims">The claims made.</param>
        /// <param name="issuerName">The name of the issuer in different languages.</param>
        /// <param name="disclosures">The disclosures.</param>
        /// <param name="display">The display of the credential.</param>
        /// <param name="encodedIssuerSignedJwt">The Issuer-signed JWT part of the SD-JWT.</param>
        /// <param name="id">The identifier.</param>
        /// <param name="issuerId">The identifier for the issuer.</param>
        /// <param name="keyId">The key record ID.</param>
        /// <param name="vct">The verifiable credential type.</param>
        [JsonConstructor]
        public SdJwtRecord(
            Dictionary<string, ClaimMetadata> displayedAttributes,
            Dictionary<string, string> claims,
            Dictionary<string, string> issuerName,
            ImmutableArray<string> disclosures,
            List<CredentialDisplayMetadata> display,
            string encodedIssuerSignedJwt,
            string issuerId,
            string keyId,
            string vct)
        {
            Id = Guid.NewGuid().ToString();
            
            Claims = claims;
            Disclosures = disclosures;
            
            Display = display;
            DisplayedAttributes = displayedAttributes;
            
            EncodedIssuerSignedJwt = encodedIssuerSignedJwt;
            
            IssuerId = issuerId;
            IssuerName = issuerName;
            
            KeyId = keyId;
            Vct = vct;
        }
        
        public SdJwtRecord(
            string serializedSdJwtWithDisclosures,
            Dictionary<string, ClaimMetadata> displayedAttributes,
            List<CredentialDisplayMetadata> display,
            Dictionary<string, string> issuerName,
            string keyId
        )
        {
            Id = Guid.NewGuid().ToString();
            
            SdJwtDoc sdJwtDoc = new SdJwtDoc(serializedSdJwtWithDisclosures);
            EncodedIssuerSignedJwt = sdJwtDoc.IssuerSignedJwt;
            Disclosures = sdJwtDoc.Disclosures.Select(x => x.Serialize()).ToImmutableArray();
            Claims = sdJwtDoc.GetAllSubjectClaims();
            Display = display;
            DisplayedAttributes = displayedAttributes;
            
            IssuerName = issuerName;
            
            KeyId = keyId;
            IssuerId = sdJwtDoc.UnsecuredPayload.SelectToken("iss")?.Value<string>() ?? 
                       throw new ArgumentNullException(nameof(IssuerId), "iss claim is missing or null");
            Vct = sdJwtDoc.UnsecuredPayload.SelectToken("vct")?.Value<string>() ?? 
                  throw new ArgumentNullException(nameof(Vct), "vct claim is missing or null"); // Extract vct
        }

        /// <summary>
        ///     Creates a dictionary of the issuer name in different languages based on the issuer metadata.
        /// </summary>
        /// <param name="issuerMetadata">The issuer metadata.</param>
        /// <returns>The dictionary of the issuer name in different languages.</returns>
        private static Dictionary<string, string>? CreateIssuerNameDictionary(IssuerMetadata issuerMetadata)
        {
            var issuerNameDictionary = new Dictionary<string, string>();

            foreach (var display in issuerMetadata.Display?.Where(d => d.Locale != null) ??
                                    Enumerable.Empty<IssuerDisplay>())
            {
                issuerNameDictionary[display.Locale!] = display.Name!;
            }

            return issuerNameDictionary.Count > 0 ? issuerNameDictionary : null;
        }
    }
    
    internal static class JsonExtensions
    {
        internal static Dictionary<string, string> GetAllSubjectClaims(this SdJwtDoc sdJwtDoc)
        {
            var unsecuredPayload = (JObject)sdJwtDoc.UnsecuredPayload.DeepClone();
            
            RemoveRegisteredClaims(unsecuredPayload);
            
            var allLeafClaims = GetLeafNodePaths(unsecuredPayload);

            return allLeafClaims.ToDictionary(key => key, key => unsecuredPayload.SelectToken(key)?.ToString() ?? string.Empty);

            void RemoveRegisteredClaims(JObject jObject)
            {
                string[] registeredClaims = { "iss", "sub", "aud", "exp", "nbf", "iat", "jti", "vct", "cnf", "status" };
                foreach (var claim in registeredClaims)
                {
                    jObject.Remove(claim);
                }
            }
        }
        
        private static List<string> GetLeafNodePaths(JObject jObject)
        {
            var leafNodePaths = new List<string>();

            TraverseJToken(jObject, "", leafNodePaths);

            return leafNodePaths;
        }

        private static void TraverseJToken(JToken token, string currentPath, List<string> leafNodePaths)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    foreach (var property in token.Children<JProperty>())
                    {
                        TraverseJToken(property.Value, $"{currentPath}.{property.Name}", leafNodePaths);
                    }
                    break;

                case JTokenType.Array:
                    int index = 0;
                    foreach (var item in token.Children())
                    {
                        TraverseJToken(item, $"{currentPath}[{index}]", leafNodePaths);
                        index++;
                    }
                    break;

                default:
                    leafNodePaths.Add(currentPath.TrimStart('.'));
                    break;
            }
        }
    }
}
