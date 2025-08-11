using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.SdJwtVc;
using WalletFramework.Storage.Database;
using WalletFramework.SdJwtVc.Models.Credential;
using WalletFramework.SdJwtVc.Persistence;
using SD_JWT.Models;

namespace WalletFramework.Storage.Tests;

public class SdJwtCredentialRecordCrudTests : IDisposable
{
    public SdJwtCredentialRecordCrudTests() => (_serviceProvider, _dbPath) = TestDbSetup.CreateServiceProvider();

    private readonly ServiceProvider _serviceProvider;

    private readonly string _dbPath;

    [Fact]
    public async Task Can_Store_And_Retrieve_SdJwtCredentialRecord()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId>>();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        const string encoded =
            "eyJraWQiOiJiZmFmYjkzMy1iNzQ4LTQ3ODYtODc1Ny0zYzg0ZWFlNmUzZGUiLCJ0eXAiOiJ2YytzZC1qd3QiLCJhbGciOiJFUzI1NiJ9.eyJfc2QiOlsiUVBEUFFCbEEzdk9QaU9qR0lRRXBOc1l5S2Zjd2M1T3dDUlV5eWY2QTlRbyIsIk9LMWJpZXUwR0RIZWVRc2lzRkxOcUdmX0Z4eW5HT0dTNHl5Q2dZeFVhTkEiLCJUSkJ4ajBGSmdTQlUxMzVDSDRacFJieTRfVG4tNWR4TFJBX0paRnNscXhjIiwiaFBjV0phVkRJdDlDZ1E3bWxzNmFSVFR6bHZ0NmlMYzlUWFRJZ2VuZDFWayIsIkNhZm9TdzRiMWdsV196ckdyN3lodFFyQ3RIYW51NG15MVBxTGtXQkx5aFkiXSwibmJmIjoxNzA2NTQyNjgxLCJ2Y3QiOiJJRC1DYXJkIiwiX3NkX2FsZyI6InNoYS0yNTYiLCJpc3MiOiJodHRwczovL2U4MGMtMjE3LTExMS0xMDgtMTc0Lm5ncm9rLWZyZWUuYXBwIiwiY25mIjp7Imp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6Img2VUtiVXQ1SW4yTzVwUzUxYXRWaERuTDl0SGR4S3lkMTZXTG94R2dFQzQiLCJ5IjoiMDdIX05RcmlxRmxSb0JjVk5ZVW5aS2wwQ1A0U0NiN3RxU0NWWFNDTWh0ayJ9fSwiZXhwIjoxNzM4MjUxNDgxLCJpYXQiOjE3MTY5OTA0MDAsInN0YXR1cyI6eyJpZHgiOjYsInVyaSI6Imh0dHBzOi8vZTgwYy0yMTctMTExLTEwOC0xNzQubmdyb2stZnJlZS5hcHAvc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmQ1MDllMzYtNTQzNy00Zjg4LTkzYTUtNDEzNDA3ZjZiZDhmIn19.-3GEPOjEn4bopEGyy8ho_kFSfQVmkkZiFKMebtiZE6EsyRnunJtA46M_SwHQjmSm-73zIeRX7L7Rpszm8dkFhQ~WyJfSU1WWFVtc052bm9YTDR3NVRPSFpnIiwiYWRkcmVzcyIseyJzdHJlZXRfYWRkcmVzcyI6IjQyIE1hcmtldCBTdHJlZXQiLCJwb3N0YWxfY29kZSI6IjEyMzQ1In1d~WyJ3RzkzbExRRFBDUVgxTUtCYW5mVkVRIiwibGFzdF9uYW1lIiwiRG9lIl0~WyJFa1h0a0JHZXd2dkthRXlzTWhyVGJnIiwibmF0aW9uYWxpdGllcyIsWyJCcml0aXNoIiwiQmV0ZWxnZXVzaWFuIl1d~WyJzQlh2dVQxRHhaN0NrMTdJUXQzWWd3IiwiZmlyc3RfbmFtZSIsIkpvaG4iXQ~WyJoRWphWTA2WmFsNUZTS0pXSm9kUjZnIiwiZGVncmVlcyIsW3sidW5pdmVyc2l0eSI6IlVuaXZlcnNpdHkgb2YgQmV0ZWxnZXVzZSIsInR5cGUiOiJCYWNoZWxvciBvZiBTY2llbmNlIn0seyJ1bml2ZXJzaXR5IjoiVW5pdmVyc2l0eSBvZiBCZXRlbGdldXNlIiwidHlwZSI6Ik1hc3RlciBvZiBTY2llbmNlIn1dXQ~";

        var sdJwtDoc = new SdJwtDoc(encoded);
        var sdjwt = new SdJwtCredential(
            sdJwtDoc,
            credentialId,
            credentialSetId,
            Option<List<SdJwtDisplay>>.None,
            keyId,
            CredentialState.Active,
            false,
            Option<DateTime>.None);

        await repository.Add(sdjwt);

        var fetched = await repository.GetById(credentialId);

