using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.TestSamples;
using WalletFramework.MdocVc.Serialization;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using LanguageExt;
using WalletFramework.MdocVc.Display;
using WalletFramework.MdocVc.Persistence;
using Xunit;

namespace WalletFramework.MdocVc.Tests;

public class MdocCredentialTests
{
    [Fact]
    public void Can_Serialize_MdocCredential()
    {
        // Arrange
        var mdoc = Mdoc.ValidMdoc(MdocSamples.GetEncodedMdocSample()).UnwrapOrThrow();
        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();
        var credential = new MdocCredential(
            mdoc,
            credentialId,
            credentialSetId,
            Option<List<MdocDisplay>>.None,
            keyId,
            CredentialState.Active,
            false,
            Option<DateTime>.None
        );

        // Act
        var sut = MdocCredentialSerializer.Serialize(credential);

        // Assert
        var jObject = JObject.Parse(sut);
        jObject[MdocCredentialSerializationConstants.MdocJsonKey]!.ToString().Should().Be(mdoc.Encode());
        jObject[MdocCredentialSerializationConstants.CredentialIdJsonKey]!.ToString().Should().Be(credentialId.AsString());
        jObject[MdocCredentialSerializationConstants.KeyIdJsonKey]!.ToString().Should().Be(keyId.AsString());
        jObject[MdocCredentialSerializationConstants.CredentialStateJsonKey]!.ToString().Should().Be(CredentialState.Active.ToString());
        
    }

    [Fact]
    public void Can_Deserialize_MdocCredential()
    {
        // Arrange
        var json = MdocSamples.GetMdocCredentialSample();

        // Act
        var sut = MdocCredentialSerializer.Deserialize(json).UnwrapOrThrow();

        // Assert
        sut.CredentialState.Should().Be(CredentialState.Active);
        sut.OneTimeUse.Should().BeFalse();
        sut.Mdoc.DocType.AsString().Should().Be(MdocSamples.DocType);
        sut.CredentialId.AsString().Should().Be(MdocSamples.CredentialId);
        sut.CredentialSetId.AsString().Should().Be(MdocSamples.CredentialSetId);
        sut.KeyId.AsString().Should().Be(MdocSamples.KeyId);
        
        // Assert mDOC structure
        var nameSpace = NameSpace.ValidNameSpace(MdocSamples.NameSpace).UnwrapOrThrow();
        var issuerNameSpaces = sut.Mdoc.IssuerSigned.IssuerNameSpaces;
        issuerNameSpaces.Value.Should().ContainKey(nameSpace);
        
        var items = issuerNameSpaces[nameSpace];
        var givenNameItem = items.FirstOrDefault(item => item.ElementId.Value == "given_name");
        var familyNameItem = items.FirstOrDefault(item => item.ElementId.Value == "family_name");
        
        givenNameItem.Should().NotBeNull();
        familyNameItem.Should().NotBeNull();
        givenNameItem!.Element.ToString().Should().Be(MdocSamples.GivenName);
        familyNameItem!.Element.ToString().Should().Be(MdocSamples.FamilyName);
    }

    [Fact]
    public void Can_Map_To_Record()
    {
        // Arrange
        var mdoc = Mdoc.ValidMdoc(MdocSamples.GetEncodedMdocSample()).UnwrapOrThrow();
        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();
        var credential = new MdocCredential(
            mdoc,
            credentialId,
            credentialSetId,
            Option<List<MdocDisplay>>.None,
            keyId,
            CredentialState.Active,
            false,
            Option<DateTime>.None);

        // Act
        var sut = new MdocCredentialRecord(credential);

        // Assert
        sut.DocType.Should().Be(mdoc.DocType.AsString());
        sut.DocType.Should().Be(MdocSamples.DocType);
        
        // Verify that the serialized data contains the correct credential information
        var deserializedCredential = sut.ToDomainModel();
        deserializedCredential.CredentialId.AsString().Should().Be(credentialId.AsString());
        deserializedCredential.CredentialSetId.AsString().Should().Be(credentialSetId.AsString());
        deserializedCredential.KeyId.AsString().Should().Be(keyId.AsString());
    }

    [Fact]
    public void Can_Map_From_Record()
    {
        // Arrange
        var mdoc = Mdoc.ValidMdoc(MdocSamples.GetEncodedMdocSample()).UnwrapOrThrow();
        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();
        var credential = new MdocCredential(
            mdoc,
            credentialId,
            credentialSetId,
            Option<List<MdocDisplay>>.None,
            keyId,
            CredentialState.Active,
            false,
            Option<DateTime>.None
        );

        // Act
        var sut = new MdocCredentialRecord(credential).ToDomainModel();

        // Assert
        sut.CredentialState.Should().Be(CredentialState.Active);
        sut.OneTimeUse.Should().BeFalse();
        sut.Mdoc.DocType.AsString().Should().Be(MdocSamples.DocType);
        sut.CredentialId.AsString().Should().Be(credentialId);
        sut.CredentialSetId.AsString().Should().Be(credentialSetId);
        sut.KeyId.AsString().Should().Be(keyId);
        
        // Assert mDOC structure
        var nameSpace = NameSpace.ValidNameSpace(MdocSamples.NameSpace).UnwrapOrThrow();
        var issuerNameSpaces = sut.Mdoc.IssuerSigned.IssuerNameSpaces;
        issuerNameSpaces.Value.Should().ContainKey(nameSpace);
        
        var items = issuerNameSpaces[nameSpace];
        var givenNameItem = items.FirstOrDefault(item => item.ElementId.Value == "given_name");
        var familyNameItem = items.FirstOrDefault(item => item.ElementId.Value == "family_name");
        
        givenNameItem.Should().NotBeNull();
        familyNameItem.Should().NotBeNull();
        givenNameItem!.Element.ToString().Should().Be(MdocSamples.GivenName);
        familyNameItem!.Element.ToString().Should().Be(MdocSamples.FamilyName);
    }
}
