# Versioning and NuGet publishing

Every merged pull request into `develop` or `master` runs `bump-version.yml`.
An open PR only runs build validation. Closing a PR without merging does not
change the version or publish a package. Direct branch pushes also only validate.

`src/Directory.Build.props` stores the shared four-part `AssemblyVersion`:
`Major.Minor.Patch.Build`. Both library projects inherit it, along with FileVersion
and Version. Change the version in this file only; the project files contain no
version overrides. The automated bump commits only `src/Directory.Build.props`.

| PR merged into | Version before | Committed version | NuGet package version |
| --- | --- | --- | --- |
| `develop` | `1.3.3.0` | `1.3.3.1` | `1.3.3.1-pre` |
| `develop` again | `1.3.3.1` | `1.3.3.2` | `1.3.3.2-pre` |
| `master` | `1.3.3.2` | `1.4.0.0` | `1.4.0` (normalized) |

The develop increment preserves Major, Minor, and Patch. The master increment
resets Patch and Build to zero. Patch and Major can be changed manually when
needed. After a release, merge master back into develop to carry the new Minor
version into subsequent prereleases.

The workflow displays versions with a `v` prefix (for example, `v1.3.3.2-pre`).
The prefix is omitted from NuGet package metadata. NuGet supports four-part
versions but normalizes a fourth component of zero away; `1.4.0.0` and `1.4.0`
identify the same package version. See
[NuGet version normalization](https://learn.microsoft.com/en-us/nuget/concepts/package-versioning#normalized-version-numbers).

The version change is committed back to the target branch by
`github-actions[bot]`. Reruns find the existing commit for that PR instead of
incrementing again. Simultaneous merges retry against the latest target branch
without force pushing. The build workflow is explicitly called with the bump
commit SHA, since pushes using `GITHUB_TOKEN` do not trigger push workflows.

The called workflow builds and tests that commit, then uploads its packages and
returns the exact artifact name. A separate `publish-nuget` job in
`bump-version.yml` downloads that artifact and publishes only after the build job
succeeds. NuGet OIDC authentication stays outside the reusable build workflow.
Rerunning a publish skips versions that are already on NuGet.org. This publishes
the `Lite.EventIpc` package; the hosting library remains a project dependency
without its own NuGet package configuration.

## GitHub setup

1. Merge these workflows into the default branch and both target branches. Remove
   the old `bump-develop-version.yml` workflow so it cannot also increment versions.
2. In GitHub **Settings > Secrets and variables > Actions > Variables**, create
   the repository variable `NUGET_USER`. Set it to your NuGet.org username
   (profile name), not your email address or GitHub username.
3. Allow the Actions bot to push version commits to both `develop` and `master`.
   The bump job requests `contents: write`, but repository rules can still reject
   the push. No repository settings or protection rules are changed by these files.
4. Merge a PR into develop and inspect **Version and publish merged PR**. Expect
   one version commit and a package ending in `-pre`.
5. Merge a PR into master. Expect a Minor increment and a stable package.

The merged-PR workflow checks out only the trusted target branch after a merge,
never an unmerged PR head. Ordinary PR validation does not publish or receive
publishing credentials. Only the publishing job has `id-token: write` permission;
it uses `NuGet/login@v1` to exchange GitHub's OIDC token for a short-lived API key.
The existing `NUGET_AUTH_TOKEN` repository secret is no longer used.

## NuGet.org Trusted Publishing setup

On NuGet.org, select your username, **Trusted Publishing**, and add a policy:

| Setting | Value |
| --- | --- |
| Repository owner | `SuessLabs` |
| Repository | `Lite.EventIpc` |
| Workflow file | `bump-version.yml` (filename only) |
| Environment | Leave blank; the publishing job does not use an environment |
| Package scope | `Lite.EventIpc` |
| Permission | Push new package versions |

Choose the NuGet account or organization that owns the package as the policy
owner. `NUGET_USER` identifies the individual NuGet.org user performing the login.
The policy must cover the package and that user's publishing rights.

The same policy handles develop prereleases and master stable releases. NuGet's
temporary keys last one hour and are requested just before publishing. There is
no stored API key to rotate. See the
[NuGet Trusted Publishing guide](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing).

After configuring the policy and `NUGET_USER`, merge the updated workflows and
test a publish. After the first successful Trusted Publishing run, remove the
unused GitHub `NUGET_AUTH_TOKEN` repository secret and revoke the old API key on
NuGet.org if no other automation uses it.

## Publish failures

If NuGet reports `File does not exist (.../artifacts/*.nupkg)` despite successful
packing, check for mixed Windows path separators in the wildcard argument. The
publish step resolves packages with PowerShell `Get-ChildItem` and passes each
file's full path to NuGet, avoiding this wildcard-matching failure.

If **Check Trusted Publishing configuration** fails, configure the `NUGET_USER`
repository variable. If **Authenticate to NuGet.org** fails, check that the NuGet
policy matches the repository, `bump-version.yml` filename, environment setting,
and policy owner. A 401 or 403 during push requires checking the policy's Push
scope and the user's publishing rights for `Lite.EventIpc`.

The publish step checks that the temporary key and packages exist, then prints
NuGet's response with the key redacted. It retains the full-path workaround and
`--skip-duplicate` so reruns do not fail on already-published package versions.

For other failures, inspect the NuGet server response in **Publish to NuGet.org**.
After changing only the policy or repository variable, rerun the failed publishing
job to request a fresh temporary key. Workflow code changes require a new run
using the updated workflow; rerunning an old run uses its original code.
