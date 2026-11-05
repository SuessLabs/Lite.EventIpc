# Build versioning

Merging a pull request into `develop` runs `bump-develop-version.yml`. It increments
the patch component of `AssemblyVersion` in both library projects and commits the
change to `develop` using `github-actions[bot]`. For example, `1.3.0` becomes
`1.3.1`; the existing file and package version properties inherit that change.
Closing a PR without merging does not increment the version.

Rerunning a workflow finds the existing bump commit for that PR instead of
incrementing again. If another merge advances `develop` during the bump, the
workflow fetches the latest branch, recalculates the version, and retries the
push up to three times without force pushing.

The workflow then calls `build-github.yml` with the bump commit's SHA to build,
test, and upload packages. This explicit call is necessary because pushes made
with `GITHUB_TOKEN` do not trigger another push workflow. See
[GitHub's token documentation](https://docs.github.com/en/actions/concepts/security/github_token).

Package versions are based on the project version:

| Build | Example package version |
| --- | --- |
| PR validation | `1.3.0-alpha.123` |
| `develop` merge, after the committed bump | `1.3.1-beta.124` |
| `master` push | `1.3.1` |

The final number in prerelease versions is the workflow run number. Versions are
passed to both build and pack so the assembly metadata and NuGet package agree.
Only pushes to `master` publish to NuGet.org, using `NUGET_AUTH_TOKEN`.

The bump job requests `contents: write`. Repository policy and branch protection
must allow the Actions bot to push to `develop`; otherwise that job fails with an
instruction to check permissions and rerun. This configuration does not change
repository settings or bypass branch protection. Ensure these workflow files
are available on the default branch as well as `develop` for the
[`pull_request_target` trigger](https://docs.github.com/en/actions/reference/workflows-and-actions/events-that-trigger-workflows#pull_request_target).
