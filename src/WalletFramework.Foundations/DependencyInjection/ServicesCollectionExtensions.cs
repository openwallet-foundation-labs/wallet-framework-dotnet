using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Events;
using WalletFramework.Core.StatusList;
using WalletFramework.Credentials.CredentialSet;
using WalletFramework.Credentials.CredentialSet.Persistence;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Implementations;
using WalletFramework.MdocLib.Security.Cose.Abstractions;
using WalletFramework.MdocLib.Security.Cose.Implementations;
using WalletFramework.MdocVc.Persistence;
using WalletFramework.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vci.AuthFlow.Persistence;
using WalletFramework.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vci.Authorization.DPop.Implementations;
using WalletFramework.Oid4Vci.Authorization.Implementations;
using WalletFramework.Oid4Vci.CredentialNonce.Abstractions;
using WalletFramework.Oid4Vci.CredentialNonce.Implementations;
using WalletFramework.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vci.CredOffer.Implementations;
using WalletFramework.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vci.CredRequest.Implementations;
using WalletFramework.Oid4Vci.Implementations;
using WalletFramework.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vci.Issuer.Implementations;
using WalletFramework.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vp.AuthResponse.Encryption.Implementations;
using WalletFramework.Oid4Vp.Dcql.Services;
using WalletFramework.Oid4Vp.Persistence;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Abstractions;
using WalletFramework.Oid4Vp.RelyingPartyAuthentication.Implementations;
using WalletFramework.Oid4Vp.Services;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Persistence;
using WalletFramework.SdJwtVc.Services;
using WalletFramework.Storage;
using WalletFramework.Storage.Database;
using WalletFramework.Storage.Records;
using WalletFramework.WalletAttestations;
using WalletFramework.WalletAttestations.Abstractions;

namespace WalletFramework.DependencyInjection;

public static class ServiceCollectionExtensions
{
    private static IServiceCollection AddDefaultServices(IServiceCollection services)
    {
        services.AddScoped<IAuthFlowSessionStore, AuthFlowSessionRepository>();
        services.AddScoped<ICompletedPresentationStore, CompletedPresentationRepository>();
        services.AddScoped<ICredentialDataSetStore, CredentialDataSetRepository>();
        services.AddScoped<ICredentialSetService, CredentialSetService>();
        services.AddScoped<IDcApiService, DcApiService>();
        services.AddScoped<IDcqlService, DcqlService>();
        services.AddScoped<IMdocCandidateService, MdocCandidateService>();
        services.AddScoped<IMdocCredentialStore, MdocCredentialRepository>();
        services.AddScoped<IOid4VciClientService, Oid4VciClientService>();
        services.AddScoped<IOid4VpClientService, Oid4VpClientService>();
        services.AddScoped<IOid4VpHaipClient, Oid4VpHaipClient>();
        services.AddScoped<IPresentationService, PresentationService>();
        services.AddScoped<ISdJwtCredentialStore, SdJwtCredentialStore>();

        services.AddSingleton<IAesGcmEncryption, AesGcmEncryption>();
        services.AddSingleton<IAuthorizationRequestService, AuthorizationRequestService>();
        services.AddSingleton<IAuthorizationResponseEncryptionService, AuthorizationResponseEncryptionService>();
        services.AddSingleton<IClientAttestationService, ClientAttestationService>();
        services.AddSingleton<ICoseSign1Signer, CoseSign1Signer>();
        services.AddSingleton<ICredentialNonceService, CredentialNonceService>();
        services.AddSingleton<ICredentialOfferService, CredentialOfferService>();
        services.AddSingleton<ICredentialRequestService, CredentialRequestService>();
        services.AddSingleton<IDPopHttpClient, DPopHttpClient>();
        services.AddSingleton<IEventAggregator, EventAggregator>();
        services.AddSingleton<IIssuerMetadataService, IssuerMetadataService>();
        services.AddSingleton<IMdocAuthenticationService, MdocAuthenticationService>();
        services.AddSingleton<IRpAuthService, RpAuthService>();
        services.AddSingleton<IRpRegistrarService, RpRegistrarService>();
        services.AddSingleton<IStatusListService, StatusListService>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddSingleton<IVctMetadataService, VctMetadataService>();
        services.AddSingleton<IVerifierKeyService, VerifierKeyService>();

        services.AddSdJwtVcServices();

        return services;
    }

