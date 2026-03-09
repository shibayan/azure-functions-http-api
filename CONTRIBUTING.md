# Contributing to HTTP API Extensions for Azure Functions

Thank you for your interest in contributing! This document provides guidelines and information for contributors.

## How to Contribute

### Reporting Bugs

If you find a bug, please open an [issue](https://github.com/shibayan/azure-functions-http-api/issues/new?template=bug_report.md) with a clear description of the problem. Include:

- Steps to reproduce the issue
- Expected and actual behavior
- .NET version and Azure Functions runtime version
- Package version (`WebJobs.Extensions.HttpApi` or `Functions.Worker.Extensions.HttpApi`)

### Suggesting Features

Feature requests are welcome! Please open an [issue](https://github.com/shibayan/azure-functions-http-api/issues/new?template=feature_request.md) describing the feature you'd like to see, why you need it, and how it should work.

### Submitting Pull Requests

1. Fork the repository
2. Create a new branch from `master` (`git checkout -b feature/my-feature`)
3. Make your changes
4. Ensure the project builds successfully (`dotnet build -c Release`)
5. Ensure code formatting is correct (`dotnet format --verify-no-changes`)
6. Commit your changes with a clear commit message
7. Push to your fork and open a Pull Request

## Development Setup

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download) or later
- An IDE such as Visual Studio, Visual Studio Code, or JetBrains Rider

### Building

```
dotnet build -c Release
```

### Code Style

This project uses `.editorconfig` for code style enforcement and `dotnet format` for linting. Please ensure your changes pass formatting checks before submitting a PR:

```
dotnet format --verify-no-changes --verbosity detailed
```

## Code of Conduct

This project has adopted the [Contributor Covenant Code of Conduct](CODE_OF_CONDUCT.md). By participating, you are expected to uphold this code.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
