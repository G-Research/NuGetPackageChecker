# NuGetPackageChecker

![Build Status](https://github.com/G-Research/NuGetPackageChecker/actions/workflows/ci.yaml/badge.svg)

<img src="./Monitor.png" width="300px" />

An MSBuild extension to check for required packages and versions.

## How does it work?

Validates NuGet package references and versions for projects in the footprint. This works by adding an additional .targets file to the build process which checks the contents of package references.

Warnings and errors are written to the standard build output and any validation failures will fail the build.

## Why?

There are often times when it is necessary to ensure the inclusion of certain packages or exclusion of others.

The most obvious examples currently are test projects which rely on additional packages, and the extensions contained within them, to augment the build process. Ensuring that test projects include the correct references with the correct versions is essential in order to be able to run the tests as part of CI pipelines or gather code coverage.

The requirement for package references and controlling versions is however not isolated to test projects and being able to ensure references or prevent the use of incompatible versions of packages is essential in avoiding issues rather than blindly permitting the use of any later versions.

This package provides a means of controlling required references and versions to ensure compatibility.

## Usage

Add a reference to the NuGet package which will add a build-only dependency to your project. The easiest way to do this for a large number of projects is to include a Directory.Build.props file at the root of the repo containing:

```xml
<Project>
  <ItemGroup>
    <PackageReference Include="NuGetPackageChecker" Version="0.1.2" PrivateAssets="All"/>
  </ItemGroup>
</Project>
```

To automatically import the NuGet package into all projects.

Required or forbidden package references are configured using item groups which can also be added to the Directory.Build.props files but can be updated/overridden as required.

```xml
<Project>
  <ItemGroup>
    <RequiredPackages Include="SecureBuildProcess" MinVersion="0.1" />
    <ForbiddenPackages Include="DodgyPackage" />
  </ItemGroup>
</Project>
```

## Security

Please see our [security policy](https://github.com/G-Research/NuGetPackageChecker/blob/main/SECURITY.md) for details on reporting security vulnerabilities.

## Contributing

We welcome contributions! To get started, read the [CONTRIBUTING.md](CONTRIBUTING.md) and the [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) files to see how best to contribute to the project.

## Issue Reporting

If you encounter any issues or bugs, please report them via the [GitHub Issues](https://github.com/G-Research/NuGetPackageChecker/issues) page. Provide as much detail as possible, including:
- Steps to reproduce the issue.
- Expected behavior.
- Actual behavior.
- Relevant logs or screenshots.

## License

This project is licensed under the [Apache 2.0 License](LICENSE).

Thank you for using and contributing to NuGetPackageChecker!

