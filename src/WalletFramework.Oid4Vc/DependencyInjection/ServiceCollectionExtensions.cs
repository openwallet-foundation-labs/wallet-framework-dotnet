using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.StatusList;
using WalletFramework.MdocLib.Device.Abstractions;
using WalletFramework.MdocLib.Device.Implementations;
using WalletFramework.MdocLib.Security.Cose.Abstractions;
using WalletFramework.MdocLib.Security.Cose.Implementations;
using WalletFramework.Oid4Vc.CredentialSet;
using WalletFramework.Oid4Vc.Database.Migration.Abstraction;
using WalletFramework.Oid4Vc.Database.Migration.Implementations;
using WalletFramework.Oid4Vc.Dcql.Services;
using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.DPop.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.CredRequest.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Implementations;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Issuer.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption.Implementations;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Services;

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
        builder.AddSingleton<IAuthFlowSessionStorage, AuthFlowSessionStorage>();
        builder.AddSingleton<IAuthorizationResponseEncryptionService, AuthorizationResponseEncryptionService>();
        builder.AddSingleton<IAuthorizationRequestService, AuthorizationRequestService>();
        builder.AddSingleton<ICoseSign1Signer, CoseSign1Signer>();
        builder.AddSingleton<ICredentialOfferService, CredentialOfferService>();
        builder.AddSingleton<ICredentialRequestService, CredentialRequestService>();
        builder.AddSingleton<ICredentialSetService, CredentialSetService>();
        builder.AddSingleton<ICredentialSetStorage, CredentialSetStorage>();
        builder.AddSingleton<IDcqlService, DcqlService>();
        builder.AddSingleton<IDPopHttpClient, DPopHttpClient>();
        builder.AddSingleton<IIssuerMetadataService, IssuerMetadataService>();
        builder.AddSingleton<IMdocAuthenticationService, MdocAuthenticationService>();
        builder.AddSingleton<IMdocCandidateService, MdocCandidateService>();
        builder.AddSingleton<IMdocStorage, MdocStorage>();
        builder.AddSingleton<IMigrationStepsProvider, MigrationStepsProvider>();
        builder.AddSingleton<IOid4VciClientService, Oid4VciClientService>();
        builder.AddSingleton<IOid4VpClientService, Oid4VpClientService>();
        builder.AddSingleton<IOid4VpHaipClient, Oid4VpHaipClient>();
        builder.AddSingleton<IOid4VpRecordService, Oid4VpRecordService>();
        builder.AddSingleton<IPexService, PexService>();
        builder.AddSingleton<IPresentationCandidateService, PresentationCandidateService>();
        builder.AddSingleton<IRecordsMigrationService, RecordsMigrationService>();
        builder.AddSingleton<IStatusListService, StatusListService>();
        builder.AddSingleton<ITokenService, TokenService>();
        builder.AddSingleton<IVctMetadataService, VctMetadataService>();

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
        builder.AddSingleton<IOid4VciClientService>(x => x.GetService<TImplementation>());
        builder.AddSingleton<TService>(x => x.GetService<TImplementation>());
        return builder;
    }
    
    /// <summary>
    /// Adds the extended CredentialSet service.
    /// </summary>
    /// <returns>The extended CredentialSet credential service.</returns>
    /// <param name="builder">Builder.</param>
    /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
    public static IServiceCollection AddExtendedCredentialSetStorage<TImplementation>(this IServiceCollection builder)
        where TImplementation : class, ICredentialSetStorage
    {
        builder.AddSingleton<TImplementation>();
        builder.AddSingleton<ICredentialSetStorage>(x => x.GetService<TImplementation>());
        return builder;
    }
}
