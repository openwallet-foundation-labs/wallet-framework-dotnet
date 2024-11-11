using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.BatchCredentialIssuanceFun;
using static WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer.BatchSize;

namespace WalletFramework.Oid4Vc.Oid4Vci.Models.Metadata.Issuer;

/// <summary>
///     Represents the information about the Credential Issuer's supports for batch issuance of Credentials
/// </summary>
public record BatchCredentialIssuance
{
    /// <summary>
    ///     Specifies the maximum array size for the proofs parameter in a Credential Request
    /// </summary>
    public Option<BatchSize> BatchSize { get; }
    
    private BatchCredentialIssuance(
        Option<BatchSize> batchSize)
    {
        BatchSize = batchSize;
    }

    public static Option<BatchCredentialIssuance> OptionalBatchCredentialIssuance(JToken issuance) => issuance.ToJObject().ToOption().OnSome(jObject =>
    {
        var batchSize = jObject.GetByKey(BatchSizeJsonKey).ToOption().OnSome(OptionBatchSize);

        return batchSize.IsNone ? Option<BatchCredentialIssuance>.None : new BatchCredentialIssuance(batchSize);
    });
}

public static class BatchCredentialIssuanceFun
{
    public const string BatchSizeJsonKey = "batch_size";
    
    public static JObject EncodeToJson(this BatchCredentialIssuance issuance)
    {
        var json = new JObject();
        issuance.BatchSize.IfSome(name => json.Add(BatchSizeJsonKey, name.ToString()));
        return json;
    }
}
