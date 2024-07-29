using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Digests;
using WalletFramework.MdocLib.Elements;
using WalletFramework.MdocLib.Issuer;
using WalletFramework.MdocLib.Security.Cose;
using Xunit;
using static WalletFramework.MdocLib.Mdoc;
using static WalletFramework.MdocLib.Constants;

namespace WalletFramework.MdocLib.Tests;

public class MdocTests
{
    [Fact]
    public void Can_Encode()
    {
        // Arrange
        var encodedSample = Samples.EncodedMdoc;

        // Act
        ValidMdoc(encodedSample).Match(
            mdoc =>
            {
                var sut = mdoc.Encode();
                sut.Should().Be(Samples.EncodedMdoc);
            },
            _ => Assert.Fail("Validation of mDoc failed")
        );
    }

    // TODO: Implement this
    [Fact]
    public void Mdoc_With_Non_Matching_DocType_Is_Rejected()
    {
    }

    [Fact]
    public void Can_Parse_Mdoc()
    {
        // Arrange
        var encoded = Samples.EncodedMdoc;

        // Act
        ValidMdoc(encoded).Match(sut =>
            {
                // Assert
                sut.DocType.ToString().Should().Be(Samples.DocType);

                var issuerSignedItems = sut.IssuerSigned.IssuerNameSpaces[Samples.MdlIsoNameSpace];

                issuerSignedItems[0].ElementId.Value.Should().Be("issue_date");
                issuerSignedItems[0].Element.Value.AsT0.Value.Should().Be("2024-01-12");
                issuerSignedItems[1].ElementId.Value.Should().Be("expiry_date");
                issuerSignedItems[1].Element.Value.AsT0.Value.Should().Be("2025-01-12");
                issuerSignedItems[2].ElementId.Value.Should().Be("family_name");
                issuerSignedItems[2].Element.Value.AsT0.Value.Should().Be("Silverstone");
                issuerSignedItems[3].ElementId.Value.Should().Be("given_name");
                issuerSignedItems[3].Element.Value.AsT0.Value.Should().Be("Inga");
                issuerSignedItems[4].ElementId.Value.Should().Be("birth_date");
                issuerSignedItems[4].Element.Value.AsT0.Value.Should().Be("1991-11-06");
                issuerSignedItems[5].ElementId.Value.Should().Be("issuing_country");
                issuerSignedItems[5].Element.Value.AsT0.Value.Should().Be("US");
                issuerSignedItems[6].ElementId.Value.Should().Be("document_number");
                issuerSignedItems[6].Element.Value.AsT0.Value.Should().Be("12345678");
                issuerSignedItems[7].ElementId.Value.Should().Be("driving_privileges");

                var privileges = issuerSignedItems[7].Element.Value.AsT1[0].Value.AsT2.Value;

                privileges[Samples.VehicleCategoryCodeIdentifier].Value.AsT0.Value.Should().Be("A");
                privileges[Samples.IssueDateIdentifier].Value.AsT0.Value.Should().Be("2023-01-01");
                privileges[Samples.ExpiryDateIdentifier].Value.AsT0.Value.Should().Be("2043-01-01");

                var issuerAuth = sut.IssuerSigned.IssuerAuth;

                issuerAuth.ProtectedHeaders[Samples.Es256CoseLabel].Value.Should().Be(ProtectedHeaders.Alg.AlgValue.Es256);

                // TODO: Better cert assertion
                issuerAuth.UnprotectedHeaders[Samples.X509ChainCoseLabel].Should().BeOfType<CBORObject>();

                issuerAuth.Payload.DigestAlgorithm.Value.Should().Be(DigestAlgorithmValue.Sha256);
                issuerAuth.Payload.DocType.ToString().Should().Be(Samples.DocType);
                issuerAuth.Payload.Version.Should().Be(new Version("1.0"));

                issuerAuth.Payload.ValidityInfo.ValidFrom.Should().Be(new DateTime(2024, 01, 12, 01, 10, 05));
                issuerAuth.Payload.ValidityInfo.ValidUntil.Should().Be(new DateTime(2025, 01, 12, 01, 10, 05));
                issuerAuth.Payload.ValidityInfo.Signed.Should().Be(new DateTime(2024, 01, 12, 01, 10, 05));
                issuerAuth.Payload.ValidityInfo.ExpectedUpdate.IsNone.Should().BeTrue();
            },
            _ => Assert.Fail("Validation of mDoc failed"));
    }

