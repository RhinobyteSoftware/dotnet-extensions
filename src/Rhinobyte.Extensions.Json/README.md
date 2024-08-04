# Rhinobyte.Extensions.Json

[![NuGet version (Rhinobyte.Extensions.Json)](https://img.shields.io/nuget/v/Rhinobyte.Extensions.Json.svg?style=flat)](https://www.nuget.org/packages/Rhinobyte.Extensions.Json/)

This library contains extensions for the .NET System.Text.Json library. The extensions provide additional features and convenience methods for working with JSON data.


## AssumeUtcDateTimeOffsetConverter

A custom JsonConverter for DateTimeOffset that assumes a fallback of UTC Timezone information should be used if the JSON token does not include time zone details.

The default converter currently always assumes the system local time zone should be used if no time zone information is provided in the JSON token. This can lead to incorrect date time values being parsed if the JSON token was serialized with a different time zone than the system local time zone.

## PolymorphicFallbackTypeResolver (.NET 8.0+)

A customized type info resolver that will check for a specified fallback type via a JsonPolymorphicDeserializationFallbackAttribute when deserializing polymorphic JSON using an interface for the polymorphic base type.

Example usage:
```cs
    public static readonly JsonSerializerOptions OptionsWithPolymorphicFallbackTypeResolver = new JsonSerializerOptions
    {
        TypeInfoResolver = new PolymorphicFallbackTypeResolver()
    };
```

See the [PolymorphicFallbackTypeResolverTests](./Rhinobyte.Extensions.Json.Tests/PolymorphicFallbackTypeResolverTests.cs) for additional examples on how to use this feature.