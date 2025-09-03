using System.Collections.Immutable;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using InvalidOperationException = System.InvalidOperationException;

namespace WalletFramework.SdJwtLib.Models;

public class SdJwtDoc
{
    public ImmutableList<Disclosure> Disclosures { get; }
    
    /// <summary>
    /// The serialized SD-JWT in the format of "Issuer Signed JWT~Disclosure1~Disclosure2~...~DisclosureN~"
    /// </summary>
    public string IssuanceFormat { get; }
    
    /// <summary>
    /// The Issuer Signed JWT of the SD-JWT serialized in the format of "Header.Payload.Signature"
    /// </summary>
    public string IssuerSignedJwt { get; }
    
    /// <summary>
    /// The secured payload of the SD-JWT as a JObject
    /// </summary>
    public JObject SecuredPayload { get; }
    
    /// <summary>
    /// The unsecured payload of the SD-JWT as a JObject
    /// </summary>
    public JObject UnsecuredPayload { get; }

    public SdJwtDoc(string serializedSdJwt)
    {
        if (serializedSdJwt.Contains("~") && !serializedSdJwt.EndsWith('~'))
        {
            serializedSdJwt = serializedSdJwt.Substring(0, serializedSdJwt.LastIndexOf('~'));
        }
        var sdJwtItems = serializedSdJwt.Split('~');
        sdJwtItems = Array.FindAll(sdJwtItems, item => !string.IsNullOrEmpty(item));

        IssuanceFormat = serializedSdJwt;
        IssuerSignedJwt = sdJwtItems.First();
        SecuredPayload = JObject.Parse(Base64UrlEncoder.Decode(sdJwtItems.First().Split('.')[1]));
        Disclosures = sdJwtItems[1..].Select(Disclosure.Deserialize).ToImmutableList();
        (UnsecuredPayload, Disclosures) = DecodeSecuredPayload((JObject)SecuredPayload.DeepClone(), Disclosures.ToList());
    }
    
    public void AssertThatJwtSignatureIsValid(string issuerJwk, string expectedIssuer)
    {
        var jwtHandler = new JwtSecurityTokenHandler();
        
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = expectedIssuer,
            ValidateAudience = false,
            ValidateLifetime =  UnsecuredPayload.SelectToken("exp") != null,
            ValidateIssuerSigningKey = true,
            ValidTypes = new string[] {"vc+sd-jwt"},
            ValidAlgorithms = new string[] {"ES256"},
            IssuerSigningKey = JsonWebKey.Create(issuerJwk)
        };

        try
        {
            jwtHandler.ValidateToken(IssuerSignedJwt, validationParameters, out _);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Invalid SD-JWT - Issuer Signed Jwt invalid", ex);
        }
    }
    
    private (JObject, ImmutableList<Disclosure>) DecodeSecuredPayload(JObject securedPayload, List<Disclosure> disclosures)
    {
        var sdAlg = securedPayload.SelectToken("$._sd_alg")?.Value<string?>();
        securedPayload.SelectToken("$._sd_alg")?.Parent?.Remove();

        var (unsecuredPayload, validDisclosures) = sdAlg?.ToLowerInvariant() switch
        {
            "sha-256" => ValidateDisclosures(securedPayload, disclosures, [], SdAlg.SHA256),
            null => disclosures.Any() 
                ? ValidateDisclosures(securedPayload, disclosures, [], SdAlg.SHA256)
                : (securedPayload, disclosures),
            _ => throw new InvalidOperationException("Invalid SD-JWT - Unsupported _sd_alg")
        };

        return (unsecuredPayload, validDisclosures.ToImmutableList());
    }

    private (JObject, List<Disclosure>) ValidateDisclosures(JObject securedPayload, List<Disclosure> disclosures, List<string> processedDigests, SdAlg hashAlgorithm)
    {
        var embeddedSdDigests = securedPayload.SelectTokens("$.._sd").FirstOrDefault();
        if (embeddedSdDigests != null)
        {
            foreach (var token in embeddedSdDigests.ToList()) // this is always a single item 
            {
                if (processedDigests.Any(processedDigest => processedDigest == token.ToString()))
                    throw new InvalidOperationException("Invalid SD-JWT - Digests must be unique");
                processedDigests.Add(token.ToString());
                
                var matchingDisclosure = disclosures.Find(disclosure => disclosure.GetDigest(hashAlgorithm) == token.ToString());
                if (matchingDisclosure == null)
                    continue;
                
                if (matchingDisclosure.Name == "_sd" | matchingDisclosure.Name == "...")
                    throw new InvalidOperationException("Invalid SD-JWT - _sd and ... are reserved claim names");

                var parent = embeddedSdDigests.Parent?.Parent;
                if (parent == null || parent.SelectToken(matchingDisclosure.Name!) != null)
                    throw new InvalidOperationException($"Invalid SD-JWT - Disclosure {matchingDisclosure.Name} already exists in the payload");
            
                parent.Add(new JProperty(matchingDisclosure.Name!, matchingDisclosure.Value));
                
                matchingDisclosure.Path = $"{parent.Path}.{matchingDisclosure.Name}".TrimStart('.');
            }

            embeddedSdDigests.Parent?.Remove();
            ValidateDisclosures(securedPayload, disclosures, processedDigests, hashAlgorithm);
        }

        var embeddedArrayDigests = securedPayload.SelectTokens("$..['...']").ToList();
        if (embeddedArrayDigests.Count > 0)
        {
            foreach (var arrayDigests in embeddedArrayDigests)
            {
                if (processedDigests.Any(processedDigest => processedDigest == arrayDigests.ToString()))
                    throw new InvalidOperationException("Invalid SD-JWT - Digests must be unique");
                processedDigests.Add(arrayDigests.ToString());
                
                var matchingDisclosure = disclosures.Find(disclosure => disclosure.GetDigest(hashAlgorithm) == arrayDigests.ToString());

                if (matchingDisclosure == null)
                    arrayDigests.Parent?.Parent?.Remove();
                else
                {
                    matchingDisclosure.Path = arrayDigests.Parent?.Parent?.Path;
                    arrayDigests.Parent?.Parent?.Replace((JToken)matchingDisclosure.Value);
                }
            }
            ValidateDisclosures(securedPayload, disclosures, processedDigests, hashAlgorithm);
        }
        
        return (securedPayload, disclosures);
    }
}

public enum SdAlg
{
    SHA256
}
