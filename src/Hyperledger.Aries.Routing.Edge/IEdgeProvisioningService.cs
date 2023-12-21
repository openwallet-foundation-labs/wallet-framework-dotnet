using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Configuration;

namespace Hyperledger.Aries.Routing.Edge
{
    public interface IEdgeProvisioningService
    {
        /// <summary>
        /// Creates an Edge Wallet based on the provided Agent Options.
        /// Afterwards the method <see cref="CreateMediatorConnectionAndInboxAsync"/> can be used to establish a mediator connection.
        /// </summary>
        /// <param name="agentOptions">The Agent Options.</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the process.</param>
        /// <returns></returns>
        Task ProvisionAsync(AgentOptions options, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates an Edge Wallet using the default Agent Options.
        /// Afterwards the method <see cref="CreateMediatorConnectionAndInboxAsync"/> can be used to establish a mediator connection.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token to cancel the process.</param>
        /// <returns></returns>
        Task ProvisionAsync(CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Creates a connection and inbox with a mediator associated with the given public configuration.
        /// </summary>
        /// <param name="agentOptions">The edge context.</param>
        /// <returns></returns>
        Task CreateMediatorConnectionAndInboxAsync(AgentOptions agentOptions);
    }
}
