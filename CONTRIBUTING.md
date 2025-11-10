# Contributing to Wallet Framework for .NET

Thank you for your interest in contributing! This guide explains how to propose changes and what maintainers expect in pull requests.

## Ways to contribute
- Report bugs and request features via GitHub Issues.
- Improve docs: README.md, samples, and code comments.
- Submit code changes via Pull Requests (PRs).

## Development setup
- Prerequisites: .NET SDK (net8.0)
- Build: `dotnet build`
- Run tests: `dotnet test`

## Before you start
- For larger work, open an Issue/Discussion to align on scope.
- For small fixes or features, you can go straight to a PR.

## Coding standards
- C#: follow standard .NET conventions. Keep code clear and well-documented.
- We follow a functional programming approach using [LanguageExt](https://github.com/pasoft-share/language-ext/tree/master?tab=readme-ov-file): prefer immutability, pure functions, and expression-oriented code.
- Add/update unit tests for your changes; avoid breaking public APIs unless coordinated.

## Commits and PRs
- If applicable, always link the related Issue in the PR description (e.g., "Fixes #123").
- Every PR MUST update [CHANGELOG.md](./CHANGELOG.md) under the `[Unreleased]` section, using one of the categories: Added, Changed, Deprecated, Removed, Fixed, Security. Use clear, user-facing language and link to the PR/Issue.
- Sign your commits and include a sign-off: `git commit -S --signoff`.
- Keep PRs focused and reasonably small.
- Target branch:
  - Features/regular (bug)fixes → `develop`.
  - Hotfixes for production → `hotfix/vX.Y.Z` branched from `main` (Maintainer responsibility).

## Review and merge
- CI must pass (build + tests).
- At least one maintainer approval (four-eye principle) is required.

## Security
Do NOT open public issues for vulnerabilities. Please contact the maintainers privately with details so we can coordinate a fix and disclosure.
