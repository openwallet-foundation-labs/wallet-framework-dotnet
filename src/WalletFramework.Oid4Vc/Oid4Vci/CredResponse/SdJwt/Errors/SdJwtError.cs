using System.Globalization;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredResponse.SdJwt.Errors;

public record SdJwtParsingError(JValue Value, Exception E) 
    : Error($"The encoded SD-JWT could not be parsed. Value is: {Value.ToString(CultureInfo.InvariantCulture)}", E);
