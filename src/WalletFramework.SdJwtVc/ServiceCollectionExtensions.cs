using Microsoft.Extensions.DependencyInjection;
using SD_JWT.Roles;
using SD_JWT.Roles.Implementation;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.SdJwtVc;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSdJwtVcServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IHolder, Holder>();
        builder.AddSingleton<ISdJwtVcHolderService, SdJwtVcHolderService>();
        builder.AddSingleton<ISdJwtSigner, SdJwtSigner>();
        return builder;
    }
    
    /// <summary>
    /// Adds the extended Sd-JWT credential service.
    /// </summary>
    /// <returns>The extended SD-JWT credential service.</returns>
    /// <param name="builder">Builder.</param>
    /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
    public static IServiceCollection AddExtendedSdJwtHolderService<TImplementation>(this IServiceCollection builder)
        where TImplementation : class, ISdJwtVcHolderService
    {
        builder.AddSingleton<TImplementation>();
        builder.AddSingleton<ISdJwtVcHolderService>(x => x.GetService<TImplementation>());
        return builder;
    }
}
