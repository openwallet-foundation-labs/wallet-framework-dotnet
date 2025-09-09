using Newtonsoft.Json.Linq;

namespace WalletFramework.SdJwtLib.Roles.Issuer;

public class SdArray : JArray
{
}

public class SdProperty : JProperty
{
    public SdProperty(JProperty other) : base(other)
    {
    }

    public SdProperty(string name, params object[] content) : base(name, content)
    {
    }

    public SdProperty(string name, object? content) : base(name, content)
    {
    }
}
