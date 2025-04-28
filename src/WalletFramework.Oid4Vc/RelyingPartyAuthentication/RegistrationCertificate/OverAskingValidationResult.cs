using LanguageExt;
using WalletFramework.Core.X509;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;
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
                List<IEnumerable<string>> certifiedClaimSets = [];

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

                            var registrationCertifiedClaimSets = registrationCertificate.CredentialSets.Match(
                                credentialsSets =>
                                {
                                    return credentialsSets.SelectMany(
                                        set => set.Options ?? Enumerable.Empty<string[]>()
                                    );
                                },
                                () => []);

                            certifiedClaimSets.AddRange(registrationCertifiedClaimSets);

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

                var requestedClaimSets = authorizationRequest
                    .DcqlQuery!
                    .CredentialSetQueries?
                    .SelectMany(query => query.Options ?? []) ?? [];
                
                var isOverAskingClaimSets = !requestedClaimSets.All(requestedClaimSet =>
                {
                    return certifiedClaimSets.Any(certifiedClaimSet =>
                    {
                        return requestedClaimSet.All(certifiedClaimSet.Contains);
                    });
                });

                return new OverAskingValidationResult(!isOverAskingClaims && !isOverAskingClaimSets);
            },
            _ => new OverAskingValidationResult(true)
        );
    }
}
