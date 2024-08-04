#if NET8_0_OR_GREATER
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhinobyte.Extensions.Json.Tests;

[TestClass]
public class PolymorphicFallbackTypeResolverTests
{
	public static readonly JsonSerializerOptions OptionsWithPolymorphicFallbackTypeResolver = new JsonSerializerOptions
	{
		Converters =
		{
			new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
		},
		TypeInfoResolver = new PolymorphicFallbackTypeResolver(),
		WriteIndented = true
	};

	[TestMethod]
	public void All_PolymorphicBaseType_Implementations_Should_Be_Serializable_And_Deserializable()
	{
		var polymorphicImplementationTypes = typeof(IPolymorphicBaseType).Assembly.GetTypes()
			.Where(t => t.IsClass && !t.IsAbstract && typeof(IPolymorphicBaseType).IsAssignableFrom(t))
			.ToList();

		var testObjectBuilder = new Fixture();

		foreach (var changeEventType in polymorphicImplementationTypes)
		{
			var changeEventInstance = (IPolymorphicBaseType)testObjectBuilder.CreateByType(changeEventType);
			var jsonString = JsonSerializer.Serialize<IPolymorphicBaseType>(changeEventInstance, OptionsWithPolymorphicFallbackTypeResolver);

			// Deserializing as the base interface type should correctly resolve to the actual type
			var deserializedInstance = JsonSerializer.Deserialize<IPolymorphicBaseType>(jsonString, OptionsWithPolymorphicFallbackTypeResolver);

			deserializedInstance.Should().NotBeNull().And.BeOfType(changeEventType);
		}
	}

	[TestMethod]
	public void DeserializingInterface_Using_PolymorphicFallbackTypeResolver_Should_Deserialize_To_Fallback_Type_When_An_Unrecognized_Discrimnator_Is_Encountered()
	{
		//lang=json
		var changeEventJson = @"{ ""$type"": ""UnrecognizedEventType"", ""ChangedTimestamp"": ""2021-09-01T00:00:00Z"", ""EventType"": ""UnrecognizedEventType"" }";

		var deserializedInstance = JsonSerializer.Deserialize<IPolymorphicBaseType>(changeEventJson, OptionsWithPolymorphicFallbackTypeResolver);

		deserializedInstance.Should().NotBeNull().And.BeOfType<PolymorphicFallbackType>();

		var fallbackInstance = (PolymorphicFallbackType)deserializedInstance!;
		fallbackInstance.ChangedTimestamp.Should().Be(DateTimeOffset.Parse("2021-09-01T00:00:00Z"));
		fallbackInstance.EventType.Should().Be("UnrecognizedEventType");

	}

	[JsonDerivedType(typeof(PolymorphicTypeA), nameof(PolymorphicTypeA))]
	[JsonDerivedType(typeof(PolymorphicTypeB), nameof(PolymorphicTypeB))]
	[JsonDerivedType(typeof(PolymorphicFallbackType), nameof(PolymorphicFallbackType))]
	[JsonPolymorphicDeserializationFallback(typeof(PolymorphicFallbackType))]
	[JsonPolymorphic(IgnoreUnrecognizedTypeDiscriminators = true, UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization)]
	public interface IPolymorphicBaseType
	{
		DateTimeOffset ChangedTimestamp { get; set; }
		string EventType { get; set; }
	}

	public class PolymorphicFallbackType : IPolymorphicBaseType
	{
		public DateTimeOffset ChangedTimestamp { get; set; }
		public required string EventType { get; set; }
	}

	public class PolymorphicTypeA : IPolymorphicBaseType
	{
		public DateTimeOffset ChangedTimestamp { get; set; }
		public required string EventType { get; set; }
		public required string Title { get; set; }
	}

	public class PolymorphicTypeB : IPolymorphicBaseType
	{
		public DateTimeOffset ChangedTimestamp { get; set; }
		public required string Description { get; set; }
		public required string EventType { get; set; }
	}
}

public static class AutoFixtureExtensions
{
	/// <summary>
	/// Auto-fixture already has a method for this but it's internal only ='(
	/// </summary>
	public static object CreateByType(this Fixture testObjectBuilder, Type type) => new SpecimenContext(testObjectBuilder).Resolve(type);
}
#endif
