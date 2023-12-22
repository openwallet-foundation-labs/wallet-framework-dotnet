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
            
        private readonly IProvisioningService _provisioningService;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;
        private readonly IEdgeClientService _edgeClientService;
        private readonly IWalletRecordService _recordService;
        private readonly IAgentProvider _agentProvider;
        private readonly AgentOptions _agentOptions;

        public EdgeProvisioningService(
            IProvisioningService provisioningService,
            IConnectionService connectionService,
            IMessageService messageService,
            IEdgeClientService edgeClientService,
            IWalletRecordService recordService,
            IAgentProvider agentProvider,
            IOptions<AgentOptions> agentOptions)
        {
            _provisioningService = provisioningService;
            _connectionService = connectionService;
            _messageService = messageService;
            _agentProvider = agentProvider;
            _agentOptions = agentOptions.Value;
            _edgeClientService = edgeClientService;
            _recordService = recordService;
        }

        public async Task EnsureMediatorConnectionAndInboxAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default)
        {
            var agentContext = await _agentProvider.GetContextAsync();
            var provisioning = await _provisioningService.GetProvisioningAsync(agentContext.Wallet);
            if (provisioning.GetTag(MediatorConnectionIdTagName) != null)
                return;
            
            await CreateMediatorConnection(agentContext, agentOptions);
            
            await _edgeClientService.CreateInboxAsync(agentContext, agentOptions.MetaData);
        }
        
        private async Task CreateMediatorConnection(IAgentContext agentContext, AgentOptions agentOptions)
        {
            var discovery = await _edgeClientService.DiscoverConfigurationAsync(agentOptions.EndpointUri);

            await _provisioningService.UpdateEndpointAsync(agentContext.Wallet, new AgentEndpoint
            {
                Uri = discovery.ServiceEndpoint, 
                Verkey = new[] { discovery.RoutingKey}, 
                Did = agentOptions.AgentDid
            });
            
            var (request, record) = await _connectionService.CreateRequestAsync(agentContext, discovery.Invitation);
            var response = await _messageService.SendReceiveAsync<ConnectionResponseMessage>(agentContext, request, record);
        
            await _connectionService.ProcessResponseAsync(agentContext, response, record);
        
            // Remove the routing key explicitly as it won't ever be needed.
            // Messages will always be sent directly with return routing enabled
            record = await _connectionService.GetAsync(agentContext, record.Id);
            record.Endpoint = new AgentEndpoint(record.Endpoint.Uri, null, null);
            await _recordService.UpdateAsync(agentContext.Wallet, record);
            
            var provisioning = await _provisioningService.GetProvisioningAsync(agentContext.Wallet);
            provisioning.SetTag(MediatorConnectionIdTagName, record.Id);
            await _recordService.UpdateAsync(agentContext.Wallet, provisioning);
        }
        
        public async Task ProvisionAsync(AgentOptions agentOptions, CancellationToken cancellationToken = default)
        {
            try
            {
                await _provisioningService.ProvisionAgentAsync(agentOptions);
            }
            catch (WalletExistsException)
            {
                // OK
            }
        }

        public Task ProvisionAsync(CancellationToken cancellationToken = default) => ProvisionAsync(_agentOptions, cancellationToken);
        
        public Task EnsureMediatorConnectionAndInboxAsync(CancellationToken cancellationToken = default) => ProvisionAsync(_agentOptions, cancellationToken);
        
        public Task StartAsync(CancellationToken cancellationToken) => ProvisionAsync(cancellationToken);

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
