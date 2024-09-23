using FluentAssertions;
using LanguageExt;
using WalletFramework.Oid4Vc.Oid4Vp.AuthResponse.Encryption;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthResponse.Encryption.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.AuthResponse.Encryption;

public class EncryptedAuthorizationResponseTests
{
    [Fact]
    public void Can_Encrypt_With_Mdoc()
    {
        var nonce = Nonce.GenerateNonce();
        var authResponse = AuthResponseEncryptionSamples.MdocResponse;
        var mdocNonce = Nonce.GenerateNonce();

        var sut = authResponse.Encrypt(
            AuthResponseEncryptionSamples.Jwk,
            nonce.AsBase64Url.ToString(),
            mdocNonce);

        sut.Jwe.Length().Should().Be(AuthResponseEncryptionSamples.ValidMdocJwe.Length);
    }

    [Fact]
    public void Can_Encrypt_With_Sd_Jwt()
    {
        var nonce = Nonce.GenerateNonce();
        var authResponse = AuthResponseEncryptionSamples.SdJwtResponse;

        var sut = authResponse.Encrypt(
            AuthResponseEncryptionSamples.Jwk,
            nonce.AsBase64Url.ToString(),
            Option<Nonce>.None);

        sut.Jwe.Length.Should().Be(AuthResponseEncryptionSamples.ValidSdJwtJwe.Length);
    }
}
