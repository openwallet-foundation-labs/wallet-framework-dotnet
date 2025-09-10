using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Roles.Issuer;

namespace WalletFramework.SdJwtLib.Roles.Implementation;

public class Issuer : IIssuer
{
    public string Issue(List<Claim> claims, string issuerJwk)
    {
        // The interface should be refactored to expect the unsecured payload as string and not as a list of claims and the sd structure should be defined within a separate json
        // Issue(string payload, string sdStructure, string issuerJwk)
        throw new NotImplementedException();
    }
    
    public SdJwtDoc IssueCredential(JObject payload, string issuerJwk)
    {
        var counter = 0;
        var sdPaths = new List<string>();
        var disclosures = new List<Disclosure>();
        
        FindAndReplaceSdProperties(payload, "$", ref sdPaths, ref counter, disclosures);
        var securedPayload = payload;

        var jwtHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor();
        
        // Create a JsonDocument from the securedPayloadJson which is less error prone when used with JwtSecurityTokenHandler 
        var securedPayloadJson = JsonConvert.SerializeObject(securedPayload);
        var jdoc = JsonDocument.Parse(securedPayloadJson);

        tokenDescriptor.TokenType = "dc+sd-jwt";
        
        tokenDescriptor.Claims = jdoc.RootElement.EnumerateObject().ToDictionary(j => j.Name, j => (object)j.Value);
        tokenDescriptor.Claims.Add("_sd_alg", "sha-256");
        
        tokenDescriptor.SigningCredentials = new SigningCredentials(new JsonWebKey(issuerJwk), SecurityAlgorithms.EcdsaSha256);
        var token = jwtHandler.CreateEncodedJwt(tokenDescriptor);

        var encodedSdJwt = token + "~" + string.Join("~", disclosures.Select(d => d.Serialize())) + "~";
        
        return new SdJwtDoc(encodedSdJwt);
    }
    
    public static void FindAndReplaceSdProperties(JToken token, string path, ref List<string> sdPaths, ref int counter, List<Disclosure> disclosures)
    {
        if (token is SdProperty sdProperty)
        {
            sdPaths.Add(path);
            FindAndReplaceSdProperties(sdProperty.Value, path, ref sdPaths, ref counter, disclosures);
            AddSdDigestToParent(token, disclosures);
        } 
        else if (token is JProperty property)
        {
            FindAndReplaceSdProperties(property.Value, path, ref sdPaths, ref counter, disclosures);
        }
        else if (token is JObject obj)
        {
            foreach (var child in obj.Properties().ToList())
            {
                FindAndReplaceSdProperties(child, path + "." + child.Name, ref sdPaths, ref counter, disclosures);
            }
        }
        else if (token is JArray array)
        {
            for (var i = 0; i < array.Count; i++)
            {
                FindAndReplaceSdProperties(array[i], path + "[" + i + "]", ref sdPaths, ref counter, disclosures);
            }
        }
        else if (token is JValue)
        {
            Console.WriteLine($"{++counter}: {path}");
        }
        else
        {
            throw new Exception("Something unexpected happened.");
        }
    }

    private static void AddSdDigestToParent(JToken token, List<Disclosure> disclosures)
    {
        if (token is SdProperty sdProperty)
        {
            var newDisclosure = new Disclosure(sdProperty.Name, sdProperty.Value);
            if (token.Parent?.SelectToken("_sd") is JArray digests)
            {
                digests.Add(newDisclosure.GetDigest());    
            }
            else
            {
                token.Parent?.Add(new JProperty("_sd", new JArray(newDisclosure.GetDigest())));    
            }
            disclosures.Add(newDisclosure);
            token.Remove();
        }
    }
}
