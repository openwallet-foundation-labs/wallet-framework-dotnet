# Software Development Lifecycle (SDLC)

This document outlines the SDLC for the Wallet Framework .NET, including the branching strategy, versioning, release process, and quality assurance measures.

---

## Branching Strategy

We follow a GitFlow-inspired model, which uses two primary, long-lived branches and several supporting, short-lived branches to manage development and releases effectively:

**Primary Branches**
* **`main`**: This branch represents the latest **production-ready, stable release**. It is updated only from `release` or `hotfix` branches. All commits on `main` are tagged with a version number.
* **`develop`**: This is the primary **integration branch** for active development. All new `feature` branches are based on `develop` and merged back into it via Pull Requests (PRs). This branch always reflects the state of the *next* planned release.


**Supporting Branches**
* **`feature/<name-or-ticket>`**: Feature branches are used for all new development (features, improvements, non-critical fixes).
    * Branched from: `develop`
    * Merged into: `develop` (via Pull Request)
* **`release/vX.Y.Z`**: When the `develop` branch is ready for a new release, a `release` branch is created. This branch is used for final testing, stabilization, and bug fixing. No new features are added here.
    * Branched from: `develop`
    * Merged into: `main` (for release) and `develop` (to port back any fixes).
* **`hotfix/vX.Y.Z`**: Used for addressing critical, high-priority bugs discovered in the `main` (production) release.
    * Branched from: `main`
    * Merged into: `main` (for immediate patch release) and `develop` (to ensure the fix is in future releases).

---

## Versioning Strategy

