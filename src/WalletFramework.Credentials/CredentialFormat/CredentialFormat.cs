using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;

namespace WalletFramework.Credentials;

public readonly record struct CredentialFormat
{
    private string Value { get; }

    private CredentialFormat(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(CredentialFormat credentialFormat) => credentialFormat.ToString();

    public static Validation<CredentialFormat> ValidCredentialFormat(JToken format) => format
        .ToJValue()
        .OnSuccess(value => ValidCredentialFormat(value.ToString(CultureInfo.InvariantCulture)));

    public static Validation<CredentialFormat> ValidCredentialFormat(string format) =>
        SupportedFormats.Contains(format)
            ? new CredentialFormat(format)
            : new FormatNotSupportedError(format).ToInvalid<CredentialFormat>();

    private static List<string> SupportedFormats =>
    [
        CredentialFormatConstants.SdJwtVcFormat,
        CredentialFormatConstants.SdJwtDcFormat,
        CredentialFormatConstants.MdocFormat
    ];
}
