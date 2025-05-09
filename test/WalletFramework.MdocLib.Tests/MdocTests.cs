using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using PeterO.Cbor;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib.Digests;
using WalletFramework.MdocLib.Elements;
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
        var encodedSample = MdocSamples.EncodedMdoc;

        // Act
        ValidMdoc(encodedSample).Match(
            mdoc =>
            {
                var sut = mdoc.Encode();
                sut.Should().Be(MdocSamples.EncodedMdoc);
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
        var encoded = MdocSamples.EncodedMdoc;

        // Act
        ValidMdoc(encoded).Match(sut =>
            {
                // Assert
                sut.DocType.ToString().Should().Be(MdocSamples.DocType);

                var issuerSignedItems = sut.IssuerSigned.IssuerNameSpaces[MdocSamples.MdlIsoNameSpace];

                issuerSignedItems.Count.Should().Be(6);

                issuerSignedItems[0].ElementId.Value.Should().Be("family_name");
                issuerSignedItems[0].Element.Value.AsT0.Value.Should().Be("Doe");

                issuerSignedItems[1].ElementId.Value.Should().Be("issue_date");
                issuerSignedItems[1].Element.Value.AsT0.Value.Should().Be("2019-10-20");

                issuerSignedItems[2].ElementId.Value.Should().Be("expiry_date");
                issuerSignedItems[2].Element.Value.AsT0.Value.Should().Be("2024-10-20");

                issuerSignedItems[3].ElementId.Value.Should().Be("document_number");
                issuerSignedItems[3].Element.Value.AsT0.Value.Should().Be("123456789");

                issuerSignedItems[5].ElementId.Value.Should().Be("driving_privileges");
                var privilegesArray = issuerSignedItems[5].Element.Value.AsT1; // ElementArray
                privilegesArray.Value.Count.Should().Be(2);

                // First privilege
                var privilege1 = privilegesArray[0].Value.AsT2.Value; // ElementMap
                privilege1[MdocSamples.VehicleCategoryCodeIdentifier].Value.AsT0.Value.Should().Be("A");
                privilege1[MdocSamples.IssueDateIdentifier].Value.AsT0.Value.Should().Be("2018-08-09");
                privilege1[MdocSamples.ExpiryDateIdentifier].Value.AsT0.Value.Should().Be("2024-10-20");

                // Second privilege
                var privilege2 = privilegesArray[1].Value.AsT2.Value;
                privilege2[MdocSamples.VehicleCategoryCodeIdentifier].Value.AsT0.Value.Should().Be("B");
                privilege2[MdocSamples.IssueDateIdentifier].Value.AsT0.Value.Should().Be("2017-02-23");
                privilege2[MdocSamples.ExpiryDateIdentifier].Value.AsT0.Value.Should().Be("2024-10-20");

                var issuerAuth = sut.IssuerSigned.IssuerAuth;

                issuerAuth.ProtectedHeaders[MdocSamples.Es256CoseLabel].Value.Should().Be(ProtectedHeaders.Alg.AlgValue.Es256);

                // TODO: Better cert assertion
                issuerAuth.UnprotectedHeaders[MdocSamples.X509ChainCoseLabel].Should().BeOfType<CBORObject>();

                issuerAuth.Payload.DigestAlgorithm.Value.Should().Be(DigestAlgorithmValue.Sha256);
                issuerAuth.Payload.DocType.ToString().Should().Be(MdocSamples.DocType);
                issuerAuth.Payload.Version.Should().Be(new Version("1.0"));

                // The following dates are not asserted as the sample does not provide them in the decoded JSON
            },
            _ => Assert.Fail("Validation of mDoc failed"));
    }

    [Fact]
    public void Can_Selectively_Disclose()
    {
        // Arrange
        var sample = MdocSamples.EncodedMdoc;
        
        var disclosures = new List<ElementIdentifier>
        {
            MdocSamples.FamilyName,
            MdocSamples.DrivingPrivileges
        };

        var dictionary = new Dictionary<NameSpace, List<ElementIdentifier>>
        {
            { MdocSamples.MdlIsoNameSpace, disclosures }
        };

        ValidMdoc(sample).Match(
            sut =>
            {
                // Act
                sut.SelectivelyDisclose(dictionary);

                // Assert
                var items = sut.IssuerSigned.IssuerNameSpaces.Value[MdocSamples.MdlIsoNameSpace];
                items.Should().Contain(item => item.ElementId.Value == MdocSamples.FamilyName);
                items.Should().Contain(item => item.ElementId.Value == MdocSamples.DrivingPrivileges);
            },
            _ => Assert.Fail("Validation of mdoc failed")
        );
    }

    [Fact]
    public void Can_Selectively_Disclose_And_Encode()
    {
        // Arrange
        var sample = MdocSamples.EncodedMdoc;

        ValidMdoc(sample).Match(
            mdoc =>
            {
                var disclosures = new List<ElementIdentifier>
                {
                    MdocSamples.GivenName,
                    MdocSamples.FamilyName,
                    MdocSamples.DrivingPrivileges
                };
                
                var dictionary = new Dictionary<NameSpace, List<ElementIdentifier>>
                {
                    { MdocSamples.MdlIsoNameSpace, disclosures }
                };

                // Act
                mdoc.SelectivelyDisclose(dictionary);
                var sut = mdoc.Encode();

                // Assert
                sut.Should().Be(MdocSamples.SelectivelyDisclosedEncodedMdoc);
            },
            _ => Assert.Fail("Validation of mDoc failed")
        );
    }

    [Fact]
    public void Mdoc_With_Invalid_Digests_Is_Rejected()
    {
        // Arrange
        var mdoc = MdocSamples.EncodedMdoc;
        var base64 = Base64UrlEncoder.DecodeBytes(mdoc);
        var cbor = CBORObject.DecodeFromBytes(base64);

        var msoEncodedBytes = cbor[IssuerSignedLabel][IssuerAuthLabel][2].GetByteString();
        var msoBytes = CBORObject.DecodeFromBytes(msoEncodedBytes).GetByteString();

        var mso = CBORObject.DecodeFromBytes(msoBytes);
        var valueDigests = mso["valueDigests"][MdocSamples.MdlNameSpace];
        valueDigests[2] = CBORObject.FromObject(Helpers.GenerateRandomBytes());
        valueDigests[5] = CBORObject.FromObject(Helpers.GenerateRandomBytes());
        valueDigests[7] = CBORObject.FromObject(Helpers.GenerateRandomBytes());

        var corruptedDigestsBytes = CBORObject.FromObject(valueDigests);
        mso["valueDigests"][MdocSamples.MdlNameSpace] = corruptedDigestsBytes;
        var encodedMso = CBORObject.FromObject(mso.EncodeToBytes());
        var encodedIssuerAuthPayload = CBORObject.FromObject(CBORObject.FromObject(encodedMso).EncodeToBytes());
        cbor[IssuerSignedLabel][IssuerAuthLabel][2] = encodedIssuerAuthPayload;

        var corruptedMdocBytes = cbor.EncodeToBytes();
        var corruptedMdoc = Base64UrlEncoder.Encode(corruptedMdocBytes);

        // Act
        ValidMdoc(corruptedMdoc).Match(
            // Assert
            _ => Assert.Fail("mdoc must not invalid"),
            _ => {}
        );
    }

    [Fact]
    public void Mdoc_With_Invalid_Structure_Is_Rejected()
    {
        // Arrange
        var mdoc = MdocSamples.EncodedMdoc;
        var base64 = Base64UrlEncoder.DecodeBytes(mdoc);
        var cbor = CBORObject.DecodeFromBytes(base64);

        cbor[IssuerSignedLabel][IssuerAuthLabel][1] = CBORObject.FromObject("schwachsinn");
        cbor[IssuerSignedLabel][IssuerAuthLabel].RemoveAt(3);
        cbor.Remove(DocTypeLabel);

        var bytes = cbor.EncodeToBytes();
        var encoded = Base64UrlEncoder.Encode(bytes);

        // Act
        ValidMdoc(encoded).Match(
            _ => Assert.Fail("Mdoc must invalid"),
            sut => { }
        );
    }
}
