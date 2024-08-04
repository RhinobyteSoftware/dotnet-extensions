#if NET8_0_OR_GREATER
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Rhinobyte.Extensions.Json;

/// <summary>
/// When deserializing a polymorphic type to an interface base type, this resolve allows the deserializer to create an instance of a specified fallback type
/// if it encounters an unrecognized type discriminator value.
/// <br />
/// The interface must be decorated with the <see cref="JsonPolymorphicDeserializationFallbackAttribute"/> to specify the fallback type to use.
/// </summary>
public class PolymorphicFallbackTypeResolver : DefaultJsonTypeInfoResolver
{
	/// <inheritdoc />
	public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
	{
		var jsonTypeInfo = base.GetTypeInfo(type, options);

#pragma warning disable CA1062 // Validate arguments of public methods
		if (jsonTypeInfo.CreateObject is null && type.IsInterface)
		{
			var fallbackType = type.GetCustomAttribute<JsonPolymorphicDeserializationFallbackAttribute>()?.FallbackType;
			if (fallbackType is not null && type.IsAssignableFrom(fallbackType))
			{
				var fallbackTypeInfo = base.GetTypeInfo(fallbackType, options);
				jsonTypeInfo.CreateObject = fallbackTypeInfo.CreateObject;
			}
		}
#pragma warning restore CA1062 // Validate arguments of public methods

		return jsonTypeInfo;
	}
}
#endif
