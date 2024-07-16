using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Models;

[JsonConverter(typeof(ValueTypeJsonConverter<CredentialConfigurationId, CredentialConfigurationIdDecoder>))]
public readonly struct CredentialConfigurationId
{
    private string Value { get; }

    public static implicit operator string(CredentialConfigurationId id) => id.ToString();
    
    private CredentialConfigurationId(string id) => Value = id;

    public override string ToString() => Value;

    public static Validation<CredentialConfigurationId> ValidCredentialConfigurationId(JToken id) =>
        id.ToJValue().OnSuccess(value =>
        {
            try
            {
                var str = value.ToString(CultureInfo.InvariantCulture);
                if (string.IsNullOrWhiteSpace(str))
                    return new CredentialConfigurationIdIsNullOrWhitespaceError();

                return new CredentialConfigurationId(str);
            }
            catch (Exception e)
            {
                return new CredentialConfigurationIdError(value, e).ToInvalid<CredentialConfigurationId>();
            }
        });
}

public class CredentialConfigurationIdDecoder : IValueTypeDecoder<CredentialConfigurationId>
{
    public CredentialConfigurationId Decode(JToken token) =>
        CredentialConfigurationId
            .ValidCredentialConfigurationId(token)
            .UnwrapOrThrow(new InvalidOperationException("CredentialConfigurationId is corrupt"));
}