    private static IRecordsBuilder AddDefaultStorageRecords(IRecordsBuilder recordsBuilder)
    {
        recordsBuilder.AddRecord<AuthFlowSessionRecord, AuthFlowSessionRecordConfiguration>();
        recordsBuilder.AddRecord<CredentialDataSetRecord, CredentialDataSetRecordConfiguration>();
        recordsBuilder.AddRecord<MdocCredentialRecord, MdocCredentialRecordConfiguration>();
        recordsBuilder.AddRecord<CompletedPresentationRecord, CompletedPresentationRecordConfiguration>();
        recordsBuilder.AddRecord<SdJwtCredentialRecord, SdJwtCredentialRecordConfiguration>();

        return recordsBuilder;
    }

    private static IServiceCollection AddStorage(
        IServiceCollection services,
        WalletFrameworkStorageOptions storageOptions)
    {
        storageOptions.RegisterSqliteProvider(services);
        var connectionString = storageOptions.GetConnectionString();

        Storage.Database.DependencyInjection.ServiceCollectionExtensions.ConfigureStorage(
            services,
            connectionString,
            recordsBuilder =>
            {
                AddDefaultStorageRecords(recordsBuilder);
                storageOptions.ConfigureRecords(recordsBuilder);
            });

        if (storageOptions.AutoInitializeEnabled)
        {
            AutoInitializeStorage(services);
        }

        return services;
    }

    private static IServiceCollection AutoInitializeStorage(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var databaseCreator = serviceProvider.GetRequiredService<IDatabaseCreator>();

        databaseCreator.EnsureDatabaseCreated().GetAwaiter().GetResult();

        return services;
    }

    /// <summary>
    ///     Extensions.
    /// </summary>
    /// <param name="builder">Builder.</param>
    extension(IServiceCollection builder)
    {
        /// <summary>
        ///     Adds the extended CredentialSet service.
        /// </summary>
        /// <returns>The extended CredentialSet credential service.</returns>
        public IServiceCollection AddExtendedCredentialSetStorage<TImplementation>()
            where TImplementation : class, ICredentialDataSetStore
        {
            builder.AddScoped<TImplementation>();
            builder.AddScoped<ICredentialDataSetStore>(x => x.GetService<TImplementation>()!);
            return builder;
        }

        /// <summary>
        ///     Adds the extended OpenID4Vci Client service.
        /// </summary>
        /// <returns>The extended OpenID4Vci Client service.</returns>
        /// <typeparam name="TService">The 1st type parameter.</typeparam>
        /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
        public IServiceCollection AddExtendedOid4VciClientService<TService, TImplementation>()
            where TService : class, IOid4VciClientService
            where TImplementation : class, TService, IOid4VciClientService
        {
            builder.AddScoped<TImplementation>();
            builder.AddScoped<IOid4VciClientService>(x => x.GetService<TImplementation>()!);
            builder.AddScoped<TService>(x => x.GetService<TImplementation>()!);
            return builder;
        }

        /// <summary>
        ///     Adds the default wallet framework services.
        /// </summary>
        public IServiceCollection AddWalletFramework() => builder.AddWalletFramework(static _ => { });

        /// <summary>
        ///     Adds the default wallet framework services and allows nested storage configuration.
        /// </summary>
        public IServiceCollection AddWalletFramework(Action<IWalletFrameworkBuilder> configure)
        {
            AddDefaultServices(builder);

            var builder1 = new WalletFrameworkBuilder();
            configure(builder1);

            if (builder1.StorageOptions is { } storageOptions)
            {
                AddStorage(builder, storageOptions);
            }

            return builder;
        }
    }
}
