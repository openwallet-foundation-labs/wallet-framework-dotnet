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
}
