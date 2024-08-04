#if NET8_0_OR_GREATER
using System;

namespace Rhinobyte.Extensions.Json;

/// <summary>
/// It is advantageous to be able to use an interface as the polymorphic base type for serialization so
/// that implementing classes can subclass something else to avoid needing to duplicate properties which then have to be kept in sync.
/// <br />
/// However when deserializing to an interface base type, the deserializer can't fallback to the base type if it encounters an unrecognized type discriminator value.
/// <br />
/// This attribute allows us to specify a fallback type to use in such scenarios. The <see cref="PolymorphicFallbackTypeResolver"/> must be included in the serialization options
/// for deserialization to the fallback type to work.
/// </summary>
[AttributeUsage(AttributeTargets.Interface)]
public sealed class JsonPolymorphicDeserializationFallbackAttribute : Attribute
{
	/// <summary>
	/// The type to use as a fallback when deserializing using an interface base type and an unrecognized type discriminator value is encountered.
	/// </summary>
	public Type FallbackType { get; }

	/// <summary>
	/// Use the provided <paramref name="fallbackType"/> as the fallback type when deserializing using an interface base type and an unrecognized type discriminator value is encountered.
	/// </summary>
	public JsonPolymorphicDeserializationFallbackAttribute(Type fallbackType)
	{
		FallbackType = fallbackType;
	}
}
#endif
