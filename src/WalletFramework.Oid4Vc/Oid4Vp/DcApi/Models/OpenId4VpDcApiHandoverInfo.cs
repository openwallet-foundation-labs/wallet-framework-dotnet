using LanguageExt;
using PeterO.Cbor;
using WalletFramework.Core.Encoding;

namespace WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;

/// <summary>
/// Represents OpenID4VPDCAPIHandoverInfo according to the OpenID4VP specification.
/// Contains the handover parameters as a CBOR array: [origin, nonce, jwkThumbprint]
/// </summary>
public record OpenId4VpDcApiHandoverInfo(
    Origin Origin,
    string Nonce,
    Option<byte[]> JwkThumbprint)
{
    /// <summary>
    /// Converts the handover info to CBOR representation as an array
    /// </summary>
    /// <returns>CBOR array containing [origin, nonce, jwkThumbprint]</returns>
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewArray();
        
        result.Add(Origin.AsString());
        result.Add(Nonce);

        JwkThumbprint.Match(
            thumbprint => result.Add(thumbprint),
            () => result.Add(CBORObject.Null)
        );
        
        return result;
    }
    
    /// <summary>
    /// Encodes the handover info as CBOR bytes
    /// </summary>
    /// <returns>CBOR-encoded bytes of the handover info</returns>
    public byte[] ToCborBytes() => ToCbor().EncodeToBytes();
    
    /// <summary>
    /// Computes the SHA-256 hash of the handover info CBOR bytes
    /// </summary>
    /// <returns>SHA-256 hash of the CBOR-encoded handover info</returns>
    public Sha256Hash ToHash()
    {
        var handoverInfoBytes = ToCborBytes();
        return Sha256Hash.ComputeHash(handoverInfoBytes);
    }
} 
