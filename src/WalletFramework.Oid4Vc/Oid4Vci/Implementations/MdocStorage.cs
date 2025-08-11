// using Hyperledger.Aries.Agents;
// using Hyperledger.Aries.Storage;
// using LanguageExt;
// using WalletFramework.Core.Credentials;
// using WalletFramework.Core.Functional;
// using WalletFramework.MdocVc;
// using WalletFramework.Oid4Vc.Oid4Vci.Abstractions;
//
// namespace WalletFramework.Oid4Vc.Oid4Vci.Implementations;
//
// public class MdocStorage : IMdocStorage
// {
//     public MdocStorage(IAgentProvider agentProvider, IWalletRecordService recordService)
//     {
//         _agentProvider = agentProvider;
//         _recordService = recordService;
//     }
//
//     private readonly IAgentProvider _agentProvider;
//     private readonly IWalletRecordService _recordService;
//
//     public async Task<Unit> Add(MdocRecord record)
//     {
//         var context = await _agentProvider.GetContextAsync();
//         await _recordService.AddAsync(context.Wallet, record);
//         return Unit.Default;
//     }
//
//     public async Task<Option<MdocRecord>> Get(CredentialId id)
//     {
//         var context = await _agentProvider.GetContextAsync();
//         return await _recordService.GetAsync<MdocRecord>(context.Wallet, id);
//     }
//
//     public async Task<Option<IEnumerable<MdocRecord>>> List(
//         Option<ISearchQuery> query,
//         int count = 100,
//         int skip = 0)
//     {
//         var context = await _agentProvider.GetContextAsync();
//         var list = await _recordService.SearchAsync<MdocRecord>(
//             context.Wallet, 
//             query.ToNullable(),
//             null,
//             count, 
//             skip);
//
//         if (list.Count == 0)
//             return Option<IEnumerable<MdocRecord>>.None;
//
//         return list;
//     }
//
//     public async Task<Option<IEnumerable<MdocRecord>>> List(CredentialSetId id)
//     {
//         var query = SearchQuery.Equal(
//             "~" + nameof(MdocRecord.CredentialSetId),
//             id);
//
//         var some = Option<ISearchQuery>.Some(query);
//         return await List(some);
//     }
//
//     public async Task<Unit> Update(MdocRecord record)
//     {
//         var context = await _agentProvider.GetContextAsync();
//         await _recordService.Update(context.Wallet, record);
//         return Unit.Default;
//     }
//
//     public async Task<Unit> Delete(MdocRecord record)
//     {
//         var context = await _agentProvider.GetContextAsync();
//         await _recordService.Delete(context.Wallet, record);
//         return Unit.Default;
//     }
// }
