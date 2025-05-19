using WalletFramework.Core.Functional;
using WalletFramework.Core.Functional.Errors;

namespace WalletFramework.Oid4Vc.Qes.Authorization;

public readonly record struct DocumentDigestLabel
{
    public string AsString => Value;

    private DocumentDigestLabel(string value) => Value = value;

    private string Value { get; }

    public static implicit operator string(DocumentDigestLabel label) => label.AsString;

    public static Validation<DocumentDigestLabel> FromString(string label)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            return new StringIsNullOrWhitespaceError<DocumentDigestLabel>();
        }

        return new DocumentDigestLabel(label);
    }
}
