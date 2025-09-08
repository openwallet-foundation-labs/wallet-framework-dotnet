using System;
using System.ComponentModel;
using System.Globalization;
using Newtonsoft.Json;

namespace WalletFramework.Core.Localization;

public sealed class LocalJsonConverter : JsonConverter<Locale>
{
    public override void WriteJson(JsonWriter writer, Locale value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }

    public override Locale ReadJson(JsonReader reader, Type objectType, Locale existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            throw new JsonSerializationException("Locale cannot be null");

        if (reader.TokenType != JsonToken.String)
            throw new JsonSerializationException($"Unexpected token {reader.TokenType} when parsing Locale");

        var s = (string)reader.Value!;
        return Locale.Create(s);
    }
}

public sealed class LocaleTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is string s)
        {
            return Locale.Create(s);
        }

        return base.ConvertFrom(context, culture, value);
    }

    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType == typeof(string) && value is Locale locale)
        {
            return locale.ToString();
        }

        return base.ConvertTo(context, culture, value, destinationType);
    }
}


