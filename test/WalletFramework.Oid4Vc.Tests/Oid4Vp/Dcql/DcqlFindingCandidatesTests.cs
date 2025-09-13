using FluentAssertions;
using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Cryptography.Models;
using WalletFramework.Core.Functional;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.MdocVc.Display;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;
using static WalletFramework.Oid4Vc.Oid4Vp.Dcql.DcqlFun;
using MdocSamples = WalletFramework.TestSamples.MdocSamples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql;

public class DcqlFindingCandidatesTests
{
    [Fact]
    public void Can_Process_Mdoc_Credential_Query()
    {
        var mdoc = GetMdocCredentialSample();
        var query = DcqlSamples.GetMdocGivenNameQuery();

        var sut = query.ProcessWith([mdoc]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Mdoc_Credential_Query_With_Multiple_Claims()
    {
        var mdoc = GetMdocCredentialSample();
        var query = DcqlSamples.GetMdocGivenNameAndFamilyNameQuery();

        var sut = query.ProcessWith([mdoc]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials =
                    from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Mdoc_Credential_Query_With_Multiple_Credentials_In_One_Candidate()
    {
        var mdoc1 = GetMdocCredentialSample();
        var mdoc2 = GetMdocCredentialSample();
        var query = DcqlSamples.GetMdocGivenNameQuery();

        var sut = query.ProcessWith([mdoc1, mdoc2]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc1, mdoc2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_Query_That_Asks_For_SdJwt_And_Mdoc_At_The_Same_Time()
    {
        var mdoc = GetMdocCredentialSample();
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetMdocAndSdJwtFamilyNameQuery();

        var sut = query.ProcessWith([mdoc, sdJwt]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(2);
                foreach (var candidate in presentationCandidates)
                {
                    query.CredentialQueries.Select(q => q.Id.AsString()).Should().Contain(candidate.Identifier);
                    var expectedClaims = query.CredentialQueries.First(q => q.Id == candidate.Identifier).Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail($"Expected all claims to be disclosed for {candidate.Identifier}, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([mdoc, sdJwt]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query()
    {
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetIdCardNationalitiesSecondIndexQuery();

        var sut = query.ProcessWith([sdJwt]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();

                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([sdJwt]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query_With_Multiple_Candidates()
    {
        var idCard = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCard2Credential();
        var query = DcqlSamples.GetIdCardAndIdCard2NationalitiesSecondIndexQuery();

        var sut = query.ProcessWith([idCard, idCard2]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(2);
                foreach (var candidate in presentationCandidates)
                {
                    query.CredentialQueries.Select(q => q.Id.AsString()).Should().Contain(candidate.Identifier);
                    var expectedClaims = query.CredentialQueries.First(q => q.Id == candidate.Identifier).Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail($"Expected all claims to be disclosed for {candidate.Identifier}, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([idCard, idCard2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void Can_Process_SdJwt_Credential_Query_With_Multiple_Credentials_In_One_Candidate()
    {
        var idCard = SdJwtSamples.GetIdCardCredential();
        var idCard2 = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetIdCardNationalitiesSecondIndexQuery();

        var sut = query.ProcessWith([idCard, idCard2]);

        sut.FlattenCandidates().Match(
            candidates =>
            {
                var presentationCandidates = candidates.ToList();
                presentationCandidates.Should().HaveCount(1);
                foreach (var candidate in presentationCandidates)
                {
                    candidate.Identifier.Should().Be(query.CredentialQueries[0].Id);
                    var expectedClaims = query.CredentialQueries[0].Claims!;
                    candidate.ClaimsToDisclose.Match(
                        claims =>
                        {
                            claims.Select(c => c.Path).Should().BeEquivalentTo(expectedClaims.Select(c => c.Path));
                        },
                        () => Assert.Fail("Expected all claims to be disclosed, but got none."));
                }

                var credentials = from candidate in presentationCandidates
                    from set in candidate.CredentialSetCandidates
                    from credential in set.Credentials
                    select credential;

                credentials.Should().Contain([idCard, idCard2]);
            },
            () => Assert.Fail()
        );
    }

    [Fact]
    public void No_Match_Returns_None()
    {
        var sdJwt = SdJwtSamples.GetIdCardCredential();
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();

        var sut = query.ProcessWith([sdJwt]);

        sut.FlattenCandidates().Match(
            _ => Assert.Fail(),
            () => { }
        );
    }

    [Fact]
    public void No_Match_Returns_None_For_Mdoc()
    {
        var mdoc = GetMdocCredentialSample();
        var query = DcqlSamples.GetNoMatchErrorClaimPathQuery();

        var sut = query.ProcessWith([mdoc]);

        sut.FlattenCandidates().Match(
            _ => Assert.Fail(),
            () => { }
        );
    }

    private MdocCredential GetMdocCredentialSample()
    {
        var encodedMdoc = MdocSamples.GetEncodedMdocSample();
        var mdoc = Mdoc.ValidMdoc(encodedMdoc).UnwrapOrThrow();
        var credentialId = CredentialId.CreateCredentialId();
        var credentialSetId = CredentialSetId.CreateCredentialSetId();
        var keyId = KeyId.CreateKeyId();
        var credentialState = CredentialState.Active;
        var oneTimeUse = false;

        var mdocCredential = new MdocCredential(mdoc, credentialId, credentialSetId, Option<List<MdocDisplay>>.None, keyId, credentialState, oneTimeUse, Option<DateTime>.None);
        
        return mdocCredential;
    }
} 
