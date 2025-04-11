using System.Globalization;
using System.Reactive;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneOf;
using Org.BouncyCastle.X509;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

public record Attachment
{
    [JsonProperty("format")]
    public string Format { get; }
    
    public IEnumerable<X509Certificate> Certificates { get; private set; }
    
    public OneOf<RegistrationCertificate, Unit> Payload { get; private set; }

    public IEnumerable<string> CredentialIds { get; }

    [JsonConstructor]
    private Attachment(
        string format,
        JToken data,
        IEnumerable<string> credentialIds)
    {
        var jObject = JObject.Parse(data.ToString());
        
        Format = format;
        Payload = Format switch
        {
            Constants.RegistrationCertificateFormat => jObject
                .GetByKey("payload")
                .OnSuccess(token => token.ToJObject())
                .OnSuccess(RegistrationCertificate.FromJObject)
                .UnwrapOrThrow(),
            _ => Unit.Default
        };
        CredentialIds = credentialIds;

        Certificates = jObject.GetByKey("protected")
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(jObject => jObject.GetByKey("x5c"))
            .OnSuccess(x5c => x5c.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJValue()))
            .OnSuccess(x =>
            {
                var parser = new X509CertificateParser();
                return x.Select(jToken =>
                    parser.ReadCertificate(Convert.FromBase64String(jToken.ToString(CultureInfo.InvariantCulture))));
            })
            .UnwrapOrThrow();
    }
}
