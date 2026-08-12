// TODO: Add this when Client Attestation is available
// public record MdocTokenRequest
// {
//     public TokenRequest VciTokenRequest { get; }
//
//     public string ClientAssertionType => "urn:ietf:params:oauth:client-assertion-type:jwt-client-attestation";
//     
//     public ClientAssertion ClientAssertion { get; }
// }
//
// public readonly struct ClientAssertion
// {
//     public ClientAttestationJwt Jwt { get; }
//     
//     public ClientAttestationPop ClientAttestationPop { get; }
// }
//
// public readonly struct ClientAttestationJwt
// {
//     public IEnumerable<Jwk> DeviceKeys { get; }
// }
