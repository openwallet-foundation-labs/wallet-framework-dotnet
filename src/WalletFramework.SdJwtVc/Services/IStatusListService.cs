using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.SdJwtVc.Models.StatusList;

namespace WalletFramework.SdJwtVc.Services;

public interface IStatusListService
{
    Task<Option<CredentialState>> GetState(Status status);
}
