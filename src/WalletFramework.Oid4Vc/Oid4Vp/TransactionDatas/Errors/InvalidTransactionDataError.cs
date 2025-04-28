using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;

public record InvalidTransactionDataError : VpError
{
    private const string Code = "invalid_transaction_data";

    public Option<PresentationRequest> PresentationRequest { get; }

    public InvalidTransactionDataError(string message) : base(Code, message)
    {
    }

    public InvalidTransactionDataError(
        string Message,
        Option<Exception> Exception) : base(Code, Message, Exception)
    {
    }

    public InvalidTransactionDataError(string message, PresentationRequest request) : base(Code, message) =>
        PresentationRequest = request;

    public InvalidTransactionDataError(
        string Message,
        PresentationRequest request,
        Option<Exception> Exception) : base(Code, Message, Exception) =>
        PresentationRequest = request;
}
