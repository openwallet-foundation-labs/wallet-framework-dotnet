using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.Models;

namespace WalletFramework.Oid4Vc.Oid4Vp;

public record CompletedPresentation(
    string PresentationId,
    string ClientId,
    List<PresentedCredentialSet> PresentedCredentialSets,
    Option<ClientMetadata> ClientMetadata,
    Option<string> Name,
    DateTimeOffset LastTimeUsed);
