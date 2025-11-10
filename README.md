# Wallet Framework for .NET

Wallet Framework for .NET is an open framework for building digital identity wallets with a focus on [OID4VC](https://openid.net/openid4vc/), [SD-JWT](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-selective-disclosure-jwt-05) and mdoc (ISO/IEC 18013-5).

The framework was initiated as Aries Framework .NET in the Hyperledger Foundation. The Hyperledger Aries and Indy components have been deprecated, are now considered legacy and will not be supported anymore.

## Table of Contents <!-- omit in toc -->

- [Wallet Framework for .NET](#wallet-framework-for-net)
  - [Roadmap](#roadmap)
  - [Protocols](#protocols)
  - [OpenID for Verifiable Credentials (OID4VC)](#openid-for-verifiable-credentials-oid4vc)
  - [Credential Formats](#credential-formats)
  - [Releases / Versioning](#releases--versioning)
  - [License](#license)

## Roadmap
- OpenID4VC Support
- SD-JWT VC Support
- mdoc Support


## Protocols

## OpenID for Verifiable Credentials ([OID4VC](https://openid.net/sg/openid4vc/specifications/))

| Protocol                            | Link                                                                                                                                             | State          |
|-------------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------| -------------- |
| **OpenID for Verifiable Credential Issuance** | [OID4VCI](https://openid.github.io/OpenID4VCI/openid-4-verifiable-credential-issuance-wg-draft.html)                                             | :construction: |
| - Pre-Authorized Code Flow          | [Pre-Auth-Flow](https://openid.github.io/OpenID4VCI/openid-4-verifiable-credential-issuance-wg-draft.html#name-pre-authorized-code-flow)         | :white_check_mark: |
| - Authorization Code Flow           | [Auth-Flow](https://openid.github.io/OpenID4VCI/openid-4-verifiable-credential-issuance-wg-draft.html#name-authorization-code-flow)              | :construction: |
| - Holder Binding / Key Binding      | [KB-JWT](https://openid.github.io/OpenID4VCI/openid-4-verifiable-credential-issuance-wg-draft.html#name-binding-the-issued-credenti)             | :white_check_mark: |
| - Wallet Attestation                | [Wallet Attestation](https://openid.github.io/OpenID4VCI/openid-4-verifiable-credential-issuance-wg-draft.html#name-trust-between-wallet-and-is) | :construction: |
| - Issuer Authentication             | [Issuer Authentication](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0-00.html#name-issuer-identification-and-k)        | :construction: |
| - Demonstrating Proof of Possession | [DPoP](https://openid.net/specs/openid4vc-high-assurance-interoperability-profile-sd-jwt-vc-1_0-00.html#name-crypto-suites)                                       | :construction: |
| **OpenID for Verifiable Presentations** | [OID4VP](https://openid.net/specs/openid-4-verifiable-presentations-1_0-ID2.html)                                                                | :construction: |
| - Same-Device Flow                  | [Same-Device](https://openid.github.io/OpenID4VP/openid-4-verifiable-presentations-wg-draft.html#name-same-device-flow)                          | :white_check_mark: |
| - Cross-Device Flow                 | [Cross-Device](https://openid.github.io/OpenID4VP/openid-4-verifiable-presentations-wg-draft.html#name-cross-device-flow)                        | :white_check_mark: |                                      | :white_check_mark: |
| - Verifier Authentication           | [Verifier Authentication](https://openid.github.io/OpenID4VP/openid-4-verifiable-presentations-wg-draft.html#name-verifier-metadata-managemen)   | :construction: |
| **Self-Issued OpenID Provider v2**  | [SIOPv2](https://openid.net/specs/openid-connect-self-issued-v2-1_0-ID1.html)                                                                    | :construction: |

## Credential Formats

| Credential Format                   | Link                                                                                                                                      | State              |
| ----------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------- | ------------------ |
| SD-JWT-based Verifiable Credentials | [SD-JWT VC](https://datatracker.ietf.org/doc/html/draft-ietf-oauth-sd-jwt-vc-00)                                                          | :construction:     |
| mdoc (ISO/IEC 18013-5)              | [mdoc](https://www.iso.org/standard/69084.html)                                                                                           | :construction:     |


## Contributing

We are actively developing this framework and welcome contributions from the community. Please read our [CONTRIBUTING](./CONTRIBUTING.md) document to understand our branching strategy, versioning strategy and release workflows before submitting a pull request.

## License

[Apache License Version 2.0](./LICENSE)
