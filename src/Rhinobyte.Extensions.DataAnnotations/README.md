
# Rhinobyte.Extensions.DataAnnotations

[![NuGet version (Rhinobyte.Extensions.DataAnnotations)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.DataAnnotations.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.DataAnnotations/)


## DateTime Validation Attributes

This library was started primarily to provide a thread safe alternative to using the `[Range]` attribute for
`DateTime` properties. I was frustrated that the maintainers chose not to fix the [thread safety issue](https://github.com/dotnet/runtime/issues/1143),
despite an easy non-contention solution being possible.

Example Usage:
```cs
using Rhinobyte.Extensions.DataAnnotations;
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