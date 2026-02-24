---
name: test-automation
description: Guidelines and best practices for authoring automated tests (including unit tests).
---

### Unit Tests

- For .NET source projects, you should ideally author unit tests using XUnit, Moq, FluentAssertions and Bogus.
  - For FluentAssertion, please use the latest, stable `7.2.x` version. Do not attempt to use the `8.x` or later versions.
  - Some references for writing good unit tests in .NET:
    - [Unit testing best practices for .NET](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)
