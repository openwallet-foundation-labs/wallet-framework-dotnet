using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct ContactAddress
{
    private string Value { get; }

    private ContactAddress(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(ContactAddress address) => address.Value;

    public static Validation<ContactAddress> ValidContactAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return new StringIsNullOrWhitespaceError<ContactAddress>();
        }

        return new ContactAddress(address);
    }
}
