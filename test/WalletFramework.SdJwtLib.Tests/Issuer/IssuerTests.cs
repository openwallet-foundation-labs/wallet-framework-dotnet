using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.SdJwtLib.Models;
using WalletFramework.SdJwtLib.Roles.Issuer;

namespace WalletFramework.SdJwtLib.Tests.Issuer;

public class IssuerTests
{
    private const string IssuerJwk = """"
                                     {
                                         "kty": "EC",
                                         "d": "1_2Dagk1gvTIOX-DLPe7GHNsGLJMc7biySNA-so7TXE",
                                         "use": "sig",
                                         "crv": "P-256",
                                         "x": "X6sZhH_kFp_oKYiPXW-LvUyAv9mHp1xYcpAK3yy0wGY",
                                         "y": "p0URU7tgWbh42miznti0NVKM36fpJBbIfnF8ZCYGryE",
                                         "alg": "ES256"
                                     }
                                     """";
    
    [Test]
    public void CanIssueCredential()
    {
        var payload = new JObject
        {
            new SdProperty("first_name","John" ),
            new SdProperty("last_name","Smith" ),
            new SdProperty("age", 25),
            new SdProperty("address", new JObject
            {
                new SdProperty("street_address", "21 2nd Street"),
                new SdProperty( "city", "New York"),
                new SdProperty( "state", "NY"),
                new SdProperty( "postal_code", "10021")
            }),
            {
                "phone_numbers", new JArray
                {
                    new JObject
                    {
                        { "type", "home" },
                        { "number", "555-1234" }
                    },
                    new JObject
                    {
                        { "type", "work" },
                        { "number", "555-4321" }
                    }
                }
            },
            { "email", "john.doe@example.com" }
        };
        
        var issuer = new Roles.Implementation.Issuer();

        var result = issuer.IssueCredential(payload, IssuerJwk);
        
        Assert.NotNull(result);
        Console.WriteLine(result.IssuanceFormat);
    }

    [Test]
    public void CanTraverseJson()
    {
        var payload = new JObject
        {
            { "first_name", "John" },
            { "last_name", "Smith" },
            new SdProperty("age", 25),
            new SdProperty("address", new JObject
            {
                new SdProperty("street_address", "21 2nd Street"),
                { "city", "New York" },
                { "state", "NY" },
                { "postal_code", "10021" }
            }),
            {
                "phone_numbers", new SdArray
                {
                    new JObject
                    {
                        { "type", "home" },
                        { "number", "555-1234" }
                    },
                    new JObject
                    {
                        { "type", "work" },
                        { "number", "555-4321" }
                    }
                }
            },
            { "email", "john.doe@example.com" }
        };
        
        var counter = 0;
        List<string> sdPaths = [];
        var disclosures = new List<Disclosure>();
        Roles.Implementation.Issuer.FindAndReplaceSdProperties(payload, "$", ref sdPaths, ref counter, disclosures);
        Console.WriteLine(string.Join(',', sdPaths));
        Console.WriteLine(payload.ToString(Formatting.Indented));
        Console.WriteLine(string.Join("\n", disclosures.Select(d => $"{d.GetDigest()} : {d.Name} : {d.Serialize()}")));
    }
}
