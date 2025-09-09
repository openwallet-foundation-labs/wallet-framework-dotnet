using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Events;
using WalletFramework.Core.StatusList;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Implementations;
using WalletFramework.MdocLib.Security.Cose.Abstractions;
using WalletFramework.MdocLib.Security.Cose.Implementations;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vc.ClientAttestation;
using WalletFramework.Oid4Vc.CredentialSet;
using WalletFramework.Oid4Vc.CredentialSet.Models;
using WalletFramework.Oid4Vc.CredentialSet.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredentialNonce.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Persistence;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.Abstractions;
using WalletFramework.Oid4Vc.RelyingPartyAuthentication.Implementations;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.SdJwtVc.Services;
using WalletFramework.Storage;
using WalletFramework.Storage.Records;

namespace WalletFramework.Oid4Vc.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the default OpenID services.
    /// </summary>
    /// <param name="builder"> The builder. </param>
    public static IServiceCollection AddOpenIdServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IAesGcmEncryption, AesGcmEncryption>();
        builder.AddSingleton<IAuthorizationRequestService, AuthorizationRequestService>();
        builder.AddSingleton<IAuthorizationResponseEncryptionService, AuthorizationResponseEncryptionService>();
        builder.AddSingleton<ICandidateQueryService, CandidateQueryService>();
        builder.AddSingleton<IClientAttestationService, ClientAttestationService>();
        builder.AddSingleton<ICoseSign1Signer, CoseSign1Signer>();
        builder.AddSingleton<ICredentialNonceService, CredentialNonceService>();
        builder.AddSingleton<ICredentialOfferService, CredentialOfferService>();
        builder.AddSingleton<ICredentialRequestService, CredentialRequestService>();
        builder.AddSingleton<ICredentialSetService, CredentialSetService>();
        builder.AddSingleton<IDPopHttpClient, DPopHttpClient>();
        builder.AddSingleton<IDcApiService, DcApiService>();
        builder.AddSingleton<IDcqlService, DcqlService>();
        builder.AddSingleton<IEventAggregator, EventAggregator>();
        builder.AddSingleton<IIssuerMetadataService, IssuerMetadataService>();
        builder.AddSingleton<IMdocAuthenticationService, MdocAuthenticationService>();
        builder.AddSingleton<IMdocCandidateService, MdocCandidateService>();
        builder.AddSingleton<IOid4VciClientService, Oid4VciClientService>();
        builder.AddSingleton<IOid4VpClientService, Oid4VpClientService>();
        builder.AddSingleton<IOid4VpHaipClient, Oid4VpHaipClient>();
        builder.AddSingleton<IPexService, PexService>();
        builder.AddSingleton<IPresentationService, PresentationService>();
        builder.AddSingleton<IRpAuthService, RpAuthService>();
        builder.AddSingleton<IRpRegistrarService, RpRegistrarService>();
        builder.AddSingleton<IStatusListService, StatusListService>();
        builder.AddSingleton<ITokenService, TokenService>();
        builder.AddSingleton<IVctMetadataService, VctMetadataService>();
        builder.AddSingleton<IVerifierKeyService, VerifierKeyService>();
        
        builder.AddScoped<IDomainRepository<MdocCredential, MdocCredentialRecord, CredentialId>, MdocCredentialRepository>();
        builder.AddScoped<IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId>, SdJwtCredentialRepository>();
        builder.AddScoped<IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>, CredentialDataSetRepository>();
        builder.AddScoped<IDomainRepository<CompletedPresentation, CompletedPresentationRecord, string>, CompletedPresentationRepository>();
        builder.AddScoped<IDomainRepository<AuthFlowSession, AuthFlowSessionRecord, WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models.AuthFlowSessionState>, AuthFlowSessionRepository>();

        builder.AddSdJwtVcServices();
        
        return builder;
    }
    
    /// <summary>
    /// Adds the extended OpenID4Vci Client service.
    /// </summary>
    /// <returns>The extended OpenID4Vci Client service.</returns>
    /// <param name="builder">Builder.</param>
    /// <typeparam name="TService">The 1st type parameter.</typeparam>
    /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
    public static IServiceCollection AddExtendedOid4VciClientService<TService, TImplementation>(this IServiceCollection builder)
        where TService : class, IOid4VciClientService
        where TImplementation : class, TService, IOid4VciClientService
    {
        builder.AddSingleton<TImplementation>();
        builder.AddSingleton<IOid4VciClientService>(x => x.GetService<TImplementation>()!);
        builder.AddSingleton<TService>(x => x.GetService<TImplementation>()!);
        return builder;
    }
    
    // /// <summary>
    // /// Adds the extended CredentialSet service.
    // /// </summary>
    // /// <returns>The extended CredentialSet credential service.</returns>
    // /// <param name="builder">Builder.</param>
    // /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
    public static IServiceCollection AddExtendedCredentialSetStorage<TImplementation>(this IServiceCollection builder)
        where TImplementation : class, IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>
    {
        builder.AddSingleton<TImplementation>();
        builder.AddSingleton<IDomainRepository<CredentialDataSet, CredentialDataSetRecord, CredentialSetId>>(
            x => x.GetService<TImplementation>()!);
        return builder;
    }

    /// <summary>
    /// Configures wallet storage for OpenID features. Registers the framework's record configurations and allows
    /// the consumer to add additional records.
    /// </summary>
    /// <param name="builder">Service collection.</param>
    /// <param name="connectionString">SQLite connection string.</param>
    /// <param name="configure">Optional callback to append additional record registrations.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureStorage(
        this IServiceCollection builder,
        string connectionString,
        Action<IRecordsBuilder>? configure = null)
    {
        Storage.Database.DependencyInjection.ServiceCollectionExtensions.ConfigureStorage(builder, connectionString, recordsBuilder =>
        {
            recordsBuilder.AddRecord<AuthFlowSessionRecord, AuthFlowSessionRecordConfiguration>();
            recordsBuilder.AddRecord<CredentialDataSetRecord, CredentialDataSetRecordConfiguration>();
            recordsBuilder.AddRecord<MdocCredentialRecord, MdocCredentialRecordConfiguration>();
            recordsBuilder.AddRecord<CompletedPresentationRecord, CompletedPresentationRecordConfiguration>();
            recordsBuilder.AddRecord<SdJwtCredentialRecord, SdJwtCredentialRecordConfiguration>();

            configure?.Invoke(recordsBuilder);
        });

        return builder;
    }
}
