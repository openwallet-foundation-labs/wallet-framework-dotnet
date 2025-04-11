using System.Reactive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Attachment
{
    [JsonProperty("format")]
    public string Format { get; }
    
    public OneOf<RegistrationCertificate, Unit> Payload { get; private set; }

    public IEnumerable<string> CredentialIds { get; }

    [JsonConstructor]
    private Attachment(
        string format,
        JToken data,
        IEnumerable<string> credentialIds)
    {
        Format = format;
        Payload = Format switch
        {
            Constants.RegistrationCertificateFormat => JObject.Parse(data.ToString())
                .GetByKey("payload")
                .OnSuccess(token => token.ToJObject())
                .OnSuccess(RegistrationCertificate.FromJObject)
                .UnwrapOrThrow(),
            _ => Unit.Default
        };
        CredentialIds = credentialIds;
    }
}
