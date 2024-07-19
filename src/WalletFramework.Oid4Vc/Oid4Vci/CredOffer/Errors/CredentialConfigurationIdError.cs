using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record CredentialConfigurationIdError(JValue Value, Exception E) 
    : Error($"The CredentialConfigurationId could not be processed. The value is `{Value}`", E);
