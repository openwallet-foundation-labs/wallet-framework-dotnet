using WalletFramework.Core.Functional;

namespace WalletFramework.MdocLib.Security.Errors;

public record SessionDataError() : Error("Could not process SessionData");
