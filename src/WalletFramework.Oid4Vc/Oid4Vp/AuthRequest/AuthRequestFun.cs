using System.Text;
using OneOf;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Query;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthRequest;

public static class AuthRequestFun
{
    public static CredentialRequirement GetCredentialRequirement(
        this AuthorizationRequest authorizationRequest, string requestIdentifier)
    {
        var result = authorizationRequest.Requirements.Match<OneOf<CredentialQuery, InputDescriptor>>(
            dcqlQuery =>
            {
                return dcqlQuery.CredentialQueries.Single(credentialQuery =>
                    credentialQuery.Id == requestIdentifier);
            },
            presentationDefinition =>
            {
                return presentationDefinition.InputDescriptors.Single(inputDescriptor =>
                    inputDescriptor.Id == requestIdentifier);
            }
        );

        return new CredentialRequirement(result);
    }

    public static CredentialRequirement GetRequirementById(
        this AuthorizationRequest authorizationRequest,
        string requestIdentifier) =>
        authorizationRequest.Requirements.Match(
            dcqlQuery =>
            {
                var query = dcqlQuery.CredentialQueries.Single(credentialQuery =>
                    credentialQuery.Id == requestIdentifier);
                return new CredentialRequirement(query);
            },
            presentationDefinition =>
            {
                var descriptor =
                    presentationDefinition.InputDescriptors.Single(inputDescriptor =>
                        inputDescriptor.Id == requestIdentifier);
                return new CredentialRequirement(descriptor);
            }
        );

    public static string FormatForLog(this OneOf<DcqlQuery, PresentationDefinition> requirements) =>
        requirements.Match(
            dcqlQuery =>
            {
                var builder = dcqlQuery.CredentialQueries.Aggregate(
                    new StringBuilder(),
                    (sb, credentialQuery) =>
                    {
                        sb.AppendLine(credentialQuery.FormatForLog());
                        return sb;
                    }
                );
                
                return $"The DCQL Query asks for: {builder}";
            },
            presentationDefinition =>
            {
                var builder = presentationDefinition.InputDescriptors.Aggregate(
                    new StringBuilder(),
                    (sb, inputDescriptor) =>
                    {
                        sb.AppendLine(inputDescriptor.FormatForLog());
                        return sb;
                    }
                );
                
                return $"The PEX Presentation Definition asks for: {builder}";
            }
        );
}
