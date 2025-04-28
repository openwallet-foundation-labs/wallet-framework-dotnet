using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using LanguageExt;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.X509;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.StatusList;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate.RegistrationCertificateFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public record RegistrationCertificate(
    Option<IEnumerable<Purpose>> Purpose,
    IEnumerable<CredentialQuery> Credentials,
    Option<IEnumerable<CredentialSetQuery>> CredentialSets,
    Contact Contact,
    Sub Sub,
    Option<IssuedAt> IssuedAt,
    Option<StatusListEntry> Status,
    IEnumerable<X509Certificate> Certificates)
{
    public static Validation<RegistrationCertificate> FromJwtTokenStr(string jwtToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwt = tokenHandler.ReadJwtToken(jwtToken);
        var jObject = JObject.Parse(jwt.Payload.SerializeToJson());
        
        var purpose = jObject.GetByKey(PurposeJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(RelyingPartyAuthentication.RegistrationCertificate.Purpose.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x.ToOption()));

        var credentials = jObject.GetByKey(CredentialsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(CredentialQuery.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x));

        var credentialSets = jObject.GetByKey(CredentialSetsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(CredentialSetQuery.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();
        
        var contact = jObject.GetByKey(ContactJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(Contact.FromJObject);
        
        var sub = jObject.GetByKey(SubJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(Sub.ValidSub);
        
        var issuedAt = jObject.GetByKey(IssuedAtJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(RelyingPartyAuthentication.RegistrationCertificate.IssuedAt.ValidIssuedAt)
            .ToOption();

        var statusList = jObject.GetByKey(StatusJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(StatusListEntry.FromJObject)
            .ToOption();
        
        var certificates = JArray.Parse(jwt.Header.X5c).ToJArray()
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJValue()))
            .OnSuccess(x =>
            {
                var parser = new X509CertificateParser();
                return x.Select(jToken =>
                    parser.ReadCertificate(Convert.FromBase64String(jToken.ToString(CultureInfo.InvariantCulture))));
            });
        
        return ValidationFun.Valid(Create)
            .Apply(purpose)
            .Apply(credentials)
            .Apply(credentialSets)
            .Apply(contact)
            .Apply(sub)
            .Apply(issuedAt)
            .Apply(statusList)
            .Apply(certificates);
    }

    private static RegistrationCertificate Create(
        Option<IEnumerable<Purpose>> purpose,
        IEnumerable<CredentialQuery> credentials,
        Option<IEnumerable<CredentialSetQuery>> credentialSets,
        Contact contact,
        Sub sub,
        Option<IssuedAt> issuedAt,
        Option<StatusListEntry> status,
        IEnumerable<X509Certificate> certificates
        ) =>
        new(purpose, credentials, credentialSets, contact, sub, issuedAt, status, certificates);
}

public static class RegistrationCertificateFun
{
    public const string PurposeJsonKey = "purpose";
    public const string ContactJsonKey = "contact";
    public const string SubJsonKey = "sub";
    public const string PrivacyPolicyJsonKey = "privacy_policy";
    public const string IssuedAtJsonKey = "iat";
    public const string CredentialsJsonKey = "credentials";
    public const string CredentialSetsJsonKey = "credential_sets";
    public const string StatusJsonKey = "status";
}
