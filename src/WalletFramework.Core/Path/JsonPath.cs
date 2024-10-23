using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Path;

public readonly struct JsonPath
{
    private string Value { get; }

    private JsonPath(string path) => Value = path;

    public override string ToString() => Value;
    
    public static implicit operator string(JsonPath jsonPath) => jsonPath.Value;

    public static Validation<JsonPath> ValidJsonPath(string path)
    {
        return new JsonPath(path);
    }
}