    [Fact]
    public void Can_Selectively_Disclose()
    {
        // Arrange
        var sample = Samples.EncodedMdoc;

        ValidMdoc(sample).Match(
            sut =>
            {
                var disclosures = new List<ElementIdentifier>
                {
                    Samples.GivenName,
                    Samples.FamilyName,
                    Samples.DrivingPrivileges
                };

                // Act
                sut.SelectivelyDisclose(Samples.MdlIsoNameSpace, disclosures);

                // Assert
                var items = sut.IssuerSigned.IssuerNameSpaces.Value[Samples.MdlIsoNameSpace];
                items.Count.Should().Be(disclosures.Count);
                items.Should().Contain(item => item.ElementId.Value == Samples.GivenName);
                items.Should().Contain(item => item.ElementId.Value == Samples.FamilyName);
                items.Should().Contain(item => item.ElementId.Value == Samples.DrivingPrivileges);
            },
            _ => Assert.Fail("Validation of mdoc failed")
        );
    }

    [Fact]
    public void Can_Selectively_Disclose_And_Encode()
    {
        // Arrange
        var sample = Samples.EncodedMdoc;

        ValidMdoc(sample).Match(
            mdoc =>
            {
                var disclosures = new List<ElementIdentifier>
                {
                    Samples.GivenName,
                    Samples.FamilyName,
                    Samples.DrivingPrivileges
                };

                // Act
                mdoc.SelectivelyDisclose(Samples.MdlIsoNameSpace, disclosures);
                var sut = mdoc.Encode();

                // Assert
                sut.Should().Be(Samples.SelectivelyDisclosedEncodedMdoc);
            },
            _ => Assert.Fail("Validation of mDoc failed")
        );
    }

    [Fact]
    public void Mdoc_With_Invalid_Digests_Is_Rejected()
    {
        // Arrange
        var mdoc = Samples.EncodedMdoc;
        var base64 = Base64UrlEncoder.DecodeBytes(mdoc);
        var cbor = CBORObject.DecodeFromBytes(base64);

        var msoEncodedBytes = cbor[IssuerSignedLabel][IssuerAuthLabel][2].GetByteString();
        var msoBytes = CBORObject.DecodeFromBytes(msoEncodedBytes).GetByteString();

        var mso = CBORObject.DecodeFromBytes(msoBytes);
        var valueDigests = mso["valueDigests"][Samples.MdlNameSpace];
        valueDigests[2] = CBORObject.FromObject(Helpers.GenerateRandomBytes());
        valueDigests[5] = CBORObject.FromObject(Helpers.GenerateRandomBytes());
        valueDigests[7] = CBORObject.FromObject(Helpers.GenerateRandomBytes());

        var corruptedDigestsBytes = CBORObject.FromObject(valueDigests);
        mso["valueDigests"][Samples.MdlNameSpace] = corruptedDigestsBytes;
        var encodedMso = CBORObject.FromObject(mso.EncodeToBytes());
        var encodedIssuerAuthPayload = CBORObject.FromObject(CBORObject.FromObject(encodedMso).EncodeToBytes());
        cbor[IssuerSignedLabel][IssuerAuthLabel][2] = encodedIssuerAuthPayload;

        var corruptedMdocBytes = cbor.EncodeToBytes();
        var corruptedMdoc = Base64UrlEncoder.Encode(corruptedMdocBytes);

        // Act
        ValidMdoc(corruptedMdoc).Match(
            // Assert
            _ => Assert.Fail("mdoc must not invalid"),
            sut =>
            {
                sut.Should().Contain(error => ((InvalidDigestError)error).Id.Value == 2);
                sut.Should().Contain(error => ((InvalidDigestError)error).Id.Value == 5);
                sut.Should().Contain(error => ((InvalidDigestError)error).Id.Value == 7);
            });
    }

    [Fact]
    public void Mdoc_With_Invalid_Structure_Is_Rejected()
    {
        // Arrange
        var mdoc = Samples.EncodedMdoc;
        var base64 = Base64UrlEncoder.DecodeBytes(mdoc);
        var cbor = CBORObject.DecodeFromBytes(base64);

        cbor[IssuerSignedLabel][IssuerAuthLabel][1] = CBORObject.FromObject("schwachsinn");
        cbor[IssuerSignedLabel][IssuerAuthLabel].RemoveAt(3);
        cbor[IssuerSignedLabel][NameSpacesLabel][Samples.MdlNameSpace][6] = CBORObject.FromObject("hier steht nur quatsch");
        cbor[IssuerSignedLabel][NameSpacesLabel][Samples.MdlNameSpace].RemoveAt(4);
        cbor.Remove(DocTypeLabel);

        var bytes = cbor.EncodeToBytes();
        var encoded = Base64UrlEncoder.Encode(bytes);

        // Act
        ValidMdoc(encoded).Match(
            _ => Assert.Fail("Mdoc must invalid"),
            sut =>
            {
                sut.Should().ContainItemsAssignableTo<CborFieldNotFoundError>();
                sut.Should().ContainItemsAssignableTo<InvalidCborByteStringError>();
                sut.Should().ContainItemsAssignableTo<CborIsNotAMapOrAnArrayError>();
            }
        );
    }
}
