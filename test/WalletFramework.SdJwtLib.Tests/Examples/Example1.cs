namespace WalletFramework.SdJwtLib.Tests.Examples;

/*
 * This example is taken from the SD-JWT spec version 8
 * https://datatracker.ietf.org/doc/html/draft-ietf-oauth-selective-disclosure-jwt-08#name-example-1-sd-jwt
 */
public class Example1 : BaseExample
{
    public override int NumberOfDisclosures => 10;

    public override string IssuedSdJwt =>
        "eyJhbGciOiAiRVMyNTYiLCAidHlwIjogImV4YW1wbGUrc2Qtand0In0.eyJfc2QiOiBbIkNyUWU3UzVrcUJBSHQtbk1ZWGdjNmJkdDJTSDVhVFkxc1VfTS1QZ2tqUEkiLCAiSnpZakg0c3ZsaUgwUjNQeUVNZmVadTZKdDY5dTVxZWhabzdGN0VQWWxTRSIsICJQb3JGYnBLdVZ1Nnh5bUphZ3ZrRnNGWEFiUm9jMkpHbEFVQTJCQTRvN2NJIiwgIlRHZjRvTGJnd2Q1SlFhSHlLVlFaVTlVZEdFMHc1cnREc3JaemZVYW9tTG8iLCAiWFFfM2tQS3QxWHlYN0tBTmtxVlI2eVoyVmE1TnJQSXZQWWJ5TXZSS0JNTSIsICJYekZyendzY002R242Q0pEYzZ2Vks4QmtNbmZHOHZPU0tmcFBJWmRBZmRFIiwgImdiT3NJNEVkcTJ4Mkt3LXc1d1BFemFrb2I5aFYxY1JEMEFUTjNvUUw5Sk0iLCAianN1OXlWdWx3UVFsaEZsTV8zSmx6TWFTRnpnbGhRRzBEcGZheVF3TFVLNCJdLCAiaXNzIjogImh0dHBzOi8vaXNzdWVyLmV4YW1wbGUuY29tIiwgImlhdCI6IDE2ODMwMDAwMDAsICJleHAiOiAxODgzMDAwMDAwLCAic3ViIjogInVzZXJfNDIiLCAibmF0aW9uYWxpdGllcyI6IFt7Ii4uLiI6ICJwRm5kamtaX1ZDem15VGE2VWpsWm8zZGgta284YUlLUWM5RGxHemhhVllvIn0sIHsiLi4uIjogIjdDZjZKa1B1ZHJ5M2xjYndIZ2VaOGtoQXYxVTFPU2xlclAwVmtCSnJXWjAifV0sICJfc2RfYWxnIjogInNoYS0yNTYiLCAiY25mIjogeyJqd2siOiB7Imt0eSI6ICJFQyIsICJjcnYiOiAiUC0yNTYiLCAieCI6ICJUQ0FFUjE5WnZ1M09IRjRqNFc0dmZTVm9ISVAxSUxpbERsczd2Q2VHZW1jIiwgInkiOiAiWnhqaVdXYlpNUUdIVldLVlE0aGJTSWlyc1ZmdWVjQ0U2dDRqVDlGMkhaUSJ9fX0.RIOaMsGF0sU1W7vmSHgN6P_70Ziz0c0p1GKgJt2-T23YRDLVulwWMScbxLoIno6Aae3i7KuCjCP3hIlI2sVpKw~WyIyR0xDNDJzS1F2ZUNmR2ZyeU5STjl3IiwgImdpdmVuX25hbWUiLCAiSm9obiJd~WyJlbHVWNU9nM2dTTklJOEVZbnN4QV9BIiwgImZhbWlseV9uYW1lIiwgIkRvZSJd~WyI2SWo3dE0tYTVpVlBHYm9TNXRtdlZBIiwgImVtYWlsIiwgImpvaG5kb2VAZXhhbXBsZS5jb20iXQ~WyJlSThaV205UW5LUHBOUGVOZW5IZGhRIiwgInBob25lX251bWJlciIsICIrMS0yMDItNTU1LTAxMDEiXQ~WyJRZ19PNjR6cUF4ZTQxMmExMDhpcm9BIiwgInBob25lX251bWJlcl92ZXJpZmllZCIsIHRydWVd~WyJBSngtMDk1VlBycFR0TjRRTU9xUk9BIiwgImFkZHJlc3MiLCB7InN0cmVldF9hZGRyZXNzIjogIjEyMyBNYWluIFN0IiwgImxvY2FsaXR5IjogIkFueXRvd24iLCAicmVnaW9uIjogIkFueXN0YXRlIiwgImNvdW50cnkiOiAiVVMifV0~WyJQYzMzSk0yTGNoY1VfbEhnZ3ZfdWZRIiwgImJpcnRoZGF0ZSIsICIxOTQwLTAxLTAxIl0~WyJHMDJOU3JRZmpGWFE3SW8wOXN5YWpBIiwgInVwZGF0ZWRfYXQiLCAxNTcwMDAwMDAwXQ~WyJsa2x4RjVqTVlsR1RQVW92TU5JdkNBIiwgIlVTIl0~WyJuUHVvUW5rUkZxM0JJZUFtN0FuWEZBIiwgIkRFIl0~";

