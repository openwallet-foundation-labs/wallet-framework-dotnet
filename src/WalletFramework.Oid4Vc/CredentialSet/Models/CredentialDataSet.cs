using LanguageExt;
using WalletFramework.Core.Credentials;
using WalletFramework.Core.Credentials.Abstractions;
using WalletFramework.Core.Functional;
using WalletFramework.Core.StatusList;
using WalletFramework.MdocLib;
using WalletFramework.MdocVc;
using WalletFramework.SdJwtVc;
using WalletFramework.SdJwtVc.Models;

namespace WalletFramework.Oid4Vc.CredentialSet.Models;

public sealed record CredentialDataSet(
    CredentialSetId CredentialSetId,
    Option<Vct> SdJwtCredentialType,
    Option<DocType> MDocCredentialType,
    Dictionary<string, string> CredentialAttributes,
    CredentialState State,
    Option<StatusListEntry> StatusListEntry,
    Option<DateTime> ExpiresAt,
    Option<DateTime> IssuedAt,
    Option<DateTime> NotBefore,
    Option<DateTime> RevokedAt,
    Option<DateTime> DeletedAt,
    string IssuerId)
{
    public static CredentialDataSet FromCredentials(IEnumerable<ICredential> credentials) =>
        FromCredentials(credentials, Option<string>.None);

    public static CredentialDataSet FromCredentials(IEnumerable<ICredential> credentials, Option<string> issuerIdHint)
    {
        var credentialArray = credentials as ICredential[] ?? credentials.ToArray();

        if (credentialArray.Length == 0)
            throw new ArgumentException("No credentials provided", nameof(credentials));

        var setId = credentialArray[0].GetCredentialSetId();

        var distinctSetIds = credentialArray
            .Select(c => c.GetCredentialSetId())
            .Distinct()
            .ToArray();

        if (distinctSetIds.Length > 1)
            throw new InvalidOperationException("All credentials must belong to the same credential set.");

        var sdJwtType = Option<Vct>.None;
        var mdocType = Option<DocType>.None;

        var attributes = new Dictionary<string, string>();

        var state = CredentialState.Active;
        var statusListEntry = Option<StatusListEntry>.None;
        var expiresAt = Option<DateTime>.None;
        var issuedAt = Option<DateTime>.None;
        var notBefore = Option<DateTime>.None;
        var revokedAt = Option<DateTime>.None;
        var deletedAt = Option<DateTime>.None;
        var issuerId = string.Empty;

        foreach (var credential in credentialArray)
        {
            switch (credential)
            {
                case SdJwtCredential sdJwtCredential:
                    sdJwtType = sdJwtCredential.Vct;
                    attributes = sdJwtCredential.Claims;
                    state = sdJwtCredential.CredentialState;
                    expiresAt = sdJwtCredential.ExpiresAt;
                    issuedAt = sdJwtCredential.IssuedAt;
                    notBefore = sdJwtCredential.NotBefore;
                    statusListEntry = sdJwtCredential.StatusListEntry;
                    break;

                case MdocCredential mdocCredential:
                    mdocType = mdocCredential.Mdoc.DocType;
                    state = mdocCredential.CredentialState;
                    if (attributes.Count == 0)
                    {
                        attributes = mdocCredential.Mdoc.IssuerSigned.IssuerNameSpaces.Value
                            .First().Value
                            .ToDictionary(
                                issuerSignedItem => issuerSignedItem.ElementId.ToString(),
                                issuerSignedItem => issuerSignedItem.Element.ToString());
                    }

                    if (expiresAt.IsNone)
                        expiresAt = mdocCredential.ExpiresAt;
                    break;

                default:
                    throw new InvalidOperationException("Unknown credential implementation");
            }
        }

        if (string.IsNullOrWhiteSpace(issuerId))
            issuerId = issuerIdHint.Fallback(string.Empty);

        return new CredentialDataSet(
            setId,
            sdJwtType,
            mdocType,
            attributes,
            state,
            statusListEntry,
            expiresAt,
            issuedAt,
            notBefore,
            revokedAt,
            deletedAt,
            issuerId);
    }
}
