using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.MdocLib.Device.Request;

public readonly struct DataElementIdentifier
{
    private string Value { get; }

    private DataElementIdentifier(string value)
    {
        Value = value;
    }

    public override string ToString() => Value;

    public static implicit operator string(DataElementIdentifier dataElementIdentifier) => dataElementIdentifier.Value;

    public static Validation<DataElementIdentifier> ValidDataElementIdentifier(string dataElementIdentifier)
    {
        if (string.IsNullOrWhiteSpace(dataElementIdentifier))
        {
            return new StringIsNullOrWhitespaceError<DataElementIdentifier>();
        }
        else
        {
            return new DataElementIdentifier(dataElementIdentifier);
        }
    }
}    

