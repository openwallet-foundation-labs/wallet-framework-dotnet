using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.Errors;

public record CredentialIssuerError(JValue Value, Exception E) 
    : Error($"The credential issuer field could not be processed. The value is: `{Value}`", E);
