using LanguageExt;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Oid4Vc.Oid4Vci.Authorization.Models;
using static WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes.TransactionCode;

namespace WalletFramework.Oid4Vc.Oid4Vci.CredOffer.GrantTypes;

/// <summary>
///     Represents the parameters for the 'pre-authorized_code' grant type.
/// </summary>
public record PreAuthorizedCode
{
    /// <summary>
    ///     Gets the pre-authorized code representing the Credential Issuer's authorization for the Wallet to obtain
    ///     Credentials of a certain type.
    /// </summary>
    [JsonProperty("pre-authorized_code")]
    public string Value { get; }
        
    /// <summary>
    ///     Specifying whether the user must send a Transaction Code along with the Token Request in a Pre-Authorized Code Flow.
    /// </summary>
    [JsonProperty("tx_code")]
    public Option<TransactionCode> TransactionCode { get; }
    
    /// <summary>
    ///     Specifying whether the user must send a Transaction Code along with the Token Request in a Pre-Authorized Code Flow.
    /// </summary>
    [JsonProperty("authorization_server")]
    public Option<AuthorizationServerId> AuthorizationServer { get; }

    private PreAuthorizedCode(string value, Option<TransactionCode> transactionCode, Option<AuthorizationServerId> authorizationServer)
    {
        Value = value;
        TransactionCode = transactionCode;
        AuthorizationServer = authorizationServer;
    }
    
    public static Option<PreAuthorizedCode> OptionalPreAuthorizedCode(JToken preAuthCode)
    {
        var transactionCode = preAuthCode
            .GetByKey("tx_code")
            .ToOption()
            .OnSome(OptionalTransactionCode);
        
        var authorizationServer = preAuthCode
            .GetByKey("authorization_server")
            .OnSuccess(AuthorizationServerId.ValidAuthorizationServerId)
            .ToOption();
        
        return preAuthCode
            .GetByKey("pre-authorized_code")
            .OnSuccess(token =>
            {
                var value = token.ToString();
                return new PreAuthorizedCode(value, transactionCode, authorizationServer);
            })
            .ToOption();
    }

    public override string ToString() => Value;
    
    public static implicit operator string(PreAuthorizedCode preAuthorizedCode) => preAuthorizedCode.Value;
}

/// <summary>
///    Represents the details of the expected Transaction Code.
/// </summary>
public record TransactionCode
{
    /// <summary>
    ///     Gets the length of the transaction code.
    /// </summary>
    [JsonProperty("length")]
    public Option<int> Length { get; }
        
    /// <summary>
    ///     Gets the description of the transaction code.
    /// </summary>
    [JsonProperty("description")]
    public Option<string> Description { get; }
        
    /// <summary>
    ///    Gets the input mode of the transaction code which specifies the valid character set. (Must be 'numeric' ot 'text')
    /// </summary>
    [JsonProperty("input_mode")]
    public Option<InputMode> InputMode { get; }

    private TransactionCode(
        Option<int> length,
        Option<string> description,
        Option<InputMode> inputMode)
    {
        Length = length;
        Description = description;
        InputMode = inputMode;
    }

    public static Option<TransactionCode> OptionalTransactionCode(JToken transactionCode)
    {
        var length = transactionCode
            .GetByKey("length")
            .OnSuccess(token => token.ToJValue())
            .OnSuccess(value => value.ToInt())
            .ToOption();
        
        var description = transactionCode
            .GetByKey("description")
            .OnSuccess(token => token.ToString())
            .ToOption();
        
        var inputMode = transactionCode
            .GetByKey("input_mode")
            .ToOption()
            .OnSome(GrantTypes.InputMode.OptionalInputMode);

        if (length.IsNone && description.IsNone && inputMode.IsNone)
            return Option<TransactionCode>.None;
        
        return new TransactionCode(length, description, inputMode);
    }
}

public record InputMode
{
    private InputModeValues Value { get; }

    private InputMode(InputModeValues value) => Value = value;

    public static Option<InputMode> OptionalInputMode(JToken inputMode)
    {
        try
        {
            var str = inputMode.ToString();
            return str switch
            {
                "numeric" => new InputMode(InputModeValues.Numeric),
                "text" => new InputMode(InputModeValues.Text),
                _ => Option<InputMode>.None
            };
        }
        catch (Exception)
        {
            return Option<InputMode>.None;
        }
    }
    
    public static implicit operator string(InputMode inputMode) => inputMode.ToString();

    public override string ToString() => ParseInputMode(Value);

    private enum InputModeValues
    {
        Numeric,
        Text
    }

    private static string ParseInputMode(InputModeValues values) =>
        values switch
        {
            InputModeValues.Numeric => "numeric",
            InputModeValues.Text => "text",
            _ => throw new ArgumentOutOfRangeException(nameof(values), values, "Invalid InputModeValues value")
        };
}
