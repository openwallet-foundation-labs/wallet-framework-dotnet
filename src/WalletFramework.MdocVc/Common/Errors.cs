using WalletFramework.Core.Functional;

namespace WalletFramework.MdocVc.Common;

public record OneTimeUseIsNotABooleanValueError(string Actual, Exception E)
    : Error($"The field 'one_time_use' is not a boolean value. Actual value is: `{Actual}`", E);