        fetched.Match(
            found =>
            {
                found.CredentialId.Should().Be(credentialId);
                found.EncodedIssuerSignedJwt.Should().Be(sdJwtDoc.IssuerSignedJwt);
                found.CredentialSetId.AsString().Should().Be(credentialSetId.AsString());
                found.KeyId.AsString().Should().Be(keyId.AsString());
                found.Disclosures.Should().BeEquivalentTo(sdjwt.Disclosures);
                found.Claims.Should().BeEquivalentTo(sdjwt.Claims);
            },
            () => throw new InvalidOperationException("Record should exist")
        );
    }

    [Fact]
    public async Task Can_Delete_SdJwtCredentialRecord_By_Domain()
    {
        var databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
        await databaseCreator.CreateDatabase();

        var repository = _serviceProvider
            .GetRequiredService<IDomainRepository<SdJwtCredential, SdJwtCredentialRecord, CredentialId>>();

        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();

        const string encoded =
            "eyJraWQiOiJiZmFmYjkzMy1iNzQ4LTQ3ODYtODc1Ny0zYzg0ZWFlNmUzZGUiLCJ0eXAiOiJ2YytzZC1qd3QiLCJhbGciOiJFUzI1NiJ9.eyJfc2QiOlsiUVBEUFFCbEEzdk9QaU9qR0lRRXBOc1l5S2Zjd2M1T3dDUlV5eWY2QTlRbyIsIk9LMWJpZXUwR0RIZWVRc2lzRkxOcUdmX0Z4eW5HT0dTNHl5Q2dZeFVhTkEiLCJUSkJ4ajBGSmdTQlUxMzVDSDRacFJieTRfVG4tNWR4TFJBX0paRnNscXhjIiwiaFBjV0phVkRJdDlDZ1E3bWxzNmFSVFR6bHZ0NmlMYzlUWFRJZ2VuZDFWayIsIkNhZm9TdzRiMWdsV196ckdyN3lodFFyQ3RIYW51NG15MVBxTGtXQkx5aFkiXSwibmJmIjoxNzA2NTQyNjgxLCJ2Y3QiOiJJRC1DYXJkIiwiX3NkX2FsZyI6InNoYS0yNTYiLCJpc3MiOiJodHRwczovL2U4MGMtMjE3LTExMS0xMDgtMTc0Lm5ncm9rLWZyZWUuYXBwIiwiY25mIjp7Imp3ayI6eyJrdHkiOiJFQyIsImNydiI6IlAtMjU2IiwieCI6Img2VUtiVXQ1SW4yTzVwUzUxYXRWaERuTDl0SGR4S3lkMTZXTG94R2dFQzQiLCJ5IjoiMDdIX05RcmlxRmxSb0JjVk5ZVW5aS2wwQ1A0U0NiN3RxU0NWWFNDTWh0ayJ9fSwiZXhwIjoxNzM4MjUxNDgxLCJpYXQiOjE3MTY5OTA0MDAsInN0YXR1cyI6eyJpZHgiOjYsInVyaSI6Imh0dHBzOi8vZTgwYy0yMTctMTExLTEwOC0xNzQubmdyb2stZnJlZS5hcHAvc3RhdHVzLWxpc3RzP3JlZ2lzdHJ5SWQ9YmQ1MDllMzYtNTQzNy00Zjg4LTkzYTUtNDEzNDA3ZjZiZDhmIn19.-3GEPOjEn4bopEGyy8ho_kFSfQVmkkZiFKMebtiZE6EsyRnunJtA46M_SwHQjmSm-73zIeRX7L7Rpszm8dkFhQ~WyJfSU1WWFVtc052bm9YTDR3NVRPSFpnIiwiYWRkcmVzcyIseyJzdHJlZXRfYWRkcmVzcyI6IjQyIE1hcmtldCBTdHJlZXQiLCJwb3N0YWxfY29kZSI6IjEyMzQ1In1d~WyJ3RzkzbExRRFBDUVgxTUtCYW5mVkVRIiwibGFzdF9uYW1lIiwiRG9lIl0~WyJFa1h0a0JHZXd2dkthRXlzTWhyVGJnIiwibmF0aW9uYWxpdGllcyIsWyJCcml0aXNoIiwiQmV0ZWxnZXVzaWFuIl1d~WyJzQlh2dVQxRHhaN0NrMTdJUXQzWWd3IiwiZmlyc3RfbmFtZSIsIkpvaG4iXQ~WyJoRWphWTA2WmFsNUZTS0pXSm9kUjZnIiwiZGVncmVlcyIsW3sidW5pdmVyc2l0eSI6IlVuaXZlcnNpdHkgb2YgQmV0ZWxnZXVzZSIsInR5cGUiOiJCYWNoZWxvciBvZiBTY2llbmNlIn0seyJ1bml2ZXJzaXR5IjoiVW5pdmVyc2l0eSBvZiBCZXRlbGdldXNlIiwidHlwZSI6Ik1hc3RlciBvZiBTY2llbmNlIn1dXQ~";
        var sdJwtDoc = new SdJwtDoc(encoded);
        var sdjwt = new SdJwtCredential(
            sdJwtDoc,
            credentialId,
            credentialSetId,
            Option<List<SdJwtDisplay>>.None,
            keyId,
            CredentialState.Active,
            false,
            Option<DateTime>.None);

        await repository.Add(sdjwt);

        await repository.Delete(sdjwt);

        var fetched = await repository.GetById(credentialId);
        fetched.IsNone.Should().BeTrue();
    }

    public void Dispose()
    {
        TestDbSetup.Cleanup(_serviceProvider, _dbPath);
        GC.SuppressFinalize(this);
    }
}
