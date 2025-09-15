using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.MdocLib.Device;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocLib.Security.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.Nonce;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
///     Represents OpenID4VPDCAPIHandover according to the OpenID4VP specification.
///     Contains a fixed identifier and the SHA-256 hash of OpenID4VPDCAPIHandoverInfo
///     Structure: ["OpenID4VPDCAPIHandover", OpenID4VPDCAPIHandoverInfoHash]
/// </summary>
public record OpenId4VpDcApiHandover(OpenId4VpDcApiHandoverInfo HandoverInfo) : IHandover
{
    /// <summary>
    ///     Mdoc generated nonce created during handover initialization
    /// </summary>
    public Nonce MdocGeneratedNonce { get; } = GenerateNonce();
    
    /// <summary>
    ///     Converts the handover to CBOR representation as an array
    /// </summary>
    /// <returns>CBOR array containing [identifier, hash]</returns>
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewArray();

        result.Add(HandoverConstants.DcApiHandoverTypeIdentifier);
        result.Add(HandoverInfo.ToHash().AsBytes);
        
        return result;
    }
    
    /// <summary>
    /// Encodes the handover as CBOR bytes
    /// </summary>
    /// <returns>CBOR-encoded bytes of the handover</returns>
    public byte[] ToCborBytes() => ToCbor().EncodeToBytes();

    public SessionTranscript ToSessionTranscript()
    {
        return new SessionTranscript(
            Option<DeviceEngagement>.None,
            Option<PublicKey>.None,
            this
        );
    }

} 
