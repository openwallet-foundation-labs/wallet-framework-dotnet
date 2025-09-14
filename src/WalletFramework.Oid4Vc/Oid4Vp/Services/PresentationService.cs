using LanguageExt;
using Microsoft.IdentityModel.Tokens;
using OneOf;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Response;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocLib.Security;
using WalletFramework.MdocVc;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.DcApi.Models;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.CredentialQueries;
using WalletFramework.Oid4Vc.Oid4Vp.Jwk;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;
using WalletFramework.Storage;

namespace WalletFramework.Oid4Vc.Oid4Vp.Services;

/// <inheritdoc />
public class PresentationService : IPresentationService
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PresentationService" /> class.
    /// </summary>
    /// <param name="sdJwtVcHolderService">The service responsible for SD-JWT related operations.</param>
    /// <param name="mdocAuthenticationService">The mdoc authentication service.</param>
    /// <param name="verifierKeyService">The verifier key service.</param>
    /// <param name="sdJwtRepository">The service responsible for SD-JWT storage operations.</param>
    public PresentationService(
        ISdJwtVcHolderService sdJwtVcHolderService,
        IMdocAuthenticationService mdocAuthenticationService,
        IVerifierKeyService verifierKeyService,
        IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> sdJwtRepository)
    {
        _sdJwtVcHolderService = sdJwtVcHolderService;
        _mdocAuthenticationService = mdocAuthenticationService;
        _verifierKeyService = verifierKeyService;
        _sdJwtRepository = sdJwtRepository;
    }


    private readonly ISdJwtVcHolderService _sdJwtVcHolderService;
    private readonly IMdocAuthenticationService _mdocAuthenticationService;
    private readonly IVerifierKeyService _verifierKeyService;
    private readonly IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId> _sdJwtRepository;

    /// <inheritdoc />
    public async Task<(List<(PresentationMap PresentationMap, ICredential PresentedCredential)> Presentations, Option<Nonce> MdocNonce)> CreatePresentations(
        AuthorizationRequest authorizationRequest,
        IEnumerable<SelectedCredential> selectedCredentials,
        Option<Origin> origin)
    {
        var credentials = selectedCredentials.ToList();

        var mdocNonce = Option<Nonce>.None;
        var presentations = new List<(PresentationMap PresentationMap, ICredential PresentedCredential)>();
        foreach (var credential in credentials)
        {
            var credentialRequirement =
                authorizationRequest.Requirements.Match<OneOf<CredentialQuery, InputDescriptor>>(
                    dcqlQuery =>
                        dcqlQuery.CredentialQueries.Single(credentialQuery =>
                            credentialQuery.Id == credential.Identifier),
                    presentationDefinition =>
                        presentationDefinition.InputDescriptors.Single(inputDescriptor =>
                            inputDescriptor.Id == credential.Identifier));

            var credentialRequirementId = credentialRequirement.Match(
                credentialQuery => credentialQuery.Id,
                inputDescriptor => inputDescriptor.Id);

            var claims = credentialRequirement.Match(
                credential.GetClaimsToDiscloseAsStrs,
                inputDescriptor => inputDescriptor.GetRequestedAttributes()
            );

            var txDataBase64UrlStringsOption = credential
                .Uc5TransactionData
                .OnSome(list => list.Select(data => data.Encoded.AsString));

            var txDataHashesOption = credential
                .TransactionData
                .OnSome(list =>
                {
                    return list.Select(txData =>
                    {
                        var hashesAlg = txData.GetHashesAlg().First();
                        return txData.Hash(hashesAlg);
                    });
                });

            var txDataHashesAsHexOption = txDataHashesOption
                .OnSome(hashes => hashes.Select(hash => hash.AsHex));

            var txDataHashesAlgOption = txDataHashesOption
                .OnSome(hashes => hashes.First().Alg.AsString);

            Format format;
            ICredential presentedCredential;

            var audience = authorizationRequest.ResponseMode switch
            {
                AuthorizationRequest.DcApi or AuthorizationRequest.DcApiJwt => origin.Match(
                    aud => $"origin:{aud}",
                    () => "origin:" + authorizationRequest.ClientId),
                AuthorizationRequest.DirectPost or AuthorizationRequest.DirectPostJwt => authorizationRequest
                    .ClientIdScheme?.AsString() + ":" + authorizationRequest.ClientId,
                _ => throw new ArgumentOutOfRangeException(nameof(authorizationRequest.ResponseMode))
            };

            string presentation;
            switch (credential.Credential)
            {
                case SdJwtCredential sdJwt:
                    format = Format.ValidFormat(sdJwt.Format).UnwrapOrThrow();
                    
                    presentation = await _sdJwtVcHolderService.CreatePresentation(
                        sdJwt,
                        [.. claims],
                        txDataBase64UrlStringsOption,
                        txDataHashesAsHexOption,
                        txDataHashesAlgOption,
                        audience,
                        authorizationRequest.Nonce);

                    presentedCredential = sdJwt;
                    break;
                case MdocCredential mdocCredential:
                    format = FormatFun.CreateMdocFormat();

                    var toDisclose = claims.Select(claim =>
                        {
                            // TODO: This is needed because in mdoc the requested attributes look like this: $['Namespace']['ElementId']. Refactor this more clean
                            var keys = claim.Split(["['", "']"], StringSplitOptions.RemoveEmptyEntries);

                            var nameSpace = NameSpace.ValidNameSpace(keys[0]).UnwrapOrThrow();
                            var elementId = ElementIdentifier
                                .ValidElementIdentifier(keys[1])
                                .UnwrapOrThrow();

                            return (NameSpace: nameSpace, ElementId: elementId);
                        })
                        .GroupBy(nameSpaceAndElementId => nameSpaceAndElementId.NameSpace, tuple => tuple.ElementId)
                        .ToDictionary(group => group.Key, group => group.ToList());

                    var mdoc = mdocCredential.Mdoc.SelectivelyDisclose(toDisclose);

                    SessionTranscript sessionTranscript;
                    switch (authorizationRequest.ResponseMode)
                    {
                        case AuthorizationRequest.DcApi:
                            var unencryptedVpDcApiHandover = Handover.FromAuthorizationRequest(
                                authorizationRequest,
                                origin.UnwrapOrThrow(),
                                Option<JsonWebKey>.None);
                            
                            sessionTranscript = unencryptedVpDcApiHandover.ToSessionTranscript();
                            break;
                        case AuthorizationRequest.DcApiJwt:
                            var encryptedVpDcApiHandover = Handover.FromAuthorizationRequest(
                                authorizationRequest,
                                origin.UnwrapOrThrow(),
                                await _verifierKeyService.GetPublicKey(authorizationRequest));
                            
                            sessionTranscript = encryptedVpDcApiHandover.ToSessionTranscript();
                            mdocNonce = encryptedVpDcApiHandover.GetMdocNonce();
                            break;
                        case AuthorizationRequest.DirectPost:
                            var unencryptedVpHandovers = Handover.FromAuthorizationRequest(
                                authorizationRequest,
                                Option<Origin>.None,
                                Option<JsonWebKey>.None);
                            
                            sessionTranscript = unencryptedVpHandovers.ToSessionTranscript();
                            break;
                        case AuthorizationRequest.DirectPostJwt:
                            var encryptedVpHandovers = Handover.FromAuthorizationRequest(
                                authorizationRequest,
                                Option<Origin>.None,
                                await _verifierKeyService.GetPublicKey(authorizationRequest));
                            
                            sessionTranscript = encryptedVpHandovers.ToSessionTranscript();
                            mdocNonce = encryptedVpHandovers.GetMdocNonce();
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(authorizationRequest.ResponseMode));
                    }

                    var authenticatedMdoc = await _mdocAuthenticationService.Authenticate(
                        mdoc, sessionTranscript, mdocCredential.KeyId);

                    presentation = new Document(authenticatedMdoc).BuildDeviceResponse().EncodeToBase64Url();

                    presentedCredential = mdocCredential;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(credential.Credential));
            }

            presentations.Add((new PresentationMap(credentialRequirementId, presentation, format), presentedCredential));
        }

        return (presentations, mdocNonce);
    }
}
