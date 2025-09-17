using WalletFramework.Core.Uri;

namespace WalletFramework.MdocVc;

public readonly struct MdocLogo
{
    public MdocLogo(Uri value)
    {
        Value = value;
    }

    private Uri Value { get; }

    public override string ToString() => Value.ToStringWithoutTrail();

    public static implicit operator string(MdocLogo logo) => logo.ToString();
}
