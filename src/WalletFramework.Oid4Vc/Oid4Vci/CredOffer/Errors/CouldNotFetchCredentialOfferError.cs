using System.Net;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record CouldNotFetchCredentialOfferError(HttpStatusCode Code) 
    : Error($"The credential offer could not be fetched with the status code: {Code}");
