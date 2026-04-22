using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.RegistrationCertificate;
using Unit = System.Reactive.Unit;
using OneOf;

namespace WalletFramework.Oid4Vp.Models;

public record VerifierAttestation
{
    public IEnumerable<string> CredentialIds { get; }

    public OneOf<RegistrationCertificate, Unit> Data { get; }
    
    public string Format { get; }

    [JsonConstructor]
    private VerifierAttestation(
        string format,
        string data,
        IEnumerable<string> credentialIds)
    {
        Format = format;
        Data = format switch
        {
            Constants.RegistrationCertificateFormat => RegistrationCertificate.FromJwtTokenStr(data).UnwrapOrThrow(),
            _ => Unit.Default
        };
        CredentialIds = credentialIds;
    }
}

public class VerifierAttestationsConverter : JsonConverter<VerifierAttestation[]>
{
    public override bool CanRead => true;

    public override bool CanWrite => false;

    public override VerifierAttestation[]? ReadJson(JsonReader reader, Type objectType,
        VerifierAttestation[]? existingValue,
        bool hasExistingValue, JsonSerializer serializer)
    {
        try
        {
            var jArray = JArray.Load(reader);
            var verifierAttestations = jArray.ToObject<VerifierAttestation[]>();
            return verifierAttestations;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public override void WriteJson(JsonWriter writer, VerifierAttestation[]? value, JsonSerializer serializer) => 
        throw new NotImplementedException();
}
