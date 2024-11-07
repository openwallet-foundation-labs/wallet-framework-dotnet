using LanguageExt;
using Newtonsoft.Json.Linq;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

public readonly struct BatchSize
{
    private int Value { get; }

    private BatchSize(int value) => Value = value;

    public override string ToString() => Value.ToString();
    
    public static implicit operator int(BatchSize batchSize) => batchSize.Value;
    
    public static Option<BatchSize> OptionBatchSize(JToken batchSize)
    {
        var str = batchSize.ToString();

        if (int.TryParse(str, out int intBatchSize))
        {
            return new BatchSize(intBatchSize);
        }
        
        return Option<BatchSize>.None;
    }
}
