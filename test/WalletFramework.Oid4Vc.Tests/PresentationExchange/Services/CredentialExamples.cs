using Newtonsoft.Json.Linq;
using SD_JWT.Roles.Issuer;

namespace WalletFramework.Oid4Vc.Tests.PresentationExchange.Services;

public static class CredentialExamples
{
    public static JObject DriverCredential => new()
    {
        {"vct", "DriverCredential"},
        {"iss", "did:example:drivers-license-authority"},
        new SdProperty("id", "123"),
        new SdProperty("issuer", "did:example:gov"),
        new SdProperty("dateOfBirth", "01/01/2000")
    };
    
    public static JObject UniversityCredential => new()
    {
        {"vct", "UniversityDegreeCredential"},
        {"iss", "did:example:univserity"},
        { "degree", "Master of Science" },
        { "universityName", "ExampleUniversity" }
    };
    
    public static JObject NestedCredential => new()
    {
        {"vct", "IdentityCredential"},
        {"iss", "did:example:issuer"},
        new SdProperty("first_name", "John"),
        new SdProperty("last_name", "Doe"),
        new SdProperty("address", new JObject
        {
            new SdProperty("street", "21 2nd Street"),
            new SdProperty("city", "New York"),
            new SdProperty("state", "NY"),
            new SdProperty("postal_code", "10021")
        })
    };
    
    public static JObject AlternativeNestedCredential => new()
    {
        {"vct", "IdentityCredential"},
        {"iss", "did:example:issuer"},
        new SdProperty("first_name", "Erika"),
        new SdProperty("last_name", "Mustermann"),
        new SdProperty("address", new JObject
        {
            new SdProperty("street", "Schuldstr. 15"),
            new SdProperty("city", "Berlin"),
            new SdProperty("state", "BE"),
            new SdProperty("postal_code", "12345")
        })
    };
}
