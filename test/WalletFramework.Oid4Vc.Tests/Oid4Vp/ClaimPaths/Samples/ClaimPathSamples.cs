using WalletFramework.Core.ClaimPaths;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Tests.Samples;
using static WalletFramework.Core.ClaimPaths.ClaimPath;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.ClaimPaths.Samples;

public static class ClaimPathSamples
{
    public static string JsonBasedCredentialSample => JsonBasedCredentialSamples.IdCardCredentialAsJsonStr;
    
    public static ClaimPath SelectFirstNameByKeySample =>
        FromObjects(["first_name"]).UnwrapOrThrow();

    public static ClaimPath SelectAllElementsInArraySample =>
        FromObjects(["degrees", null!]).UnwrapOrThrow();

    public static ClaimPath SelectSecondIndexSample =>
        FromObjects(["nationalities", 1]).UnwrapOrThrow();

    public static ClaimPath NonExistentKeySample =>
        FromObjects(["nonexistent_key"]).UnwrapOrThrow();

    public static ClaimPath IndexOnNonArraySample =>
        FromObjects(["first_name", 0]).UnwrapOrThrow();
    
    public static ClaimPath NullOnNonArraySample =>
        FromObjects(["first_name", null!]).UnwrapOrThrow();

    public static ClaimPath StringOnNonObjectSample =>
        FromObjects(["degrees", "not_a_key"]).UnwrapOrThrow();
}
