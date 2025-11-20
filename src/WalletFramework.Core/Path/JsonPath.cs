using WalletFramework.Core.Functional;

namespace WalletFramework.Core.Path;

public readonly record struct JsonPath
{
    private JsonPath(string path) => Value = path;
    
    public string Value { get; }

    public override string ToString() => Value;
    
    public static implicit operator string(JsonPath jsonPath) => jsonPath.Value;

    public static Validation<JsonPath> ValidJsonPath(string path)
    {
        return new JsonPath(path);
    }
}
