using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Base64Url;
using WalletFramework.MdocLib.Security;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models;

/// <summary>
///     Represents a credential request made by a client to the Credential Endpoint.
///     This request contains the format of the credential, the type of credential,
///     and a proof of possession of the key material the issued credential shall be bound to.
/// </summary>
public record CredentialRequest(Format Format, Option<ProofOfPossession> Proof, Option<SessionTranscript> SessionTranscript)
{
    /// <summary>
    ///     Gets the proof of possession of the key material the issued credential shall be bound to.
    /// </summary>
    public Option<ProofOfPossession> Proof { get; } = Proof;

    /// <summary>
    ///     Gets the format of the credential to be issued.
    /// </summary>
    public Format Format { get; } = Format;

    public Option<SessionTranscript> SessionTranscript { get; } = SessionTranscript;
}

public static class CredentialRequestFun
{
    private const string ProofJsonKey = "proof";
    private const string FormatJsonKey = "format";
    private const string SessionTranscriptKey = "session_transcript";
    
    public static JObject EncodeToJson(this CredentialRequest request)
    {
        var result = new JObject();

        request.Proof.IfSome(proof =>
        {
            result.Add(ProofJsonKey, JObject.FromObject(proof));
        });
        
        request.SessionTranscript.IfSome(sessionTranscript =>
        {
            result.Add(SessionTranscriptKey, Base64UrlString.CreateBase64UrlString(sessionTranscript.ToCbor().ToJSONBytes()).ToString());
        });
        
        result.Add(FormatJsonKey, request.Format.ToString());

        return result;
    }
}
