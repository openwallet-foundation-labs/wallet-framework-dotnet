using SD_JWT;
using SD_JWT.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.Services.Oid4VciClientService;
using WalletFramework.Oid4Vc.Oid4Vp.PresentationExchange.Services;
using WalletFramework.Oid4Vc.Oid4Vp.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenId4VcDefaultServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IHolder, Holder>();
        builder.AddSingleton<IPexService, PexService>();
        builder.AddSingleton<IOid4VciClientService, Oid4VciClientService>();
        builder.AddSingleton<IOid4VpClientService, Oid4VpClientService>();
        builder.AddSingleton<IOid4VpHaipClient, Oid4VpHaipClient>();
        builder.AddSingleton<IOid4VpRecordService, Oid4VpRecordService>();
            
        return builder;
    }
}
