using LanguageExt;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Base64Url;
using WalletFramework.MdocLib.Security;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Models;

/// <summary>
///     Represents a credential request made by a client to the Credential Endpoint.
///     This request contains the format of the credential, the type of credential,
///     and a proof of possession of the key material the issued credential shall be bound to.
/// </summary>
public record CredentialRequest(OneOf<CredentialIdentifier, CredentialConfigurationId> CredentialIdentification, Format Format, int specVersion, Option<ProofOfPossession> Proof, Option<ProofsOfPossession> Proofs, Option<SessionTranscript> SessionTranscript)
{
    /// <summary>
    ///     Gets the proof of possession of the key material the issued credential shall be bound to.
    /// </summary>
    public Option<ProofOfPossession> Proof { get; } = Proof;
    
    /// <summary>
    ///     Gets one or more proof of possessions of the key material the issued credential shall be bound to.
    /// </summary>
    public Option<ProofsOfPossession> Proofs { get; } = Proofs;

    //TODO: Remove when backward compatibility is not needed anymore
    /// <summary>
    ///     Gets the format of the credential to be issued.
    /// </summary>
    public Format Format { get; } = Format;

    public Option<SessionTranscript> SessionTranscript { get; } = SessionTranscript;
    
    public OneOf<CredentialIdentifier, CredentialConfigurationId> CredentialIdentification { get; } = CredentialIdentification;
    
    public int SpecVersion { get; } = specVersion;
}

public static class CredentialRequestFun
{
    private const string ProofJsonKey = "proof";
    private const string ProofsJsonKey = "proofs";
    private const string FormatJsonKey = "format";
    private const string SessionTranscriptKey = "session_transcript";
    private const string CredentialIdentifierKey = "credential_identifier";
    private const string CredentialConfigurationIdKey = "credential_configuration_id";
    
    public static JObject EncodeToJson(this CredentialRequest request)
    {
        var result = new JObject();

        request.Proof.IfSome(proof =>
        {
            result.Add(ProofJsonKey, JObject.FromObject(proof));
        });
        
        request.Proofs.IfSome(proofs =>
        {
            result.Add(ProofsJsonKey, proofs.EncodeToJson());
        });
        
        request.SessionTranscript.IfSome(sessionTranscript =>
        {
            result.Add(SessionTranscriptKey, Base64UrlString.CreateBase64UrlString(sessionTranscript.ToCbor().ToJSONBytes()).ToString());
        });
        
        request.CredentialIdentification.Match(
            identifier =>
            {
                result.Add(CredentialIdentifierKey, identifier.ToString());
                return Unit.Default;
            },
            configurationId =>
            {
                if (request.SpecVersion == 15)
                    result.Add(CredentialConfigurationIdKey, configurationId.ToString());
                else
                    result.Add(FormatJsonKey, request.Format.ToString());
                
                return Unit.Default;
            });
            
        return result;
    }
}
