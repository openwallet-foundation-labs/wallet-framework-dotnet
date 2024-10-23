using System.Collections.Immutable;
using Hyperledger.Aries.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;

namespace WalletFramework.SdJwtVc.Models.Records;

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
    ///     Gets the disclosures.
    /// </summary>
    public ImmutableArray<string> Disclosures { get; set; }

    /// <summary>
    ///     Gets or sets the display of the credential.
    /// </summary>
    public List<SdJwtDisplay>? Display { get; set; }

    /// <summary>
    ///     Gets the Issuer-signed JWT part of the SD-JWT.
    /// </summary>
    public string EncodedIssuerSignedJwt { get; set; } = null!;
    
    /// <summary>
    ///     Tracks the state of the SD-JWT.
    /// </summary>
    public CredentialState CredentialState { get; set; }
    
    //TODO: Must be set when batch issuance is implemented
    // public bool OneTimeUse { get; set; }
    
    public DateTime? ExpiresAt { get; set; }

    //TODO: Use CredentialSetId Type instead fo string
    public string CredentialSetId
    {
        get => Get();
        set => Set(value, false);
    }
    
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
    ///     Gets the key record ID.
    /// </summary>
    [JsonIgnore]
    public KeyId KeyId
    {
        get
        {
            var str = Get();
            return KeyId
                .ValidKeyId(str)
                .UnwrapOrThrow(new InvalidOperationException("Persisted Key-ID in SD-JWT Record is corrupt"));
        }
        private set
        {
            string keyId = value;
            Set(keyId);
        }
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
    /// <param name="disclosures">The disclosures.</param>
    /// <param name="display">The display of the credential.</param>
    /// <param name="issuerId"></param>
    /// <param name="encodedIssuerSignedJwt">The Issuer-signed JWT part of the SD-JWT.</param>
    /// <param name="credentialSetId">The CredentialSetId.</param>
    [JsonConstructor]
    public SdJwtRecord(
        Dictionary<string, ClaimMetadata> displayedAttributes,
        Dictionary<string, string> claims,
        ImmutableArray<string> disclosures,
        List<SdJwtDisplay> display,
        string issuerId,
        string encodedIssuerSignedJwt,
        string credentialSetId)
    {
        Claims = claims;
        Disclosures = disclosures;
           
        Display = display;
        DisplayedAttributes = displayedAttributes;
            
        EncodedIssuerSignedJwt = encodedIssuerSignedJwt;

        IssuerId = issuerId;
        CredentialSetId = credentialSetId;
    }
    
    public SdJwtRecord(
        string serializedSdJwtWithDisclosures,
        Dictionary<string, ClaimMetadata> displayedAttributes,
        List<SdJwtDisplay> display,
        KeyId keyId,
        CredentialSetId credentialSetId)
    {
        Id = Guid.NewGuid().ToString();
            
        var sdJwtDoc = new SdJwtDoc(serializedSdJwtWithDisclosures);
        EncodedIssuerSignedJwt = sdJwtDoc.IssuerSignedJwt;
        Disclosures = sdJwtDoc.Disclosures.Select(x => x.Serialize()).ToImmutableArray();
        Claims = sdJwtDoc.GetAllSubjectClaims();
        Display = display;
        DisplayedAttributes = displayedAttributes;

        CredentialSetId = credentialSetId;
        CredentialState = CredentialState.ACTIVE;
        
        KeyId = keyId;
        ExpiresAt = sdJwtDoc.UnsecuredPayload.SelectToken("exp")?.Value<long>() is not null
            ? DateTimeOffset.FromUnixTimeSeconds(sdJwtDoc.UnsecuredPayload.SelectToken("exp")!.Value<long>()).DateTime
            : null;
        IssuerId = sdJwtDoc.UnsecuredPayload.SelectToken("iss")?.Value<string>() 
                   ?? throw new ArgumentNullException(nameof(IssuerId), "iss claim is missing or null");
        Vct = sdJwtDoc.UnsecuredPayload.SelectToken("vct")?.Value<string>() 
              ?? throw new ArgumentNullException(nameof(Vct), "vct claim is missing or null");
    }
    
    public SdJwtRecord(
        SdJwtDoc sdJwtDoc,
        Dictionary<string, ClaimMetadata> displayedAttributes,
        List<SdJwtDisplay> display,
        KeyId keyId,
        CredentialSetId credentialSetId)
    {
        Id = Guid.NewGuid().ToString();
            
        EncodedIssuerSignedJwt = sdJwtDoc.IssuerSignedJwt;
        Disclosures = sdJwtDoc.Disclosures.Select(disclosure => disclosure.Serialize()).ToImmutableArray();
        Claims = sdJwtDoc.GetAllSubjectClaims();
        Display = display;
        DisplayedAttributes = displayedAttributes;
            
        CredentialSetId = credentialSetId;
        CredentialState = CredentialState.ACTIVE;
        
        KeyId = keyId;
        
        ExpiresAt = sdJwtDoc.UnsecuredPayload.SelectToken("exp")?.Value<long>() is not null
            ? DateTimeOffset.FromUnixTimeSeconds(sdJwtDoc.UnsecuredPayload.SelectToken("exp")!.Value<long>()).DateTime
            : null;
        IssuerId = sdJwtDoc.UnsecuredPayload.SelectToken("iss")?.Value<string>() 
                   ?? throw new ArgumentNullException(nameof(IssuerId), "iss claim is missing or null");
        Vct = sdJwtDoc.UnsecuredPayload.SelectToken("vct")?.Value<string>() 
              ?? throw new ArgumentNullException(nameof(Vct), "vct claim is missing or null");
    }

    public CredentialId GetId()
    {
        var id = CredentialId
            .ValidCredentialId(Id)
            .UnwrapOrThrow(new InvalidOperationException("SD-JWT RecordId is corrupt"));

        return id;
    }

    public CredentialSetId GetCredentialSetId() =>
        Core.Credentials.CredentialSetId.ValidCredentialSetId(CredentialSetId)
            .UnwrapOrThrow(new InvalidOperationException("The Id is corrupt"));
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
                //TODO decide if path without $ should be used -> currentPath.TrimStart('.')
                leafNodePaths.Add(currentPath.TrimStart('.'));
                break;
        }
    }
}
