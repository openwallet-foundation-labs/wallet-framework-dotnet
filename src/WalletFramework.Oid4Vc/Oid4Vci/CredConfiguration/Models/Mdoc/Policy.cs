using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Converters;
using WalletFramework.Core.Json.Errors;
using WalletFramework.MdocLib.Common;
using WalletFramework.MdocVc.Common;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record Policy
{
    [JsonProperty("one_time_use")]
    public bool OneTimeUse { get; }
    
    [JsonProperty("batch_size")]
    [JsonConverter(typeof(OptionJsonConverter<uint>))]
    public Option<uint> BatchSize { get; }
    
    private Policy(bool oneTimeUse, Option<uint> batchSize)
    {
        OneTimeUse = oneTimeUse;
        BatchSize = batchSize;
    }
    
    private static Policy Create(bool oneTimeUse, Option<uint> batchSize) => new(oneTimeUse, batchSize);

    public static Validation<Policy> ValidPolicy(JToken policy)
    {
        JObject jObject;
        try
        {
            jObject = policy.ToObject<JObject>()!;
        }
        catch (Exception e)
        {
            return new JTokenIsNotAnJObjectError("policy", e);
        }

        var oneTimeUse = jObject
            .GetByKey("one_time_use")
            .OnSuccess(token =>
            {
                var str = token.ToString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new FieldValueIsNullOrEmptyError("one_time_use").ToInvalid<bool>();
                }
                else
                {
                    try
                    {
                        return bool.Parse(str);
                    }
                    catch (Exception e)
                    {
                        return new OneTimeUseIsNotABooleanValueError(str, e);
                    }
                }
            });
        
        var batchSize = jObject
            .GetByKey("batch_size")
            .OnSuccess(token =>
            {
                try
                {
                    var str = token.ToString();
                    return uint.Parse(str);
                }
                catch (Exception)
                {
                    return new BatchSizeIsNotAPositiveNumberError().ToInvalid<uint>();
                }
            })
            .ToOption();

        return ValidationFun.Valid(Create)
            .Apply(oneTimeUse)
            .Apply(batchSize);
    }
}
