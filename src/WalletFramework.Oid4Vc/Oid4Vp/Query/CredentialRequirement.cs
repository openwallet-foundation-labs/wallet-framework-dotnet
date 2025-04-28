using System.Text;
using LanguageExt;
using OneOf;
using WalletFramework.MdocLib;
using WalletFramework.Oid4Vc.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.Query;

public record CredentialRequirement
{
    public CredentialRequirement(OneOf<CredentialQuery, InputDescriptor> value) => Value = value;

    private CredentialRequirement(CredentialQuery value) => Value = value;

    private CredentialRequirement(InputDescriptor value) => Value = value;

    private OneOf<CredentialQuery, InputDescriptor> Value { get; }

    public OneOf<CredentialQuery, InputDescriptor> GetQuery() => Value;

    public static implicit operator CredentialRequirement(CredentialQuery value) => new(value);

    public static implicit operator CredentialRequirement(InputDescriptor value) => new(value);
}

public static class CredentialRequirementFun
{
    public static string FormatForLog(this InputDescriptor inputDescriptor)
    {
        var result = inputDescriptor.GetRequestedAttributes().Aggregate(
            new StringBuilder(),
            (sb, requestedAttribute) =>
            {
                sb.AppendLine(requestedAttribute);
                return sb;
            }
        );

        return result.ToString();
    }

    public static string FormatForLog(this CredentialQuery query)
    {
        var result = query.GetRequestedAttributes().Aggregate(
            new StringBuilder(),
            (sb, requestedAttribute) =>
            {
                sb.AppendLine(requestedAttribute);
                return sb;
            }
        );

        return result.ToString();
    }

    public static string FormatForLog(this CredentialRequirement requirement) =>
        requirement.GetQuery().Match(
            credentialQuery => credentialQuery.FormatForLog(),
            inputDescriptor => inputDescriptor.FormatForLog()
        );

    public static string GetIdentifier(this CredentialRequirement requirement) =>
        requirement.GetQuery().Match(
            credentialQuery => credentialQuery.Id,
            inputDescriptor => inputDescriptor.Id);

    public static IEnumerable<string> GetRequestedAttributes(this CredentialRequirement requirement) =>
        requirement.GetQuery().Match(
            credentialQuery => credentialQuery.GetRequestedAttributes(),
            inputDescriptor => inputDescriptor.GetRequestedAttributes()
        );

    public static Option<OneOf<Vct, DocType>> GetRequestedCredentialType(this CredentialRequirement requirement) =>
        requirement.GetQuery().Match(
            credentialQuery => credentialQuery.GetRequestedCredentialType(),
            presentationDefinition => presentationDefinition.GetRequestedCredentialType()
        );
}
