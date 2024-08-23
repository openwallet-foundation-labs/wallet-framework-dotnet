using Hyperledger.Aries.Agents;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Records;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Abstractions;

/// <summary>
///     Service for managing authorization records. They are used during the VCI Authorization Code Flow to hold session
///     relevant inforation.
/// </summary>
public interface IAuthFlowSessionStorage
{
    /// <summary>
    ///     Deletes the authorization session record by the session identifier.
    /// </summary>
    /// <param name="context">Agent Context</param>
    /// <param name="authFlowSessionState">Session State Identifier of a Authorization Code Flow session</param>
    /// <returns></returns>
    Task<bool> DeleteAsync(IAgentContext context, AuthFlowSessionState authFlowSessionState);

    /// <summary>
    ///     Retrieves the authorization session record by the session identifier.
    /// </summary>
    /// <param name="context">Agent Context</param>
    /// <param name="authFlowSessionState">Session State Identifier of a Authorization Code Flow session</param>
    /// <returns></returns>
    Task<AuthFlowSessionRecord> GetAsync(IAgentContext context, AuthFlowSessionState authFlowSessionState);
    
    /// <summary>
    ///     Updates the authorization session record by the session identifier.
    /// </summary>
    /// <param name="context">Agent Context</param>
    /// <param name="record">the updated Authorization Session Record</param>
    /// <returns></returns>
    Task UpdateAsync(IAgentContext context, AuthFlowSessionRecord record);

    /// <summary>
    ///     Stores the authorization session record.
    /// </summary>
    /// <param name="agentContext">The Agent Context</param>
    /// <param name="authorizationData">Options specified by the Client (Wallet)</param>
    /// <param name="authorizationCodeParameters">
    ///     Parameters required for the authorization during the VCI authorization code
    ///     flow.
    /// </param>
    /// <param name="authFlowSessionState">Session State Identifier of a Authorization Code Flow session</param>
    /// <returns></returns>
    Task<string> StoreAsync(
        IAgentContext agentContext,
        AuthorizationData authorizationData,
        AuthorizationCodeParameters authorizationCodeParameters,
        AuthFlowSessionState authFlowSessionState);
}
