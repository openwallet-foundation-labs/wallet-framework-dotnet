using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct ContactPhone
{
    private string Value { get; }

    private ContactPhone(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(ContactPhone phone) => phone.Value;

    public static Validation<ContactPhone> ValidContactPhone(string phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return new StringIsNullOrWhitespaceError<ContactPhone>();
        }

        return new ContactPhone(phone);
    }
}
