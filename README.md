
# DEPRECATION MESSAGE

[Deprecated] Will be renaming package to `Rhinobyte.Extensions.DataAnnotations`
Moving repo/project to https://github.com/RhinobyteSoftware/dotnet-extensions

I will be deleting this repo shortly as well as I don't believe many/anyone is using this package yet/repo for anything as of yet


### Rhinobyte.Validation

Helper libraries for validation.

## Rhinobyte.DataAnnotations

[![NuGet version (Rhinobyte.DataAnnotations)](https://img.shields.io/nuget/v/Rhinobyte.DataAnnotations.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.DataAnnotations/)

This library contains the code to build the Rhinobyte.DataAnnotations library for .Net.
It contains types to extend the behavior provided by the `System.ComponentModel.DataAnnotations` types.

## DateTime Validation Attributes

This library was started primarily to provide a thread safe alternative to using the `[Range]` attribute for
`DateTime` properties. I was frustrated that the maintainers chose not to fix the [thread safety issue](https://github.com/dotnet/runtime/issues/1143),
despite an easy non-contention solution being possible.

Example Usage:
```cs
using Rhinobyte.DataAnnotations;
using System;

namespace SomeNamespace
{
    public class SomeModel
    {
        [SqlServerDate]
        public DateTime? Property1 { get; set; }

        [SqlServerDateTime]
        public DateTime? Property2 { get; set; }

        [SqlServerSmallDateTime]
        public DateTime? Property3 { get; set; }

        [DateTimeRange("1980-01-01", "1999-12-31 23:59:59")]
        public DateTime? Property4 { get; set; }
    }
}

```

## License

This repository is licensed under the [MIT](LICENSE) license.