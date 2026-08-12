using FluentAssertions;
using LanguageExt;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vp.Models;
using static WalletFramework.Oid4Vp.Tests.AuthRequest.Samples.X509HashSamples;

namespace WalletFramework.Oid4Vp.Tests.AuthRequest;

public class X509HashTests
{
    [Fact]
    public void Request_Object_With_The_X509_Hash_Prefix_Is_Recognized_As_The_X509_Hash_Scheme()
    {
        var requestObject = RequestObject
            .FromStr(SignedRequestObjectWithMatchingCertificateHash(), Option<string>.None)
            .UnwrapOrThrow();

        var scheme = requestObject.ClientIdScheme.Value;

        scheme.Should().Be(ClientIdScheme.ClientIdSchemeValue.X509Hash);
    }

    [Fact]
    public void A_Client_Id_Hash_Matching_The_Leaf_Certificate_Is_Accepted()
    {
        var requestObject = RequestObject
            .FromStr(SignedRequestObjectWithMatchingCertificateHash(), Option<string>.None)
            .UnwrapOrThrow();

        var sut = requestObject.ValidateCertificateHash();

        sut.Should().NotBeNull();
    }

    [Fact]
    public void A_Client_Id_Hash_Not_Matching_The_Leaf_Certificate_Is_Rejected()
    {
        var requestObject = RequestObject
            .FromStr(SignedRequestObjectWithMismatchedCertificateHash(), Option<string>.None)
            .UnwrapOrThrow();

        var validateMismatchedHash = () => requestObject.ValidateCertificateHash();

        validateMismatchedHash.Should().Throw<Exception>();
    }

    [Fact]
    public void A_Request_Object_Without_Certificates_Is_Rejected()
    {
        var requestObject = RequestObject
            .FromStr(SignedRequestObjectWithoutX5c(), Option<string>.None)
            .UnwrapOrThrow();

        var validateWithoutCertificates = () => requestObject.ValidateCertificateHash();

        validateWithoutCertificates.Should().Throw<Exception>();
    }
}
