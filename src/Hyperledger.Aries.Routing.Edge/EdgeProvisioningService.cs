using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Features.Handshakes.Connection;
using Hyperledger.Aries.Features.Handshakes.Connection.Models;
using Hyperledger.Aries.Routing;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy.WalletApi;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("Hyperledger.Aries.Tests")]

namespace Hyperledger.Aries.Agents.Edge
{
    internal class EdgeProvisioningService : IHostedService, IEdgeProvisioningService
    {
        internal const string MediatorConnectionIdTagName = "MediatorConnectionId";
        private const string MediatorInboxIdTagName = "MediatorInboxId";

        private readonly IProvisioningService provisioningService;
        private readonly IConnectionService connectionService;
        private readonly IMessageService messageService;
        private readonly IEdgeClientService edgeClientService;
        private readonly IWalletRecordService recordService;
        private readonly IAgentProvider agentProvider;
        private readonly AgentOptions agentOptions;

        public EdgeProvisioningService(
            IProvisioningService provisioningService,
            IConnectionService connectionService,
            IMessageService messageService,
            IEdgeClientService edgeClientService,
            IWalletRecordService recordService,
            IAgentProvider agentProvider,
            IOptions<AgentOptions> agentOptions)
        {
            this.provisioningService = provisioningService;
            this.connectionService = connectionService;
            this.messageService = messageService;
            this.edgeClientService = edgeClientService;
            this.recordService = recordService;
            this.agentProvider = agentProvider;
            this.agentOptions = agentOptions.Value;
        }
        
        public async Task ProvisionAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default)
        {
            var discovery = await edgeClientService.DiscoverConfigurationAsync(agentOptions.EndpointUri);

            try
            {
                agentOptions.AgentKey = discovery.RoutingKey;
                agentOptions.EndpointUri = discovery.ServiceEndpoint;

                await provisioningService.ProvisionAgentAsync(agentOptions);
            }
            catch(WalletStorageException)
            {
                // OK
            }
            catch (WalletExistsException)
            {
                // OK
            }
            
            var agentContext = await agentProvider.GetContextAsync();

            await CreateMediatorConnection(agentContext, discovery);
            
            await edgeClientService.CreateInboxAsync(agentContext, agentOptions.MetaData);
        }

        public async Task CreateMediatorConnectionAndInboxAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default)
        {
            var discovery = await edgeClientService.DiscoverConfigurationAsync(agentOptions.EndpointUri);

            var agentContext = await agentProvider.GetContextAsync();
            await provisioningService.UpdateEndpointAsync(agentContext.Wallet, new AgentEndpoint
            {
                Uri = discovery.ServiceEndpoint, 
                Verkey = new[] { discovery.RoutingKey}, 
                Did = agentOptions.AgentDid
            });

            await CreateMediatorConnection(agentContext, discovery);
            
            await edgeClientService.CreateInboxAsync(agentContext, agentOptions.MetaData);
        }

        private async Task CreateMediatorConnection(IAgentContext agentContext, AgentPublicConfiguration discovery)
        {
            var provisioning = await provisioningService.GetProvisioningAsync(agentContext.Wallet);
            
            // Check if connection has been established with mediator agent
            if (provisioning.GetTag(MediatorConnectionIdTagName) != null)
                return;
            
            var (request, record) = await connectionService.CreateRequestAsync(agentContext, discovery.Invitation);
            var response = await messageService.SendReceiveAsync<ConnectionResponseMessage>(agentContext, request, record);
        
            await connectionService.ProcessResponseAsync(agentContext, response, record);
        
            // Remove the routing key explicitly as it won't ever be needed.
            // Messages will always be sent directly with return routing enabled
            record = await connectionService.GetAsync(agentContext, record.Id);
            record.Endpoint = new AgentEndpoint(record.Endpoint.Uri, null, null);
            await recordService.UpdateAsync(agentContext.Wallet, record);
            
            provisioning.SetTag(MediatorConnectionIdTagName, record.Id);
            await recordService.UpdateAsync(agentContext.Wallet, provisioning);
        }

        public Task ProvisionAsync(CancellationToken cancellationToken = default) => ProvisionAsync(agentOptions, cancellationToken);
        
        public Task CreateMediatorConnectionAndInboxAsync(CancellationToken cancellationToken = default) => CreateMediatorConnectionAndInboxAsync(agentOptions, cancellationToken);

        public Task StartAsync(CancellationToken cancellationToken) => ProvisionAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
