

# Rhinobyte.Extensions

This repo contains the code to build the Rhinobyte extension libraries for .NET

## Libraries/Projects

### [Rhinobyte.Extensions.DataAnnotations](/src/Rhinobyte.Extensions.DataAnnotations/README.md)

[![NuGet version (Rhinobyte.Extensions.DataAnnotations)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.DataAnnotations.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.DataAnnotations/)

This library contains types to extend the behavior provided by the `System.ComponentModel.DataAnnotations` types.

### [Rhinobyte.Extensions.DependencyInjection](/src/Rhinobyte.Extensions.DependencyInjection/README.md)

[![NuGet version (Rhinobyte.Extensions.DependencyInjection)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.DependencyInjection.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.DependencyInjection/)

This library contains assembly scanning and convention based registration extensions for the out-of-the-box .NET dependency injection libraries.

### [Rhinobyte.Extensions.Json](/src/Rhinobyte.Extensions.Json/README.md)

[![NuGet version (Rhinobyte.Extensions.Json)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.Json.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.Json/)

This library contains extensions to the `System.Text.Json` base class library functionality.

### [Rhinobyte.Extensions.Reflection](/src/Rhinobyte.Extensions.Reflection/README.md)

[![NuGet version (Rhinobyte.Extensions.Reflection)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.Reflection.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.Reflection/)

This library contains extensions for .Net reflection including assembly scanning/type discovery extensions and support for parsing IL (intermediate-language) instructions.

### [Rhinobyte.Extensions.TestTools](/src/Rhinobyte.Extensions.TestTools/README.md)

[![NuGet version (Rhinobyte.Extensions.TestTools)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.TestTools.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.TestTools/)

This library contains general purpose testing helper/extension types and extensions for `MSTest`

## Contributing

Filing issues for problems encountered is greatly appreciated and contribution via PRs is welcomed.

Code changes that modify a library's public API signature will require a new major version number. For any such changes, please create an issue for discussion first and include the label `api-suggestion`. Please do not submit pull requests that include style changes.

Maintaining a very high percentage of test coverage over these libraries will be an important requirement when reviewing pull requests. The `devops/create-code-coverage-report` shell script can be run to execute the tests with code coverage enabled and to generate an html version of the coverage results report. When making a code contribution please ensure you update all existing tests as necessary and add new test cases to cover new code.

## License

This repository is licensed under the [MIT](LICENSE.txt) license.
