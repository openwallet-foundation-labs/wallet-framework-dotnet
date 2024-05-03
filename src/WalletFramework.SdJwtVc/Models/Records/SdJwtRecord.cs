using System.Collections.Immutable;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Hyperledger.Aries.Storage;
using Hyperledger.Aries.Storage.Models.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;

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
        public Dictionary<string, ClaimDisplayInfo>? DisplayedAttributes { get; set; }

        /// <summary>
        ///     Gets or sets the attributes that should be displayed.
        /// </summary>
        public List<string>? AttributeOrder { get; set; }
        
        /// <summary>
        ///     Gets or sets the claims made.
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
        public List<CredentialDisplayInfo>? Display { get; set; }

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
        private SdJwtRecord(
            Dictionary<string, ClaimDisplayInfo> displayedAttributes,
            Dictionary<string, string> claims,
            Dictionary<string, string> issuerName,
            ImmutableArray<string> disclosures,
            List<CredentialDisplayInfo> display,
            string encodedIssuerSignedJwt,
            string id,
            string issuerId,
            string keyId,
            string vct)
        {
            Claims = claims;
            Disclosures = disclosures;
            Display = display;
            DisplayedAttributes = displayedAttributes;
            EncodedIssuerSignedJwt = encodedIssuerSignedJwt;
            Id = id;
            IssuerId = issuerId;
            IssuerName = issuerName;
            KeyId = keyId;
            Vct = vct;
        }

        /// <summary>
        ///     Creates a SdJwtRecord from a SdJwtDoc.
        /// </summary>
        /// <param name="sdJwtDoc">The SdJwtDoc.</param>
        /// <returns>The SdJwtRecord.</returns>
        public static SdJwtRecord FromSdJwtDoc(SdJwtDoc sdJwtDoc)
            => new()
            {
                EncodedIssuerSignedJwt = sdJwtDoc.EncodedIssuerSignedJwt,
                Vct = ExtractVctFromJwtPayload(sdJwtDoc.EncodedIssuerSignedJwt),
                Disclosures = sdJwtDoc.Disclosures.Select(x => x.Serialize()).ToImmutableArray(),
                Claims = WithDisclosedClaims(sdJwtDoc.EncodedIssuerSignedJwt)
                    .Concat(WithSelectivelyDisclosableClaims(sdJwtDoc.Disclosures))
                    .ToDictionary(x => x.key, x => x.value)
            };

        /// <summary>
        ///     Sets display information properties of the SdJwtRecord.
        /// </summary>
        /// <param name="display">The credential display information.</param>
        /// <param name="issuerId"></param>
        /// <param name="issuerName"></param>
        /// <param name="displayedAttributes"></param>
        /// <param name="order"></param>
        internal void SetDisplayInfo(
            List<CredentialDisplayInfo> display,
            Dictionary<string, ClaimDisplayInfo> displayedAttributes,
            string issuerId,
            Dictionary<string, string>? issuerName,
            List<string>? order)
        {
            Display = display;
            DisplayedAttributes = displayedAttributes;
            IssuerId = issuerId;
            IssuerName = issuerName;
            AttributeOrder = order;
        }

        /// <summary>
        ///     Extracts the "vct" property from the JWT payload.
        /// </summary>
        /// <param name="encodedJwt">The encoded JWT.</param>
        /// <returns>The value of the verifiable credential type claim.</returns>
        private static string ExtractVctFromJwtPayload(string encodedJwt)
        {
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadJwtToken(encodedJwt);
            var payloadJson = jwtToken.Payload.SerializeToJson();
            var payloadObj = JsonDocument.Parse(payloadJson).RootElement;

            if (payloadObj.TryGetProperty("vct", out var vct))
            {
                return vct.GetString() ?? string.Empty;
            }

            return string.Empty;
        }

        private static IEnumerable<(string key, string value)> WithDisclosedClaims(string tokenAsString)
            => new JwtSecurityTokenHandler()
                .ReadJwtToken(tokenAsString).Claims
                .Where(claim => !string.Equals(claim.Type, "_sd") && !string.Equals(claim.Type, "..."))
                .Select(c => (c.Type, c.Value));

        /// <summary>
        ///     Creates a dictionary of claims based on the list of disclosures.
        /// </summary>
        /// <param name="disclosures">The list of disclosures.</param>
        /// <returns>The dictionary of claims.</returns>
        private static IEnumerable<(string key, string value)> WithSelectivelyDisclosableClaims(
            IEnumerable<Disclosure> disclosures)
            => disclosures
                .Where(x => x.Value is JValue jValue)
                .Select(d => (d.Name, ((JValue)d.Value).ToString(CultureInfo.InvariantCulture)));
    }
}
