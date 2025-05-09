using LanguageExt;
using WalletFramework.Core.X509;
using WalletFramework.Oid4Vc.Dcql.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.RelyingPartyAuthentication.RegistrationCertificate;

public record OverAskingValidationResult
{
    public bool IsValid { get; }

    private OverAskingValidationResult(bool isValid) => IsValid = isValid;

    internal static OverAskingValidationResult Validate(RequestObject requestObject)
    {
        var authorizationRequest = requestObject.ToAuthorizationRequest();
        return authorizationRequest.Requirements.Match(
            dcqlQuery =>
            {
                var registrationCertificateAttachments = authorizationRequest
                    .VerifierAttestations?
                    .Where(attachment => attachment.Format == Constants.RegistrationCertificateFormat) ?? [];

                List<string> certifiedClaims = [];

                var areTrustChainsValid = true;
                foreach (var registrationCertificateAttachment in registrationCertificateAttachments)
                {
                    _ = registrationCertificateAttachment.Data.Match(
                        registrationCertificate =>
                        {
                            certifiedClaims.AddRange(
                                registrationCertificate.Credentials.SelectMany(query =>
                                {
                                    return query.GetRequestedAttributes();
                                })
                            );

                            var isValidChain = registrationCertificate.Certificates.IsTrustChainValid();
                            if (!isValidChain)
                            {
                                areTrustChainsValid = false;
                            }

                            return Unit.Default;
                        },
                        _ =>
                        {
                            areTrustChainsValid = false;
                            return Unit.Default;
                        });
                }

                if (areTrustChainsValid == false)
                    return new OverAskingValidationResult(false);

                var requestedClaims = authorizationRequest
                    .DcqlQuery!
                    .CredentialQueries
                    .SelectMany(query => query.GetRequestedAttributes());
                
                var isOverAskingClaims = !requestedClaims.All(requestedAttribute =>
                {
                    return certifiedClaims.Contains(requestedAttribute);
                });

                return new OverAskingValidationResult(!isOverAskingClaims);
            },
            _ => new OverAskingValidationResult(true)
        );
    }
}
