using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Common;
using static WalletFramework.MdocLib.Common.Constants;

namespace WalletFramework.MdocLib;

public readonly struct DigestAlgorithm
{
    public DigestAlgorithmValue Value { get; }

    private DigestAlgorithm(DigestAlgorithmValue value) => Value = value;
    
    public static implicit operator DigestAlgorithmValue(DigestAlgorithm algorithm) => algorithm.Value;

    public static Validation<DigestAlgorithm> ValidDigestAlgorithm(CBORObject input) =>
        input.GetByLabel(DigestAlgorithmLabel).OnSuccess(digestAlgorithm =>
        {
            try
            {
                var str = digestAlgorithm.AsString();
                return str switch
                {
                    "SHA-256" => new DigestAlgorithm(DigestAlgorithmValue.Sha256),
                    "SHA-384" => new DigestAlgorithm(DigestAlgorithmValue.Sha384),
                    "SHA-512" => new DigestAlgorithm(DigestAlgorithmValue.Sha512),
                    _ => new InvalidDigestAlgorithmError(str).ToInvalid<DigestAlgorithm>()
                };
            }
            catch (Exception e)
            {
                return new CborIsNotATextStringError(DigestAlgorithmLabel, e);
            }
        });


    public static implicit operator string(DigestAlgorithm digestAlgorithm) => digestAlgorithm.ToString();

    public override string ToString() => Value switch
    {
        DigestAlgorithmValue.Sha256 => "SHA-256",
        DigestAlgorithmValue.Sha384 => "SHA-384",
        DigestAlgorithmValue.Sha512 => "SHA-512",
        _ => throw new ArgumentOutOfRangeException()
    };

    public record InvalidDigestAlgorithmError(string Value)
        : Error($"Invalid digest algorithm. The algorithm must be either SHA-256, SHA-384 or SHA-512. Actual value is {Value}");
}

public enum DigestAlgorithmValue
{
    Sha256,
    Sha384,
    Sha512
}
