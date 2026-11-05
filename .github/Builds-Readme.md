# Versioning and NuGet publishing

Every merged pull request into `develop` or `master` runs `bump-version.yml`.
An open PR only runs build validation. Closing a PR without merging does not
change the version or publish a package. Direct branch pushes also only validate.

Both library projects store the same four-part `AssemblyVersion`:
`Major.Minor.Patch.Build`. FileVersion and Version inherit that value.

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

The called workflow builds and tests that commit before packaging and publishing.
Rerunning a publish skips versions that are already on NuGet.org. This publishes
the `Lite.EventIpc` package; the hosting library remains a project dependency
without its own NuGet package configuration.

## GitHub setup

1. Merge these workflows into the default branch and both target branches. Remove
   the old `bump-develop-version.yml` workflow so it cannot also increment versions.
2. Add or update the repository Actions secret `NUGET_AUTH_TOKEN` with a NuGet.org
   API key scoped to Push for `Lite.EventIpc`.
3. Allow the Actions bot to push version commits to both `develop` and `master`.
   The bump job requests `contents: write`, but repository rules can still reject
   the push. No repository settings or protection rules are changed by these files.
4. Merge a PR into develop and inspect **Version and publish merged PR**. Expect
   one version commit and a package ending in `-pre`.
5. Merge a PR into master. Expect a Minor increment and a stable package.

The merged-PR workflow checks out only the trusted target branch after a merge,
never an unmerged PR head. The publishing job receives the NuGet secret through
`secrets: inherit`; ordinary PR validation does not publish.
