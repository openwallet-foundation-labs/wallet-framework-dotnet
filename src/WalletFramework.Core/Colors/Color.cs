using System.Drawing;
using LanguageExt;
using Newtonsoft.Json;
using WalletFramework.Core.Json.Converters;

namespace WalletFramework.Core.Colors;

[JsonConverter(typeof(ValueTypeJsonConverter<Color>))]
public readonly struct Color
{
    private System.Drawing.Color Value { get; }

    private Color(System.Drawing.Color value)
    {
        Value = value;
    }

    public override string ToString() => Value.ToHex();

    public System.Drawing.Color ToSystemColor() => Value;

    public static implicit operator System.Drawing.Color(Color color) => color.Value;

    public static implicit operator Color(System.Drawing.Color systemColor) => new(systemColor);
    
    public static implicit operator string(Color color) => color.ToString();

    public static Option<Color> OptionColor(string hexStr)
    {
        try
        {
            var colorConverter = new ColorConverter();
            var systemColor = (System.Drawing.Color)colorConverter.ConvertFromString(hexStr);
            return systemColor.ToFrameworkColor();
        }
        catch (Exception)
        {
            return Option<Color>.None;
        }
    }
}    

public static class ColorFun
{
    public static string ToHex(this System.Drawing.Color systemColor) => 
        $"#{systemColor.R:X2}{systemColor.G:X2}{systemColor.B:X2}";

    public static Color ToFrameworkColor(this System.Drawing.Color systemColor) => systemColor;
}
