using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Json.Errors;

public record JsonIsNotAMapError(JObject JObject, Exception E)
    : Error($"The jObject `{JObject}` could not be transformed into a dictionary", E);
