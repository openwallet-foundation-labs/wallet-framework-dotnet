using LanguageExt;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Payment;

namespace WalletFramework.Oid4Vc.Oid4Vp.Models;

/// <summary>
///     Represents a credential that the Holder chose to present to the Verifier.
/// </summary>
public record SelectedCredential(
    string InputDescriptorId,
    ICredential Credential,
    Option<List<PaymentTransactionData>> TransactionData);
