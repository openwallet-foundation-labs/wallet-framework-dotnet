using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Configuration;

namespace Hyperledger.Aries.Agents.Edge
{
    public interface IEdgeProvisioningService
    {
        Task ProvisionAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default);
        Task ProvisionAsync(CancellationToken cancellationToken = default);
        Task CreateMediatorConnectionAndInboxAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default);
        Task CreateMediatorConnectionAndInboxAsync(CancellationToken cancellationToken = default);
    }
}
