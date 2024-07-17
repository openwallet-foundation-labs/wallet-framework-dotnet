using Hyperledger.Aries.Agents;
using Hyperledger.Aries.Storage;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Records;
using WalletFramework.Oid4Vc.Oid4Vp.Services;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Implementations;

/// <inheritdoc />
public class AuthFlowSessionStorage : IAuthFlowSessionStorage
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Oid4VpRecordService" /> class.
    /// </summary>
    /// <param name="recordService">The service responsible for wallet record operations.</param>
    public AuthFlowSessionStorage(IWalletRecordService recordService)
    {
        _recordService = recordService;
    }
    
    private readonly IWalletRecordService _recordService;
        
    /// <inheritdoc />
    public async Task<string> StoreAsync(
        IAgentContext agentContext,
        AuthorizationData authorizationData,
        AuthorizationCodeParameters authorizationCodeParameters,
        VciSessionId sessionId)
    {
        var record = new AuthFlowSessionRecord(
            authorizationData,
            authorizationCodeParameters,
            sessionId);

        await _recordService.AddAsync(agentContext.Wallet, record, AuthFlowSessionRecordFun.EncodeToJson);
            
        return record.Id;
    }
        
    /// <inheritdoc />
    public async Task<AuthFlowSessionRecord> GetAsync(IAgentContext context, VciSessionId sessionId)
    {
        var record = await _recordService.GetAsync(context.Wallet, sessionId, AuthFlowSessionRecordFun.DecodeFromJson);
        return record!;
    }
        
    /// <inheritdoc />
    public async Task<bool> DeleteAsync(IAgentContext context, VciSessionId sessionId) => 
        await _recordService.DeleteAsync<AuthFlowSessionRecord>(context.Wallet, sessionId);
}
