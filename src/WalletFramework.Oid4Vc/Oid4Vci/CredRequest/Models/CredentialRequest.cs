using LanguageExt;
using Newtonsoft.Json.Linq;
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
    public Option<ProofOfPossession> Proof { get; } = Proof;

    /// <summary>
    ///     Gets the format of the credential to be issued.
    /// </summary>
    public Format Format { get; } = Format;
}

public static class CredentialRequestFun
{
    private const string ProofJsonKey = "proof";
    private const string FormatJsonKey = "format";
    
    public static JObject EncodeToJson(this CredentialRequest request)
    {
        var result = new JObject();

        request.Proof.IfSome(proof =>
        {
            result.Add(ProofJsonKey, JObject.FromObject(proof));
        });
        
        result.Add(FormatJsonKey, request.Format.ToString());

        return result;
    }
}
