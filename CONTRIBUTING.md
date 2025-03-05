# Contributing to NuGetPackageChecker

All contributions are encouraged and valued. Please read the [Table of Contents](#table-of-contents) before contributing to help maintainers and improve the experience for everyone. The community looks forward to your contributions.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Issues](#issues)
- [Pull Requests](#pull-requests)
- [Coding Guidelines](#coding-guidelines)
- [Commit Messages](#commit-messages)
- [Community Involvement](#community-involvement)

## Code of Conduct

This project and everyone participating in it is governed by the [Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code. Please report unacceptable behavior to the project maintainers.

## Issues

Please raise bug reports and feature requests as Issues on [the GitHub project](https://github.com/G-Research/NuGetPackageChecker/issues).

If you find a bug or a potential issue:

1. Make sure you are using the latest version of the package.
2. Check the existing [Issues](https://github.com/G-Research/NuGetPackageChecker/issues) to see if it has already been reported.
3. If no existing issue matches, [open a new issue](https://github.com/G-Research/NuGetPackageChecker/issues/new) and provide:
   - A clear and descriptive title.
   - Steps to reproduce the issue.
   - Expected vs actual behavior.
   - Relevant logs, error messages, or screenshots.
   - Your environment details (OS, .NET version, NuGet version, etc.).

## Pull Requests

Before embarking on a large change, we strongly recommend checking via a GitHub Issue first to confirm whether we are likely to accept it.

You may find that the following guidelines will help you produce a change that we accept:

* Keep your change as small and focused as practical.
* Ensure that your change is thoroughly tested.
* Document any choices you make that are not immediately obvious.
* Explain why your change is necessary or desirable.

### Submitting a Pull Request

#### Steps to Submit a PR

1. Fork the repository and create a new branch from `main`.
2. Implement your changes, following the [Coding Guidelines](#coding-guidelines).
3. Ensure your changes pass all tests.
4. Commit your changes with a meaningful commit message (see [Commit Messages](#commit-messages)).
5. Push your branch and open a pull request.
6. The maintainers will review your PR and provide feedback.

## Coding Guidelines

- Follow the existing coding style and project conventions.
- Keep changes focused; avoid unrelated modifications.
- Ensure all tests pass before submitting a PR.
- Write descriptive commit messages.
- Keep PRs small and easy to review.

## Commit Messages

A good commit message should:

- Start with a concise summary in **50 characters or less**.
- Provide additional details in the body if necessary.
- Reference relevant issues using `Fixes #issue-number` when applicable.
- Use imperative language (e.g., "Add support for XYZ" instead of "Added support for XYZ").

Example:

```
Fix validation error handling in package checker

- Improve error reporting for missing package versions
- Add unit tests for version validation logic

Fixes #42
```

## Community Involvement

Want to be more involved?

- Help review and triage issues.
- Assist new contributors by answering questions.
- Share your experience using NuGetPackageChecker.
- Contact the maintainers to discuss the project.

Thank you for helping make this project better!

