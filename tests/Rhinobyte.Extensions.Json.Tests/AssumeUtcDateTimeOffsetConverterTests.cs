using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhinobyte.Extensions.Json.Tests;

[TestClass]
public class AssumeUtcDateTimeOffsetConverterTests
{
	public static readonly JsonSerializerOptions DefaultSerializerOptions = new JsonSerializerOptions
	{

	};

	[TestMethod]
	public void AssumeUtcDateTimeOffsetConverter_Should_Preserve_The_Non_Utc_TimeZoneOffset_When_Specified_In_The_Json_Value()
	{
		//lang=json
		var testJsonWithTimezoneInfo = @"{""CurrentTimestampUtc"":""2024-07-27T00:00:00.0000000-05:00""}";
		var deserializedItem = JsonSerializer.Deserialize<TestDeserializedItem>(testJsonWithTimezoneInfo, DefaultSerializerOptions);

		var expectedTimestamp = new DateTimeOffset(2024, 7, 27, 0, 0, 0, TimeSpan.FromHours(-5));
		deserializedItem!.CurrentTimestampUtc.Offset.Should().Be(expectedTimestamp.Offset);
		deserializedItem.CurrentTimestampUtc.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromMilliseconds(1));
	}

	[TestMethod]
	public void AssumeUtcDateTimeOffsetConverter_Should_Preserve_The_Utc_TimeZoneOffset_When_Specified_With_The_Z_Suffix()
	{
		//lang=json
		var testJsonWithUtcZSuffix = @"{""CurrentTimestampUtc"":""2024-07-27T00:00:00.0000000Z""}";
		var deserializedItem = JsonSerializer.Deserialize<TestDeserializedItem>(testJsonWithUtcZSuffix, DefaultSerializerOptions);

		deserializedItem!.CurrentTimestampUtc.Offset.Should().Be(TimeSpan.Zero);

		var expectedTimestamp = new DateTimeOffset(2024, 7, 27, 0, 0, 0, TimeSpan.Zero);
		deserializedItem.CurrentTimestampUtc.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromMilliseconds(1));
	}

	[TestMethod]
	public void AssumeUtcDateTimeOffsetConverter_Should_Return_UtcOffset_When_The_JsonTimezone_Is_Unspecified()
	{
		//lang=json
		var testJsonWithNoTimezoneInfo = @"{""CurrentTimestampUtc"":""2024-07-27T00:00:00.0000000""}";
		var deserializedItem = JsonSerializer.Deserialize<TestDeserializedItem>(testJsonWithNoTimezoneInfo, DefaultSerializerOptions);

		deserializedItem!.CurrentTimestampUtc.Offset.Should().Be(TimeSpan.Zero);

		var expectedTimestamp = new DateTimeOffset(2024, 7, 27, 0, 0, 0, TimeSpan.Zero);
		deserializedItem.CurrentTimestampUtc.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromMilliseconds(1));
	}

	[TestMethod]
	public void AssumeUtcDateTimeOffsetConverter_Should_Return_UtcOffset_When_The_JsonTimezone_Is_Unspecified_And_Using_The_Universal_Sortable_Format()
	{
		// Verify we can parse the date (without timezone info) when using the 'universal sortable' format

		//lang=json
		var testJsonWithNoTimezoneInfo = @"{""CurrentTimestampUtc"":""2024-07-27 00:00:00""}";
		var deserializedItem = JsonSerializer.Deserialize<TestDeserializedItem>(testJsonWithNoTimezoneInfo, DefaultSerializerOptions);

		deserializedItem!.CurrentTimestampUtc.Offset.Should().Be(TimeSpan.Zero);

		var expectedTimestamp = new DateTimeOffset(2024, 7, 27, 0, 0, 0, TimeSpan.Zero);
		deserializedItem.CurrentTimestampUtc.Should().BeCloseTo(expectedTimestamp, TimeSpan.FromMilliseconds(1));
	}

	public class TestDeserializedItem
	{
		[JsonConverter(typeof(AssumeUtcDateTimeOffsetConverter))]
		public DateTimeOffset CurrentTimestampUtc { get; set; }
	}
}
