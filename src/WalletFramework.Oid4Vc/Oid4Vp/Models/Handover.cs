using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Security;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Handover
{
    private Handover(OneOf<OpenId4VpHandover, OpenId4VpDcApiHandover> handover) => Value = handover;
    
    private OneOf<OpenId4VpHandover, OpenId4VpDcApiHandover> Value { get; }

    public static Handover FromAuthorizationRequest(AuthorizationRequest request, Option<Origin> origin, Option<JsonWebKey> verifierPublicKey)
    {
        return request.ResponseMode switch
        {
            AuthorizationRequest.DcApi or AuthorizationRequest.DcApiJwt =>
                new Handover(new OpenId4VpDcApiHandover(new OpenId4VpDcApiHandoverInfo(
                    origin.UnwrapOrThrow(),
                    request.Nonce,
                    verifierPublicKey.OnSome(JwkFun.GetThumbprint)))),
    
            AuthorizationRequest.DirectPost or AuthorizationRequest.DirectPostJwt =>
                new Handover(new OpenId4VpHandover(new OpenId4VpHandoverInfo(
                    request.ClientIdScheme != null
                        ? $"{request.ClientIdScheme.AsString()}:{request.ClientId}"
                        : request.ClientId!,
                    request.Nonce,
                    request.ResponseUri,
                    verifierPublicKey.OnSome(JwkFun.GetThumbprint)))),

            _ => throw new ArgumentException($"Unsupported response mode: {request.ResponseMode}")
        };
    }

    public Nonce GetMdocNonce()
    {
        return Value.Match(
            vpHandover => vpHandover.MdocGeneratedNonce,
            vpDcApiHandover => vpDcApiHandover.MdocGeneratedNonce
        );
    }
    
    public SessionTranscript ToSessionTranscript()
    {
        return Value.Match(
            vpHandover => vpHandover.ToSessionTranscript(),
            vpDcApiHandover => vpDcApiHandover.ToSessionTranscript()
            );
    }
}
