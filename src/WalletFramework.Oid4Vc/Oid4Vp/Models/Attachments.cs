using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using Org.BouncyCastle.X509;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;
using Unit = System.Reactive.Unit;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Attachment
{
    public string Format { get; }
    
    public OneOf<RegistrationCertificate, Unit> Data { get; }

    public IEnumerable<string> CredentialIds { get; }

    [JsonConstructor]
    private Attachment(
        string format,
        string data,
        IEnumerable<string> credentialIds)
    {
        Format = format;
        Data = format switch
        {
            Constants.RegistrationCertificateFormat => RegistrationCertificate.FromJwtToken(data).UnwrapOrThrow(),
            _ => Unit.Default
        };
        CredentialIds = credentialIds;
    }
}
