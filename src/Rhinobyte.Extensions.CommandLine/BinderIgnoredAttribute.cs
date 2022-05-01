using System;

namespace Rhinobyte.Extensions.CommandLine;

/// <summary>
/// Attribute used to decorate model properties that should be ignored by the advanced parser/binder
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class BinderIgnoredAttribute : Attribute
{
}
