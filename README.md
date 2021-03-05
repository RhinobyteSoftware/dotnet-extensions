
# Rhinobyte.DataAnnotationsValidation

[![NuGet version (Rhinobyte.DataAnnotations)](https://img.shields.io/nuget/v/Rhinobyte.DataAnnotations.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.DataAnnotations/)

This repo contains the code to build the Rhinobyte.DataAnnotations library for .Net

This library was started primarily to provide a thread safe alternative to using the `[Range]` attribute for
`DateTime` properties. I was frustrated that the Microsoft maintainers chose not to fix the [thread safety issue](https://github.com/dotnet/runtime/issues/1143),
despite an easy non-contention solution being available.

## License

This repo is licensed under the [MIT](LICENSE) license.