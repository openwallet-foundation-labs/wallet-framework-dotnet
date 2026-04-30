# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

### [Unreleased]

#### Fixed

- Claim paths that select all elements in an array are preserved correctly when credentials or metadata are serialized.
- SD-JWT presentations include the right disclosures for nested and array-shaped claims, not only simple top-level paths.
- Fixed crash when resolving credential state from a status list: malformed JWT payloads, invalid base64, zlib/deflate errors, and out-of-range indices are handled by returning no state instead of throwing

#### Changed

- SD-JWT holder presentation APIs now take structured claim paths instead of raw path strings.

### [3.0.1] - 2026.02.27

- Fix Database concurrency issues

### [3.0.0] - 2026.02.27

#### Added

- Support for OID4VC Stack
- Support SQLite database

#### Deprecated

- Hyperledger Aries Stack
