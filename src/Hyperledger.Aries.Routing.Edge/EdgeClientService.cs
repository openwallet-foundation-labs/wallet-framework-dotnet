using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Configuration;
using Hyperledger.Aries.Extensions;
using Hyperledger.Aries.Features.Handshakes.Common;
using Hyperledger.Aries.Features.Handshakes.Connection;
using Hyperledger.Aries.Features.Handshakes.Connection.Models;
using Hyperledger.Aries.Storage;
using Hyperledger.Indy.WalletApi;
using Microsoft.Extensions.Options;

namespace Hyperledger.Aries.Routing.Edge
{
    public partial class EdgeClientService : IEdgeClientService
    {
        private const string MediatorInboxIdTagName = "MediatorInboxId";
        private const string MediatorInboxKeyTagName = "MediatorInboxKey";
        private const string MediatorConnectionIdTagName = "MediatorConnectionId";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IProvisioningService _provisioningService;
        private readonly IWalletRecordService _recordService;
        private readonly IWalletService _walletService;
        private readonly IAgentProvider _agentProvider;
        private readonly IConnectionService _connectionService;
        private readonly IMessageService _messageService;

        private readonly AgentOptions _agentOptions;

        public EdgeClientService(
            IHttpClientFactory httpClientFactory,
            IProvisioningService provisioningService,
            IWalletRecordService recordService,
            IMessageService messageService,
            IWalletService walletService,
            IAgentProvider agentProvider,
            IConnectionService connectionService,
            IOptions<AgentOptions> agentOptions)
        {
            _httpClientFactory = httpClientFactory;
            _provisioningService = provisioningService;
            _recordService = recordService;
            _walletService = walletService;
            _agentProvider = agentProvider;
            _connectionService = connectionService;
            _messageService = messageService;
            _agentOptions = agentOptions.Value;
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


        public virtual async Task AddRouteAsync(IAgentContext agentContext, string routeDestination)
        {
            var connection = await GetMediatorConnectionAsync(agentContext);
            if (connection != null)
            {
                var createInboxMessage = new AddRouteMessage { RouteDestination = routeDestination };
                await _messageService.SendAsync(agentContext, createInboxMessage, connection);
            }
        }

        public virtual async Task CreateInboxAsync(IAgentContext agentContext, Dictionary<string, string> metadata = null)
        {
            var provisioning = await _provisioningService.GetProvisioningAsync(agentContext.Wallet);
            if (provisioning.GetTag(MediatorInboxIdTagName) != null)
                return;
            
            var connection = await GetMediatorConnectionAsync(agentContext);

            var createInboxMessage = new CreateInboxMessage { Metadata = metadata };
            var response = await _messageService.SendReceiveAsync<CreateInboxResponseMessage>(agentContext, createInboxMessage, connection);

            provisioning.SetTag(MediatorInboxIdTagName, response.InboxId);
            provisioning.SetTag(MediatorInboxKeyTagName, response.InboxKey);
            await _recordService.UpdateAsync(agentContext.Wallet, provisioning);
        }

        public async Task<ConnectionRecord> GetMediatorConnectionAsync(IAgentContext agentContext)
        {
            var provisioning = await _provisioningService.GetProvisioningAsync(agentContext.Wallet);
            if (provisioning.GetTag(MediatorConnectionIdTagName) == null)
                return null;
            
            var connection = await _recordService.GetAsync<ConnectionRecord>(agentContext.Wallet, provisioning.GetTag(MediatorConnectionIdTagName));
            if (connection == null) throw new AriesFrameworkException(ErrorCode.RecordNotFound, "Couldn't locate a connection to mediator agent");
            if (connection.State != ConnectionState.Connected) throw new AriesFrameworkException(ErrorCode.RecordInInvalidState, $"You must be connected to the mediator agent. Current state is {connection.State}");

            return connection;
        }
        
        public async Task CreateMediatorConnectionAndInboxAsync(AgentOptions agentOptions)
        {
            var discovery = await DiscoverConfigurationAsync(agentOptions.EndpointUri);

            var agentContext = await _agentProvider.GetContextAsync();
            
            await _provisioningService.UpdateEndpointAsync(agentContext.Wallet, new AgentEndpoint
            {
                Uri = discovery.ServiceEndpoint, 
                Verkey = new[] { discovery.RoutingKey}, 
                Did = agentOptions.AgentDid
            });

            await CreateMediatorConnection(agentContext, discovery);
            
            await CreateInboxAsync(agentContext, agentOptions.MetaData);
        }
        
        private async Task CreateMediatorConnection(IAgentContext agentContext, AgentPublicConfiguration discovery)
        {
            if (await GetMediatorConnectionAsync(agentContext) != null)
                return;
            
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

        public virtual async Task<AgentPublicConfiguration> DiscoverConfigurationAsync(string agentEndpoint)
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"{agentEndpoint}/.well-known/agent-configuration").ConfigureAwait(false);
            var responseJson = await response.Content.ReadAsStringAsync();

            return responseJson.ToObject<AgentPublicConfiguration>();
        }

        public virtual async Task<(int, IEnumerable<InboxItemMessage>)> FetchInboxAsync(IAgentContext agentContext)
        {
            var connection = await GetMediatorConnectionAsync(agentContext);
            if (connection == null)
            {
                throw new InvalidOperationException("This agent is not configured with a mediator");
            }

            var createInboxMessage = new GetInboxItemsMessage();
            var response = await _messageService.SendReceiveAsync<GetInboxItemsResponseMessage>(agentContext, createInboxMessage, connection);

            var processedItems = new List<string>();
            var unprocessedItem = new List<InboxItemMessage>();
            foreach (var item in response.Items)
            {
                try
                {
                    await agentContext.Agent.ProcessAsync(agentContext, new PackedMessageContext(item.Data));
                    processedItems.Add(item.Id);
                }
                catch (AriesFrameworkException e) when (e.ErrorCode == ErrorCode.InvalidMessage) 
                {
                    processedItems.Add(item.Id);
                }
                catch (Exception)
                {
                    unprocessedItem.Add(item);
                }
            }

            if (processedItems.Any())
            {
                await _messageService.SendAsync(agentContext, new DeleteInboxItemsMessage { InboxItemIds = processedItems }, connection);
            }

            return (processedItems.Count, unprocessedItem);
        }

        public virtual Task AddDeviceAsync(IAgentContext agentContext, AddDeviceInfoMessage message)
        {
            return SendAgentMessageAsync(agentContext, message);
        }

        public virtual Task UpsertDeviceAsync(IAgentContext agentContext, UpsertDeviceInfoMessage message)
        {
            return SendAgentMessageAsync(agentContext, message);
        }

        private async Task SendAgentMessageAsync(IAgentContext agentContext, AgentMessage message)
        {
            var connection = await GetMediatorConnectionAsync(agentContext);
            if (connection != null)
            {
                await _messageService.SendAsync(agentContext, message, connection);
            }
        }
    }
}
