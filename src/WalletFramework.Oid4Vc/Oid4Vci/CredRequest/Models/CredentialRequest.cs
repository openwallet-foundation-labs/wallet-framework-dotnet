using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models;

/// <summary>
///     Represents a credential request made by a client to the Credential Endpoint.
///     This request contains the format of the credential, the type of credential,
///     and a proof of possession of the key material the issued credential shall be bound to.
/// </summary>
public record CredentialRequest(Option<ProofOfPossession> Proof, Format Format)
{
    /// <summary>
    ///     Gets the proof of possession of the key material the issued credential shall be bound to.
    /// </summary>
    [JsonProperty("proof")]
    public Option<ProofOfPossession> Proof { get; } = Proof;

    /// <summary>
    ///     Gets the format of the credential to be issued.
    /// </summary>
    [JsonProperty("format")]
    public Format Format { get; } = Format;
}
