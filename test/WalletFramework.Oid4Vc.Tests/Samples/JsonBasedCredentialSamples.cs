using Newtonsoft.Json.Linq;
using SD_JWT.Roles.Issuer;

namespace WalletFramework.Oid4Vc.Tests.Samples;

public static class JsonBasedCredentialSamples
{
    public static JObject AlternativeNestedCredential => new()
    {
        { "vct", "IdentityCredential" },
        { "iss", "did:example:issuer" },
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

    public static JObject BatchCredential => new()
    {
        { "vct", "BatchCredential" },
        { "iss", "did:example:batch-authority" },
        new SdProperty("id", "123"),
        new SdProperty("issuer", "did:example:gov"),
        new SdProperty("batchExp", "01/01/2000")
    };

    public static JObject DriverCredential => new()
    {
        { "vct", "DriverCredential" },
        { "iss", "did:example:drivers-license-authority" },
        new SdProperty("id", "123"),
        new SdProperty("issuer", "did:example:gov"),
        new SdProperty("dateOfBirth", "01/01/2000")
    };

    public static JObject NestedCredential => new()
    {
        { "vct", "IdentityCredential" },
        { "iss", "did:example:issuer" },
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

    public static JObject UniversityCredential => new()
    {
        { "vct", "UniversityDegreeCredential" },
        { "iss", "did:example:univserity" },
        { "degree", "Master of Science" },
        { "universityName", "ExampleUniversity" }
    };

    public const string IdCardCredentialAsJsonStr =
        """
        {
          "nbf": 1706542681,
          "vct": "ID-Card-Vct",
          "iss": "https://sample-issuer.io",
          "cnf": {
            "jwk": {
              "kty": "EC",
              "crv": "P-256",
              "x": "4sL7d2ohKR5E4aZjOw9r5tSeZmjnBPvXwZxoukIVvUE",
              "y": "I5dnhrtyA3e0ThzEmUauuIGuNsFkR1-r34ftRb8e5z8"
            }
          },
          "exp": 1896017881,
          "iat": 1745934293,
          "first_name": "John",
          "degrees": [
            {
              "type": "Bachelor of Science",
              "university": "University of Betelgeuse"
            },
            {
              "type": "Master of Science",
              "university": "University of Betelgeuse"
            }
          ],
          "last_name": "Doe",
          "address": {
            "postal_code": "12345",
            "street_address": "42 Market Street"
          },
          "nationalities": [
            "British",
            "Betelgeusian"
          ]
        }
        """;
}
