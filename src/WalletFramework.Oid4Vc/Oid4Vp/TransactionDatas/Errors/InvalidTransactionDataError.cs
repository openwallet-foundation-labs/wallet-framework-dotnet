using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;

public record InvalidTransactionDataError : VpError
{
    private const string Code = "invalid_transaction_data";

    public Option<PresentationCandidates> PresentationCandidates { get; }

    public InvalidTransactionDataError(string message) : base(Code, message)
    {
    }

    public InvalidTransactionDataError(
        string Message,
        Option<Exception> Exception) : base(Code, Message, Exception)
    {
    }

    public InvalidTransactionDataError(string message, PresentationCandidates candidates) : base(Code, message) =>
        PresentationCandidates = candidates;

    public InvalidTransactionDataError(
        string Message,
        PresentationCandidates candidates,
        Option<Exception> Exception) : base(Code, Message, Exception) =>
        PresentationCandidates = candidates;
}
