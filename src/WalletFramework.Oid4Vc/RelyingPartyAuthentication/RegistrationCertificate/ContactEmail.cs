using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public readonly struct ContactEmail
{
    private string Value { get; }

    private ContactEmail(string value) => Value = value;

    public override string ToString() => Value;

    public static implicit operator string(ContactEmail email) => email.Value;

    public static Validation<ContactEmail> ValidContactEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return new StringIsNullOrWhitespaceError<ContactEmail>();
        }

        return new ContactEmail(email);
    }
}
