using LanguageExt;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Models;

public record DPopConfig
{
    internal KeyId KeyId { get; }
    
    internal string Audience { get; init; }

    internal Option<DPopNonce> Nonce { get; init; }
    
    internal Option<OAuthToken> OAuthToken { get; init; }

    internal DPopConfig(KeyId keyId, string audience)
    {
        KeyId = keyId;
        Audience = audience;
        Nonce = Option<DPopNonce>.None;
        OAuthToken = Option<OAuthToken>.None;
    }
}
