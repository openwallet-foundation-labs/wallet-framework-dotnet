using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate.ContactFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public record Contact(ContactAddress Address, ContactEmail Email, ContactPhone Phone)
{
    public static Validation<Contact> FromJObject(JObject json)
    {
        var address = json.GetByKey(AddressJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new StringIsNullOrWhitespaceError<Contact>();
                }

                return ContactAddress.ValidContactAddress(value);
            });

        var email = json.GetByKey(EmailJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new StringIsNullOrWhitespaceError<Contact>();
                }

                return ContactEmail.ValidContactEmail(value);
            });
        
        var phone = json.GetByKey(PhoneJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(jValue =>
            {
                var value = jValue.Value?.ToString();
                if (string.IsNullOrWhiteSpace(value))
                {
                    return new StringIsNullOrWhitespaceError<Contact>();
                }

                return ContactPhone.ValidContactPhone(value);
            });

        return ValidationFun.Valid(Create)
            .Apply(address)
            .Apply(email)
            .Apply(phone);
    }
    
    private static Contact Create(
        ContactAddress address,
        ContactEmail email,
        ContactPhone phone) =>
        new(address, email, phone);
}

public static class ContactFun
{
    public const string AddressJsonKey = "address";
    public const string EmailJsonKey = "email";
    public const string PhoneJsonKey = "phone";
}
