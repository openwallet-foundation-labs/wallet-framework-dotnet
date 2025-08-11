using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using LanguageExt;
using Newtonsoft.Json.Linq;
using SD_JWT.Models;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.Core.StatusList;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Models.Credential.Attributes;

namespace WalletFramework.SdJwtVc;

public record SdJwtCredential : ICredential
{
    public bool OneTimeUse { get; init; }

    public CredentialId CredentialId { get; init; }

    public CredentialSetId CredentialSetId { get; init; }

    public CredentialState CredentialState { get; init; }
        = CredentialState.Active;

    public Dictionary<string, string> Claims { get; init; } = new();

    public ImmutableArray<string> Disclosures { get; init; } = ImmutableArray<string>.Empty;

    public KeyId KeyId { get; init; }

    public Option<DateTime> ExpiresAt { get; init; } = Option<DateTime>.None;

    public Option<DateTime> IssuedAt { get; init; } = Option<DateTime>.None;

    public Option<DateTime> NotBefore { get; init; } = Option<DateTime>.None;

    public Option<Dictionary<string, ClaimMetadata>> DisplayedAttributes { get; } =
        Option<Dictionary<string, ClaimMetadata>>.None;

    public Option<List<SdJwtDisplay>> Displays { get; init; } = Option<List<SdJwtDisplay>>.None;

    public Option<List<string>> AttributeOrder { get; } = Option<List<string>>.None;

    public Option<StatusListEntry> StatusListEntry { get; init; } = Option<StatusListEntry>.None;
    public string EncodedIssuerSignedJwt { get; init; } = string.Empty;

    public string Format { get; init; } = string.Empty;

    public string IssuerId { get; init; } = string.Empty;

    public Vct Vct { get; init; }

    public SdJwtDoc SdJwtDoc { get; init; } = null!;

    // Parameterless constructor for EF
    public SdJwtCredential()
    {
    }

    public SdJwtCredential(
        SdJwtDoc sdJwtDoc,
        CredentialId credentialId,
        CredentialSetId credentialSetId,
        Option<List<SdJwtDisplay>> displays,
        KeyId keyId,
        CredentialState credentialState,
        bool oneTimeUse,
        Option<DateTime> expiresAt)
    {
        EncodedIssuerSignedJwt = sdJwtDoc.IssuerSignedJwt;
        SdJwtDoc = sdJwtDoc;
        CredentialId = credentialId;
        CredentialSetId = credentialSetId;
        Displays = displays;
        var vctValue = sdJwtDoc.UnsecuredPayload.SelectToken("vct")?.Value<string>()
                       ?? throw new ArgumentNullException(nameof(Vct), "vct claim is missing or null");
        Vct = WalletFramework.SdJwtVc.Models.Vct.ValidVct(vctValue).UnwrapOrThrow();
        KeyId = keyId;
        CredentialState = credentialState;
        OneTimeUse = oneTimeUse;
        ExpiresAt = expiresAt;

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(EncodedIssuerSignedJwt);

        Format = token.Header.Typ;

        Disclosures = sdJwtDoc.Disclosures.Select(x => x.Serialize()).ToImmutableArray();
        Claims = GetAllSubjectClaims(sdJwtDoc);

        var status = sdJwtDoc.UnsecuredPayload.SelectToken("status")?.SelectToken("status_list")
                         ?.ToObject<StatusListEntry>()
                     ?? sdJwtDoc.UnsecuredPayload.SelectToken("status")?.ToObject<StatusListEntry>();
        StatusListEntry = status is not null
            ? Option<StatusListEntry>.Some(status)
            : Option<StatusListEntry>.None;

        IssuedAt = sdJwtDoc.UnsecuredPayload.SelectToken("iat")?.Value<long>() is { } iat
            ? Option<DateTime>.Some(DateTimeOffset.FromUnixTimeSeconds(iat).DateTime)
            : Option<DateTime>.None;

        NotBefore = sdJwtDoc.UnsecuredPayload.SelectToken("nbf")?.Value<long>() is { } nbf
            ? Option<DateTime>.Some(DateTimeOffset.FromUnixTimeSeconds(nbf).DateTime)
            : Option<DateTime>.None;

        IssuerId = sdJwtDoc.UnsecuredPayload.SelectToken("iss")?.Value<string>()
                   ?? throw new ArgumentNullException(nameof(IssuerId), "iss claim is missing or null");
    }

    public CredentialSetId GetCredentialSetId() => CredentialSetId;

    public CredentialId GetId() => CredentialId;
    
    private static Dictionary<string, string> GetAllSubjectClaims(SdJwtDoc sdJwtDoc)
    {
        var unsecuredPayload = (JObject)sdJwtDoc.UnsecuredPayload.DeepClone();

        RemoveRegisteredClaims(unsecuredPayload);

        var allLeafClaims = GetLeafNodePaths(unsecuredPayload);

        return allLeafClaims.ToDictionary(key => key,
            key => unsecuredPayload.SelectToken(key)?.ToString() ?? string.Empty);

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
                 var index = 0;
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
