namespace WalletFramework.SdJwtLib.Tests.Examples;

public class Example5 : BaseExample
{
    public override int NumberOfDisclosures => 0;
    
    public override string IssuedSdJwt => 
        "eyJraWQiOiI4MDliZWU2N2FlNDM1ZjUwMjQ2MWEwMTM4YjVlMjY5OCIsInR5cCI6InZjK3NkLWp3dCIsImFsZyI6IkVTMjU2In0.eyJuYmYiOjE3MDY1NDI2ODEsInZjdCI6Imh0dHBzOi8vc29tZS5pc3N1ZXIuY29tL3ZjdCIsImliYW4iOiJERTg5MzcwNDAwNDQwNTMyMDEzMDAwIiwiaXNzIjoiaHR0cHM6Ly9zb21lLmlzc3Vlci5jb20iLCJjbmYiOnsiandrIjp7Imt0eSI6IkVDIiwiY3J2IjoiUC0yNTYiLCJ4IjoiYkNGQlpTMFkyWFFnaklHYnJDdXd0Z2xJRU5jNDNySzVjM0MtRmo1WEpZYyIsInkiOiI0elhhZTNoeFAyMDlDelV6aDIyOHFtTmZBUEp1eHVYZEl4dDFHeEt1Z2tjIn19LCJleHAiOjE3MzgyNTE0ODEsImlhdCI6MTcyNTU0NjAxNCwic3RhdHVzIjp7ImlkeCI6IjI2MCIsInVyaSI6Imh0dHBzOi8vc29tZS5pc3N1ZXIuY29tL3N0YXR1cy1saXN0cz9yZWdpc3RyeUlkPTVlNmIyZjVlLTg3NmItNDZkNS05ZDgxLTRjNjVlZDhmMzc1YSJ9fQ.yfYRpOO4k-2IQTmydXExEOtBoGvAfdKgBBQ9WX6rPelg40Rccl_8c0Raym2HUghZBVkF6fxZDE1AXIitkZb6Ag~";
    
    public override string UnsecuredPayload => """
                                               {
                                                 "nbf": 1706542681,
                                                 "vct": "https://some.issuer.com/vct",
                                                 "iban": "DE89370400440532013000",
                                                 "iss": "https://some.issuer.com",
                                                 "cnf": {
                                                   "jwk": {
                                                     "kty": "EC",
                                                     "crv": "P-256",
                                                     "x": "bCFBZS0Y2XQgjIGbrCuwtglIENc43rK5c3C-Fj5XJYc",
                                                     "y": "4zXae3hxP209CzUzh228qmNfAPJuxuXdIxt1GxKugkc"
                                                   }
                                                 },
                                                 "exp": 1738251481,
                                                 "iat": 1725546014,
                                                 "status": {
                                                   "idx": "260",
                                                   "uri": "https://some.issuer.com/status-lists?registryId=5e6b2f5e-876b-46d5-9d81-4c65ed8f375a"
                                                 }
                                               }
                                               """;
    
    public override string SecuredPayload => """
                                            {
                                              "nbf": 1706542681,
                                              "vct": "https://some.issuer.com/vct",
                                              "iban": "DE89370400440532013000",
                                              "iss": "https://some.issuer.com",
                                              "cnf": {
                                                "jwk": {
                                                  "kty": "EC",
                                                  "crv": "P-256",
                                                  "x": "bCFBZS0Y2XQgjIGbrCuwtglIENc43rK5c3C-Fj5XJYc",
                                                  "y": "4zXae3hxP209CzUzh228qmNfAPJuxuXdIxt1GxKugkc"
                                                }
                                              },
                                              "exp": 1738251481,
                                              "iat": 1725546014,
                                              "status": {
                                                "idx": "260",
                                                "uri": "https://some.issuer.com/status-lists?registryId=5e6b2f5e-876b-46d5-9d81-4c65ed8f375a"
                                              }
                                            }
                                            """;
}
