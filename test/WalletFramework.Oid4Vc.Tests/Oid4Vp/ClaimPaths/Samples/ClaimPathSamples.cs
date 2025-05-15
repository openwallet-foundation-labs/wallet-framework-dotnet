using Newtonsoft.Json.Linq;
using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Tests.Samples;
using static WalletFramework.Core.ClaimPaths.ClaimPath;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.ClaimPaths.Samples;

public static class ClaimPathSamples
{
    public static string JsonBasedCredentialSample => JsonBasedCredentialSamples.IdCardCredentialAsJsonStr;
    
    public static ClaimPath SelectFirstNameByKeySample =>
        FromJArray(["first_name"]).UnwrapOrThrow();

    public static ClaimPath SelectAllElementsInArraySample =>
        FromJArray(["degrees", null!]).UnwrapOrThrow();

    public static ClaimPath SelectSecondIndexSample =>
        FromJArray(["nationalities", 1]).UnwrapOrThrow();

    public static ClaimPath NonExistentKeySample =>
        FromJArray(["nonexistent_key"]).UnwrapOrThrow();

    public static ClaimPath IndexOnNonArraySample =>
        FromJArray(["first_name", 0]).UnwrapOrThrow();
    
    public static ClaimPath NullOnNonArraySample =>
        FromJArray(["first_name", null!]).UnwrapOrThrow();

    public static ClaimPath StringOnNonObjectSample =>
        FromJArray(["degrees", "not_a_key"]).UnwrapOrThrow();
}
