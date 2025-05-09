using LanguageExt;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Qes.Authorization;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents a credential that the Holder chose to present to the Verifier.
/// </summary>
public record SelectedCredential(
    string Identifier,
    ICredential Credential,
    Option<List<TransactionData>> TransactionData,
    Option<List<Uc5QesTransactionData>> Uc5TransactionData);
