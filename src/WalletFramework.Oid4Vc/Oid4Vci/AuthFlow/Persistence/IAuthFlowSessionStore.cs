using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Models;

namespace WalletFramework.Oid4Vc.Oid4Vci.AuthFlow.Persistence;

public interface IAuthFlowSessionStore
{
    Task<Unit> Save(AuthFlowSession session);

    Task<Option<AuthFlowSession>> Get(AuthFlowSessionState state);

    Task<IReadOnlyList<AuthFlowSession>> List();

    Task<Unit> Delete(AuthFlowSessionState state);
}