    public override string UnsecuredPayload => """
                                               {
                                                 "iss": "https://issuer.example.com",
                                                 "iat": 1683000000,
                                                 "exp": 1883000000,
                                                 "sub": "user_42",
                                                 "nationalities": [
                                                   "US",
                                                   "DE"
                                                 ],
                                                 "cnf": {
                                                   "jwk": {
                                                     "kty": "EC",
                                                     "crv": "P-256",
                                                     "x": "TCAER19Zvu3OHF4j4W4vfSVoHIP1ILilDls7vCeGemc",
                                                     "y": "ZxjiWWbZMQGHVWKVQ4hbSIirsVfuecCE6t4jT9F2HZQ"
                                                   }
                                                 },
                                                 "updated_at": 1570000000,
                                                 "email": "johndoe@example.com",
                                                 "phone_number": "+1-202-555-0101",
                                                 "family_name": "Doe",
                                                 "phone_number_verified": true,
                                                 "address": {
                                                   "street_address": "123 Main St",
                                                   "locality": "Anytown",
                                                   "region": "Anystate",
                                                   "country": "US"
                                                 },
                                                 "birthdate": "1940-01-01",
                                                 "given_name": "John"
                                               }
                                               """;

    public override string SecuredPayload => """
                                             {
                                               "_sd": [
                                                 "CrQe7S5kqBAHt-nMYXgc6bdt2SH5aTY1sU_M-PgkjPI",
                                                 "JzYjH4svliH0R3PyEMfeZu6Jt69u5qehZo7F7EPYlSE",
                                                 "PorFbpKuVu6xymJagvkFsFXAbRoc2JGlAUA2BA4o7cI",
                                                 "TGf4oLbgwd5JQaHyKVQZU9UdGE0w5rtDsrZzfUaomLo",
                                                 "XQ_3kPKt1XyX7KANkqVR6yZ2Va5NrPIvPYbyMvRKBMM",
                                                 "XzFrzwscM6Gn6CJDc6vVK8BkMnfG8vOSKfpPIZdAfdE",
                                                 "gbOsI4Edq2x2Kw-w5wPEzakob9hV1cRD0ATN3oQL9JM",
                                                 "jsu9yVulwQQlhFlM_3JlzMaSFzglhQG0DpfayQwLUK4"
                                               ],
                                               "iss": "https://issuer.example.com",
                                               "iat": 1683000000,
                                               "exp": 1883000000,
                                               "sub": "user_42",
                                               "nationalities": [
                                                 {
                                                   "...": "pFndjkZ_VCzmyTa6UjlZo3dh-ko8aIKQc9DlGzhaVYo"
                                                 },
                                                 {
                                                   "...": "7Cf6JkPudry3lcbwHgeZ8khAv1U1OSlerP0VkBJrWZ0"
                                                 }
                                               ],
                                               "_sd_alg": "sha-256",
                                               "cnf": {
                                                 "jwk": {
                                                   "kty": "EC",
                                                   "crv": "P-256",
                                                   "x": "TCAER19Zvu3OHF4j4W4vfSVoHIP1ILilDls7vCeGemc",
                                                   "y": "ZxjiWWbZMQGHVWKVQ4hbSIirsVfuecCE6t4jT9F2HZQ"
                                                 }
                                               }
                                             }
                                             """;
}
