using Microsoft.Extensions.DependencyInjection;
using WalletFramework.IsoProximity.CommunicationPhase.Abstractions;
using WalletFramework.IsoProximity.CommunicationPhase.Implementations;
using WalletFramework.IsoProximity.EngagementPhase.Abstractions;
using WalletFramework.IsoProximity.EngagementPhase.Implementations;

namespace WalletFramework.IsoProximity.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIsoProximityServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IEngagementService, EngagementService>();
        builder.AddSingleton<IProximityCommunicationService, ProximityCommunicationService>();

        return builder;
    }
}
