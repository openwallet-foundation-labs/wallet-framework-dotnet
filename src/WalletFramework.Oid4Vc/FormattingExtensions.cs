using Newtonsoft.Json;

namespace WalletFramework.Oid4Vc;

/// <summary>
/// Formatting extensions
/// </summary>
public static class FormattingExtensions
{
    /// <summary>
    /// Converts an <see cref="object"/> to json string using default converter.
    /// </summary>
    /// <param name="obj">The object.</param>
    /// <returns></returns>
    public static string ToJson(this object obj) =>
        JsonConvert.SerializeObject(obj, Formatting.None);

    /// <summary>
    /// Converts an object to json string using the provided <see cref="JsonSerializerSettings"/>
    /// </summary>
    /// <returns>The json.</returns>
    /// <param name="obj">Object.</param>
    /// <param name="settings">SerializerSettings.</param>
    public static string ToJson(this object obj, JsonSerializerSettings settings) =>
        JsonConvert.SerializeObject(obj, settings);
}