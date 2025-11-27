using System.Text;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.AuthRequest;

public static class AuthRequestFun
{
    public static CredentialQuery GetCredentialRequirement(
        this AuthorizationRequest authorizationRequest, string requestIdentifier)
    {
        var query = authorizationRequest.DcqlQuery.CredentialQueries.Single(
            credentialQuery => credentialQuery.Id == requestIdentifier);

        return query;
    }

    public static CredentialQuery GetRequirementById(
        this AuthorizationRequest authorizationRequest,
        string requestIdentifier)
    {
        var query = authorizationRequest.DcqlQuery.CredentialQueries.Single(
            credentialQuery => credentialQuery.Id == requestIdentifier);
        return query;
    }

    public static string FormatForLog(this DcqlQuery dcqlQuery)
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
    }
}
