using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Security.Errors;

public record SessionEstablishmentError() : Error("Could not process SessionEstablishment");
