using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Oid4Vc.Oid4Vci.Services.Oid4VciClientService;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;

namespace WalletFramework.Oid4Vc;

public static class SeviceCollectionExtensions
{
    /// <summary>
    /// Adds the default OpenID services.
    /// </summary>
    /// <param name="builder"> The builder. </param>
    public static IServiceCollection AddOpenIdDefaultServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IPexService, PexService>();
        builder.AddSingleton<IOid4VciClientService, Oid4VciClientService>();
        builder.AddSingleton<IOid4VpClientService, Oid4VpClientService>();
        builder.AddSingleton<IOid4VpHaipClient, Oid4VpHaipClient>();
        builder.AddSingleton<IOid4VpRecordService, Oid4VpRecordService>();
            
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
}
