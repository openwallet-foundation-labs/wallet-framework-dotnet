using Microsoft.Extensions.DependencyInjection;
using WalletFramework.SdJwtVc.Services.SdJwtVcHolderService;

namespace WalletFramework.SdJwtVc;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the extended Sd-JWT credential service.
    /// </summary>
    /// <returns>The extended SD-JWT credential service.</returns>
    /// <param name="builder">Builder.</param>
    /// <typeparam name="TService">The 1st type parameter.</typeparam>
    /// <typeparam name="TImplementation">The 2nd type parameter.</typeparam>
    public static IServiceCollection AddExtendedSdJwtCredentialService<TService, TImplementation>(this IServiceCollection builder)
        where TService : class, ISdJwtVcHolderService
        where TImplementation : class, TService, ISdJwtVcHolderService
    {
        builder.AddSingleton<TImplementation>();
        builder.AddSingleton<ISdJwtVcHolderService>(x => x.GetService<TImplementation>());
        builder.AddSingleton<TService>(x => x.GetService<TImplementation>());
        return builder;
    }
}
