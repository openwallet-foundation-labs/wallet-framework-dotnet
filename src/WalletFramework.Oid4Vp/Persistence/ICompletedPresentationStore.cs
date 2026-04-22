using LanguageExt;

namespace WalletFramework.Oid4Vp.Persistence;

public interface ICompletedPresentationStore
{
    Task<Unit> Add(CompletedPresentation presentation);

    Task<Option<CompletedPresentation>> Get(string presentationId);

    Task<IReadOnlyList<CompletedPresentation>> List();

    Task<IReadOnlyList<CompletedPresentation>> ListByClientId(string clientId);

    Task<Unit> Save(CompletedPresentation presentation);

    Task<Unit> Delete(string presentationId);
}
