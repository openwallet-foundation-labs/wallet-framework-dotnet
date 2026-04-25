using WalletFramework.Credentials;

namespace WalletFramework.Oid4Vp.Models;

public record PresentationMap(string Identifier, string Presentation, CredentialFormat Format);
