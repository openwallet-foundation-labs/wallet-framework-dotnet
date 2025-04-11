using System.Collections;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.StatusList;
using WalletFramework.Oid4Vc.Dcql.Models;
using static WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate.RegistrationCertificateFun;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public record RegistrationCertificate(
    Option<IEnumerable<Purpose>> Purpose,
    IEnumerable<CredentialQuery> Credentials,
    Option<IEnumerable<CredentialSetQuery>> CredentialSets,
    Contact Contact,
    Sub Sub,
    Option<IssuedAt> IssuedAt,
    Option<StatusListEntry> Status)
{
    public static Validation<RegistrationCertificate> FromJObject(JObject json)
    {
        var purpose = json.GetByKey(PurposeJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(RelyingPartyAuthentication.RegistrationCertificate.Purpose.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x.ToOption()));

        var credentials = json.GetByKey(CredentialsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(CredentialQuery.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x));

        var credentialSets = json.GetByKey(CredentialSetsJsonKey)
            .OnSuccess(token => token.ToJArray())
            .OnSuccess(array => array.TraverseAll(jToken => jToken.ToJObject()))
            .OnSuccess(array => array.Select(CredentialSetQuery.FromJObject))
            .OnSuccess(array => array.TraverseAll(x => x))
            .ToOption();
        
        var contact = json.GetByKey(ContactJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(Contact.FromJObject);
        
        var sub = json.GetByKey(SubJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(Sub.ValidSub);
        
        var issuedAt = json.GetByKey(IssuedAtJsonKey)
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(RelyingPartyAuthentication.RegistrationCertificate.IssuedAt.ValidIssuedAt)
            .ToOption();

        var statusList = json.GetByKey(StatusJsonKey)
            .OnSuccess(token => token.ToJObject())
            .OnSuccess(StatusListEntry.FromJObject)
            .ToOption();
        
        return ValidationFun.Valid(Create)
            .Apply(purpose)
            .Apply(credentials)
            .Apply(credentialSets)
            .Apply(contact)
            .Apply(sub)
            .Apply(issuedAt)
            .Apply(statusList);
    }

    private static RegistrationCertificate Create(
        Option<IEnumerable<Purpose>> purpose,
        IEnumerable<CredentialQuery> credentials,
        Option<IEnumerable<CredentialSetQuery>> credentialSets,
        Contact contact,
        Sub sub,
        Option<IssuedAt> issuedAt,
        Option<StatusListEntry> status) =>
        new(purpose, credentials, credentialSets, contact, sub, issuedAt, status);
}

public static class RegistrationCertificateFun
{
    public const string PurposeJsonKey = "purpose";
    public const string ContactJsonKey = "contact";
    public const string SubJsonKey = "sub";
    public const string PrivacyPolicyJsonKey = "privacy_policy";
    public const string IssuedAtJsonKey = "iat";
    public const string CredentialsJsonKey = "credentials";
    public const string CredentialSetsJsonKey = "credentialSets";
    public const string StatusJsonKey = "status";
}
