using LanguageExt;
using WalletFramework.Core.Credentials;

namespace WalletFramework.Core.StatusList;

public interface IStatusListService
{
    Task<Option<CredentialState>> GetState(StatusListEntry statusListEntry);
}
