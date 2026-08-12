using System.Globalization;
using LanguageExt;
using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json;
using WalletFramework.Core.Json.Errors;

namespace WalletFramework.Oid4Vp.TS12SCA.Internal;

internal static class Ts12JsonFun
{
    public static Validation<Option<T>> GetOptional<T>(
        this JObject jObject,
        string key,
        Func<JToken, Validation<T>> parse) =>
        jObject.TryGetValue(key, out var token)
            ? parse(token!).Select(Option<T>.Some)
            : Option<T>.None;

    public static Validation<Option<T>> GetOptionalObject<T>(
        this JObject jObject,
        string key,
        Func<JObject, Validation<T>> parse) =>
        jObject.GetOptional(
            key,
            token =>
                from obj in token.ToJObject()
                from parsed in parse(obj)
                select parsed);

    public static Validation<Option<string>> GetOptionalString(this JObject jObject, string key) =>
        jObject.GetOptional<string>(
            key,
            token =>
            {
                var value = token.ToString();

                if (string.IsNullOrWhiteSpace(value))
                {
                    return new JsonFieldValueIsNullOrWhitespaceError(key);
                }

                return value;
            });

    public static Validation<Option<bool>> GetOptionalBoolean(this JObject jObject, string key) =>
        jObject.GetOptional<bool>(
            key,
            token =>
            {
                try
                {
                    return token.ToObject<bool>();
                }
                catch (Exception e)
                {
                    return new InvalidJsonError($"The value for {key} is not a boolean value", e);
                }
            });

    public static Validation<Option<decimal>> GetOptionalDecimal(this JObject jObject, string key) =>
        jObject.GetOptional(key, token => ToDecimal(token, key));

    public static Validation<Option<int>> GetOptionalInt(this JObject jObject, string key) =>
        jObject.GetOptional<int>(
            key,
            token =>
            {
                try
                {
                    return token.ToObject<int>();
                }
                catch (Exception e)
                {
                    return new InvalidJsonError($"The value for {key} is not an integer value", e);
                }
            });

    public static Validation<decimal> ToDecimal(JToken token, string key)
    {
        try
        {
            var value = token is JValue jValue
                ? Convert.ToString(jValue.Value, CultureInfo.InvariantCulture)
                : token.ToString();

            return decimal.Parse(value!, CultureInfo.InvariantCulture);
        }
        catch (Exception e)
        {
            return new InvalidJsonError($"The value for {key} is not a decimal value", e);
        }
    }
}
