using Newtonsoft.Json.Linq;
using WalletFramework.Core.Functional;
using WalletFramework.Core.Json.Errors;

namespace WalletFramework.Oid4Vc.Oid4Vp.Dcql.Models;

/// <summary>
/// Represents a set of claim identifiers used in DCQL.
/// </summary>
public record ClaimSet
{
    public IReadOnlyList<ClaimIdentifier> Claims { get; }

    public ClaimSet(IReadOnlyList<ClaimIdentifier> claims) => Claims = claims;
}

public static class ClaimSetFun
{
    public static Validation<ClaimSet> Validate(JArray array)
    {
        if (array.Count == 0)
            return new JArrayIsNullOrEmptyError<ClaimSet>();

        return 
            from ids in array.TraverseAll(token =>
            {
                var set = token.Type == JTokenType.String
                    ? token.Value<string>()
                    : token.ToString();

                return ClaimIdentifier.Validate(set);
            })
            select new ClaimSet([.. ids]);
    }

    public static Validation<IEnumerable<ClaimSet>> ValidateMany(JArray array)
    {
        if (array.Count == 0)
            return new JArrayIsNullOrEmptyError<IEnumerable<ClaimSet>>();

        return array.TraverseAll(token =>
        {
            var set = token.Type == JTokenType.Array 
                ? (JArray)token 
                : new JArray(token);

            return Validate(set);
        });
    }
} 
