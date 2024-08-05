using PeterO.Cbor;
using WalletFramework.Core.Encoding;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Security.Abstractions;
using static System.Text.Encoding;
using static WalletFramework.Core.Encoding.Sha256Hash;
using static WalletFramework.Oid4Vc.Oid4Vp.Models.Nonce;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Oid4VpHandover(
    Sha256Hash ClientIdSha256Hash,
    Sha256Hash ResponseUriSha256Hash,
    string Nonce,
    Nonce MdocGeneratedNonce) : IHandover
{
    public CBORObject ToCbor()
    {
        var result = CBORObject.NewArray();
        
        result.Add(ClientIdSha256Hash.AsByteString);
        result.Add(ResponseUriSha256Hash.AsByteString);
        result.Add(Nonce);

        return result;
    }
}

public static class Oid4VpHandoverFun
{
    public static Oid4VpHandover ToVpHandover(this AuthorizationRequest request)
    {
        var mdocGeneratedNonce = GenerateNonce();
        var generatedNonceBytes = mdocGeneratedNonce.ToByteString();

        var clientIdBytes = UTF8.GetBytes(request.ClientId);
        var responseUriBytes = UTF8.GetBytes(request.ResponseUri);

        var clientIdToHash = CBORObject.NewArray();
        var clientId = CBORObject.FromObject(request.ClientId);
        clientIdToHash.Add(clientId);
        clientIdToHash.Add(mdocGeneratedNonce.ToString());
        var clientIdToHashBytes = clientIdToHash.EncodeToBytes();
        
        var responseUriToHash = CBORObject.NewArray();
        var responseUri = CBORObject.FromObject(request.ResponseUri);
        responseUriToHash.Add(responseUri);
        responseUriToHash.Add(mdocGeneratedNonce.ToString());
        var responseUriToHashBytes = responseUriToHash.EncodeToBytes();

        // var clientIdHash = ComputeHash(clientIdBytes, generatedNonceBytes);
        // var responseUriHash = ComputeHash(responseUriBytes, generatedNonceBytes);
        var clientIdHash = ComputeHash(clientIdToHashBytes);
        var responseUriHash = ComputeHash(responseUriToHashBytes);

        // TODO: Remove this when AuthorizationRequest has strong type nonce
        // var nonce = ValidNonce(request.Nonce).UnwrapOrThrow();

        return new Oid4VpHandover(clientIdHash, responseUriHash, request.Nonce, mdocGeneratedNonce);
    }
}
