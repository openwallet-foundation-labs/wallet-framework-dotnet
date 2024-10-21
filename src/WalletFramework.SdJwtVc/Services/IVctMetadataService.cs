using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Localization;
using WalletFramework.SdJwtVc.Models;
using WalletFramework.SdJwtVc.Models.VctMetadata;

namespace WalletFramework.SdJwtVc.Services;

public interface IVctMetadataService
{
    public Task<Option<VctMetadata>> ProcessMetadata(Vct vct);
}
