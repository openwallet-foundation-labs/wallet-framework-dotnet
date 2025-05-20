using LanguageExt;
using WalletFramework.Core.Base64Url;
using WalletFramework.Core.Functional;
using WalletFramework.Oid4Vc.Oid4Vp.Errors;
using WalletFramework.Oid4Vc.Oid4Vp.Models;
using WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas.Errors;
using WalletFramework.Oid4Vc.Qes.Authorization;

namespace WalletFramework.Oid4Vc.Oid4Vp.TransactionDatas;

internal static class TransactionDataFun
{
    public static Validation<CandidateTxDataMatch> FindCandidateForTransactionData(
        this IEnumerable<PresentationCandidate> candidates,
        TransactionData transactionData)
    {
        var result = candidates.FirstOrDefault(candidate =>
        {
            return transactionData
                .GetTransactionDataProperties()
                .CredentialIds
                .Select(id => id.AsString)
                .Contains(candidate.Identifier);
        });
        
        if (result is null)
        {
            var error = new InvalidTransactionDataError("Not enough credentials found to satisfy the authorization request with transaction data");
            return error.ToInvalid<CandidateTxDataMatch>();
        }
        else
        {
            return new CandidateTxDataMatch(result, transactionData);
        }
    }

    public static Base64UrlString GetEncoded(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().Encoded;

    public static IEnumerable<TransactionDataHashesAlg> GetHashesAlg(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().TransactionDataHashesAlg;

    public static TransactionDataType GetTransactionDataType(this TransactionData transactionData) =>
        transactionData.GetTransactionDataProperties().Type;

    internal static Validation<AuthorizationRequestCancellation, PresentationRequest> ProcessUc5TransactionData(
        PresentationRequest presentationRequest,
        IEnumerable<InputDescriptorTransactionData> txData)
    {
        var result = presentationRequest.CandidateQueryResult.Candidates.Match(
            candidateSets =>
            {
                // Flatten to (setIndex, candidateIndex, candidate)
                var indexedCandidates = candidateSets
                    .SelectMany((set, setIdx) =>
                        set.Candidates.Select((candidate, candIdx) => (setIdx, candIdx, candidate)))
                    .ToList();

                var updatedCandidates = indexedCandidates.ToDictionary(x => (x.setIdx, x.candIdx), x => x.candidate);

                foreach (var inputDescriptorTxData in txData)
                {
                    var found = indexedCandidates.FirstOrDefault(x =>
                        x.candidate.Identifier == inputDescriptorTxData.InputDescriptorId);

                    if (found == default)
                    {
                        return new InvalidTransactionDataError(
                            $"No credentials found that satisfy the authorization request for input descriptor {inputDescriptorTxData.InputDescriptorId}",
                            presentationRequest).ToInvalid<PresentationRequest>();
                    }

                    // Add the UC5 transaction data to the candidate
                    var updated = found.candidate.AddUc5TransactionData(inputDescriptorTxData.TransactionData);
                    updatedCandidates[(found.setIdx, found.candIdx)] = updated;
                }

                // Reconstruct sets with updated candidates
                var newSets = candidateSets.Select((set, setIdx) =>
                    set with
                    {
                        Candidates = [.. set.Candidates.Select((_, candIdx) => updatedCandidates[(setIdx, candIdx)])]
                    }).ToList();

                var newResult = presentationRequest.CandidateQueryResult with
                {
                    Candidates = newSets
                };

                return presentationRequest with { CandidateQueryResult = newResult };
            },
            () => presentationRequest
        );

        return result.Value.MapFail(error =>
        {
            var responseUriOption = presentationRequest.AuthorizationRequest.GetResponseUriMaybe();
            var vpError = error as VpError ?? new InvalidRequestError("Could not parse the Authorization Request");
            return new AuthorizationRequestCancellation(responseUriOption, [vpError]);
        });
    }

    internal static Validation<AuthorizationRequestCancellation, PresentationRequest> ProcessVpTransactionData(
        PresentationRequest presentationRequest,
        IEnumerable<TransactionData> transactionDatas)
    {
        var result = presentationRequest.CandidateQueryResult.Candidates.Match(
            candidateSets =>
            {
                // Flatten to (setIndex, candidateIndex, candidate)
                var indexedCandidates = candidateSets
                    .SelectMany((set, setIdx) =>
                    {
                        return set.Candidates.Select((candidate, candIdx) => (setIdx, candIdx, candidate));
                    })
                    .ToList();

                var updatedCandidates = indexedCandidates.ToDictionary(
                    tuple => (tuple.setIdx, tuple.candIdx),
                    tuple => tuple.candidate
                );

                foreach (var txData in transactionDatas)
                {
                    var found = indexedCandidates.FirstOrDefault(tuple =>
                    {
                        return new[] { tuple.candidate }.FindCandidateForTransactionData(txData).IsSuccess;
                    });

                    if (found == default)
                    {
                        return new InvalidTransactionDataError(
                            $"No credentials found that satisfy the transaction data with type {txData.GetTransactionDataType().AsString()}",
                            presentationRequest).ToInvalid<PresentationRequest>();
                    }

                    // Update the candidate with the transaction data
                    var updated = found.candidate.AddTransactionDatas([txData]);
                    updatedCandidates[(found.setIdx, found.candIdx)] = updated;
                }

                // Reconstruct sets with updated candidates
                var newSets = candidateSets.Select((set, setIdx) =>
                    set with
                    {
                        Candidates = [.. set.Candidates.Select((_, candIdx) => updatedCandidates[(setIdx, candIdx)])]
                    }).ToList();

                var newResult = presentationRequest.CandidateQueryResult with
                {
                    Candidates = newSets
                };

                return presentationRequest with { CandidateQueryResult = newResult };
            },
            () => new InvalidTransactionDataError(
                    "No credentials found that satisfy the authorization request with transaction data",
                    presentationRequest)
                .ToInvalid<PresentationRequest>()
        );

        return result.Value.MapFail(error =>
        {
            var responseUriOption = presentationRequest.AuthorizationRequest.GetResponseUriMaybe();
            var vpError = error as VpError ?? new InvalidRequestError("Could not parse the Authorization Request");
            return new AuthorizationRequestCancellation(responseUriOption, [vpError]);
        });
    }

    private static TransactionDataProperties GetTransactionDataProperties(this TransactionData transactionData) =>
        transactionData.Match(
            payment => payment.TransactionDataProperties,
            qes => qes.TransactionDataProperties,
            qcert => qcert.TransactionDataProperties);
}
