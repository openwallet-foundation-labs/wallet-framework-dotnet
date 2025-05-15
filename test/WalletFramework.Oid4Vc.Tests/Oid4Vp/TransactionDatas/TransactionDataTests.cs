using FluentAssertions;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Oid4Vc.Oid4Vp.Dcql;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;
using WalletFramework.Oid4Vc.Tests.Oid4Vp.Dcql.Samples;
using WalletFramework.Oid4Vc.Tests.Samples;

namespace WalletFramework.Oid4Vc.Tests.Oid4Vp.TransactionDatas;

public class TransactionDataTests
{
    [Fact]
    public void Transaction_Data_Are_Matched_To_Candidate_Correctly()
    {
        // Arrange
        var authRequest = TransactionDataSamples.GetAuthRequestWithSingleCredentialTransactionData();
        var sdJwtRecord = SdJwtSamples.GetIdCardCredential();
        
        var credentials = new List<ICredential> { sdJwtRecord };
        var candidateQueryResult = authRequest.Requirements.Match(
            dcql => dcql.ProcessWith(credentials),
            _ => throw new NotSupportedException("Only DCQL flow supported in this test")
        );

        var presentationRequest = new PresentationRequest(authRequest, candidateQueryResult);
        var transactionDatas = authRequest.TransactionData.IfNone([]);

        // Act
        var result = TransactionDataFun.ProcessVpTransactionData(presentationRequest, transactionDatas);

        // Assert
        result.Match(
            updatedRequest =>
            {
                updatedRequest.CandidateQueryResult.Candidates.Match(
                    sets =>
                    {
                        sets.Should().HaveCount(1);
                        sets[0].Candidates[0].TransactionData.Match(
                            data => data.Should().Contain(transactionDatas[0]),
                            () => Assert.Fail("Expected transaction data to be present")
                        );
                    },
                    () => Assert.Fail("Expected candidate sets to be present")
                );
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }

    [Fact]
    public void Transaction_Data_Are_Matched_To_Multiple_Credentials_Sets_Correctly()
    {
        // Arrange
        var authRequest = TransactionDataSamples.GetAuthRequestWithTwoCredentialsTransactionData();

        var idCardCredential = SdJwtSamples.GetIdCardCredential();
        var idCard2Credential = SdJwtSamples.GetIdCard2Credential();

        var credentials = new List<ICredential> { idCardCredential, idCard2Credential };
        var candidateQueryResult = authRequest.Requirements.Match(
            dcql => dcql.ProcessWith(credentials),
            _ => throw new NotSupportedException("Only DCQL flow supported in this test")
        );

        var presentationRequest = new PresentationRequest(authRequest, candidateQueryResult);
        var transactionDatas = authRequest.TransactionData.IfNone([]);

        // Act
        var result = TransactionDataFun.ProcessVpTransactionData(presentationRequest, transactionDatas);

        // Assert
        result.Match(
            updatedRequest =>
            {
                updatedRequest.CandidateQueryResult.Candidates.Match(
                    sets =>
                    {
                        var idCardCandidate = sets[0].Candidates.FirstOrDefault(c => c.Identifier == "idcard");
                        var idCard2Candidate = sets[1].Candidates.FirstOrDefault(c => c.Identifier == "idcard2");
                        idCardCandidate!.TransactionData.Match(
                            data => data.Should().Contain(transactionDatas[0]),
                            () => Assert.Fail("Expected transaction data to be present for idcard candidate")
                        );
                        idCard2Candidate!.TransactionData.Match(
                            _ => Assert.Fail("Expected no transaction data for idcard2 candidate"),
                            () => { }
                        );
                    },
                    () => Assert.Fail("Expected candidate sets to be present")
                );
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }

    [Fact]
    public void Transaction_Data_Are_Matched_To_Multiple_Candidates_In_One_Set_Correctly()
    {
        // Arrange
        var authRequest = TransactionDataSamples.GetAuthRequestWithMultipleCandidatesInOneSetTransactionData();
        var idCardCredential1 = SdJwtSamples.GetIdCardCredential();
        var idCardCredential2 = SdJwtSamples.GetIdCard2Credential();
        var credentials = new List<ICredential> { idCardCredential1, idCardCredential2 };
        var candidateQueryResult = authRequest.Requirements.Match(
            dcql => dcql.ProcessWith(credentials),
            _ => throw new NotSupportedException("Only DCQL flow supported in this test")
        );

        var presentationRequest = new PresentationRequest(authRequest, candidateQueryResult);
        var transactionDatas = authRequest.TransactionData.IfNone([]);

        // Act
        var result = TransactionDataFun.ProcessVpTransactionData(presentationRequest, transactionDatas);

        // Assert
        result.Match(
            updatedRequest =>
            {
                updatedRequest.CandidateQueryResult.Candidates.Match(
                    sets =>
                    {
                        sets.Should().HaveCount(1);
                        var candidateSet = sets[0];
                        candidateSet.Candidates.Should().HaveCount(2);
                        candidateSet.Candidates[0].TransactionData.Match(
                            data => data.Should().Contain(transactionDatas[0]),
                            () => Assert.Fail("Expected transaction data to be present for candidate")
                        );
                    },
                    () => Assert.Fail("Expected candidate sets to be present")
                );
            },
            error => Assert.Fail($"Expected success but got error: {error}")
        );
    }
} 
