using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc.Common;
using WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Errors;
using static WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc.PolicyFun;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredConfiguration.Models.Mdoc;

public record Policy
{
    public bool OneTimeUse { get; }
    
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
            .GetByKey(OneTimeUseJsonKey)
            .OnSuccess(token =>
            {
                var str = token.ToString();
                if (string.IsNullOrWhiteSpace(str))
                {
                    return new FieldValueIsNullOrEmptyError(OneTimeUseJsonKey).ToInvalid<bool>();
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
            .GetByKey(BatchSizeJsonKey)
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

public static class PolicyFun
{
    public const string OneTimeUseJsonKey = "one_time_use";
    public const string BatchSizeJsonKey = "batch_size";

    public static JObject EncodeToJson(this Policy policy)
    {
        var policyJson = new JObject
        {
            { OneTimeUseJsonKey, policy.OneTimeUse }
        };

        policy.BatchSize.IfSome(batchSize =>
            policyJson.Add(BatchSizeJsonKey, batchSize)
        );

        return policyJson;
    }
}