We adhere to **Semantic Versioning 2.0.0** ([SemVer](https://semver.org/)). All release tags follow the `vMAJOR.MINOR.PATCH` format.

* **`MAJOR`** version increments for incompatible API changes.
* **`MINOR`** version increments for adding functionality in a backward-compatible manner.
* **`PATCH`** version increments for backward-compatible bug fixes.

---

## Changelog Maintenance

We follow the [Keep a Changelog](https://keepachangelog.com/en/1.0.0/) format. The [`CHANGELOG.md`](./CHANGELOG.md) file is a curated, chronological list of notable changes.

* **Developer Responsibility:** Every Pull Request (for features, bugfixes, or hotfixes) **must** include an update to the [`CHANGELOG.md`](./CHANGELOG.md) file.
* **Process:** Add a new line item under the `[Unreleased]` section. The item should be a clear, user-facing description of the change, and it should link to the PR or Issue.
* **Categories:** Group your change under one of the following headings: `Added`, `Changed`, `Deprecated`, `Removed`, `Fixed`, `Security`.
* **Release Process:** During the "Release Process", described in the following section, the `[Unreleased]` section will be renamed to the new version and given a release date (e.g., `[1.1.0] - 2025-11-03`). A new, empty `[Unreleased]` section will be added above it.
---

## Release Process

This process is managed by the project maintainers.
A standard release is triggered when the `develop` branch has accumulated enough features for a new `MINOR` release (or `MAJOR` if breaking changes are included), is stable, and has been approved by the maintainer team.

### Public Releases

We publish two types of releases:

* **Production Releases:** These are stable, official releases intended for production use. They follow the standard `vMAJOR.MINOR.PATCH` format (e.g., `v1.2.0`).
* **Pre-releases:** These are built from a `release` branch for final testing and validation before a full production release. They are marked with a `-rc` (release candidate) suffix, such as `v1.2.0-rc.1`. These are not intended for production environments.
* **Feature-releases:** These are feature builds and not intended for production environments. They are created whenever a Pull Request is successfully merged into the `develop` branch.


### Standard Release (MINOR or MAJOR)

1.  **Ensure `develop` is Ready:** Confirm that all features and fixes for the new release are merged into `develop` and all CIs are passing.
    ```bash
    git fetch origin
    ```
2.  **Create Release Branch:**
    ```bash
    # Branch from develop
    git checkout -b release/X.Y.0 origin/develop
    ```
3.  **Prepare the Release** (on the `release` branch):
    * Run final integration and regression tests.
    * Update the [`CHANGELOG.md`](./CHANGELOG.md) file:
        * Rename the `[Unreleased]` section to `[X.Y.0] - YYYY-MM-DD`.
        * Add a new `[Unreleased]` section at the top.
    * Bump the version number (`<Version>`) in the [Directory.Build.props](Directory.Build.props) file.
    * Make a final properly signed "Version bump" commit:
    ```bash
    git commit -S --signoff -m "Bump version to X.Y.0"
    ```
4.  **Merge into `main` when the release is ready:**
    * Open a PR to merge `release/X.Y.0` into `main`.
    * Get a final review and merge it. **Use a merge commit, not squash.**
    * Make sure that the release branch is up-to-date and that the commit is properly signed.
    ```bash
    git checkout main
    git merge --no-ff -S --signoff release/X.Y.0
    ```
5.  **Tag the Release:**
    ```bash
    git tag -a vX.Y.0 -m "Release X.Y.0"
    git push origin main --tags
    ```
6.  **Merge back into `develop`:**
    * This is critical to ensure the "Version bump" and "Changelog" commits get back into the development line.
    * Make sure that the release branch is up-to-date and that the commit is properly signed.
    ```bash
    git checkout develop
    git merge --no-ff -S --signoff release/X.Y.0
    git push origin develop
    ```
7.  **Delete the Release Branch:**
    ```bash
    git branch -d release/X.Y.0
    git push origin --delete release/X.Y.0
    ```
8.  **Publish:**
    * The GitHub Actions pipeline, automatically trigger on merges towards release and main; builds the release assets, and publishes the official package to NuGet.
    * Go to the GitHub "Releases" page and create a new release from the tag, pasting the changelog notes for this version into the description.

### Hotfix Release (PATCH)

1.  **Create Hotfix Branch from `main`:**
    * Make sure that main is up-to-date
    ```bash
    git checkout -b hotfix/X.Y.1 main
    ```
2.  **Fix the Bug:**
    * Make the necessary code changes.
    * Update the `CHANGELOG.md` under a new `[Unreleased]` section (or a temporary `[X.Y.1]` section).
    * Bump the version number (`<Version>`) in the [Directory.Build.props](Directory.Build.props) file.
    * Make sure that the commit is properly signed.
    ```bash
    git commit -S --signoff -m "Hotfix: <Describe the HotFix>. Bump version to X.Y.1"
    ```
3.  **Merge into `main`:**
    * Open a PR and get a final review before merging the hotfix.
    * Make sure that the commit is properly signed.
    ```bash
    git checkout main
    git merge --no-ff -S --signoff hotfix/X.Y.1
    ```
4.  **Tag the Release:**
    ```bash
    git tag -a vX.Y.1 -m "Hotfix X.Y.1"
    git push origin main --tags
    ```
5.  **Merge back into `develop`:**
    * This is critical to ensure the hotfix is not lost in the next release.
    * Make sure that the commit is properly signed.
    ```bash
    git checkout develop
    git merge --no-ff -S --signoff hotfix/X.Y.1
    git push origin develop
    ```
6.  **Delete the Hotfix Branch:**
    ```bash
    git branch -d hotfix/X.Y.1
    git push origin --delete hotfix/X.Y.1
    ```
7.  **Publish:** The merge towards main will trigger the release pipeline, and a GitHub Release should be created with the hotfix changelog.
---

## Continuous Integration (CI) & Quality

Our CI pipeline is configured to run on every Pull Request targeting the `develop` or `main` branches. The following checks **must** pass before a PR can be merged:

1.  **Build:** The project must compile successfully.
2.  **Automated Tests:** All unit and integration tests must pass.
3.  **Peer Review (4-Eye Principle):** All Pull Requests must be reviewed and approved by **at least one** other team member (other than the author) before they can be merged. This ensures a "four-eye principle" is applied to all code changes.


TODO:
- Add changelog box to PR template
