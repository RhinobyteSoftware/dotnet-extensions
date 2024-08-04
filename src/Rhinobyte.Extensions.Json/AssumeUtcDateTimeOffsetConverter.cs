using System;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rhinobyte.Extensions.Json;

/// <summary>
/// A customized <see cref="JsonConverter{T}"/> for <see cref="DateTimeOffset"/> values that assumes the timezone information should default to UTC if not provided in the JSON token.
/// </summary>
/// <remarks>
/// The default <see cref="System.Text.Json.Serialization.JsonConverter{T}"/> for <see cref="DateTimeOffset"/> values currently always assumes the timezone information
/// should be set to the local system timezone if not provided in the JSON token.
/// </remarks>
public sealed class AssumeUtcDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
	/// <summary>
	/// <see href="https://github.com/dotnet/runtime/blob/f6a7ebbb81540401e6b5520afa3ba87c2bd6bcfe/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L91">Copy of value from dotnet runtime JsonConstants</see>
	/// </summary>
	public const int MaximumFormatDateTimeLength = 27;    // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000

	/// <summary>
	/// <see href="https://github.com/dotnet/runtime/blob/f6a7ebbb81540401e6b5520afa3ba87c2bd6bcfe/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L92">Copy of value from dotnet runtime JsonConstants</see>
	/// </summary>
	public const int MaximumFormatDateTimeOffsetLength = 33;  // StandardFormat 'O', e.g. 2017-06-12T05:30:45.7680000-07:00

	/// <summary>
	/// <see href="https://github.com/dotnet/runtime/blob/f6a7ebbb81540401e6b5520afa3ba87c2bd6bcfe/src/libraries/System.Text.Json/src/System/Text/Json/Writer/JsonWriterHelper.Date.cs#L13">Copy of value from dotnet runtime JsonWriterHelper</see>
	/// </summary>
	private static readonly StandardFormat s_dateTimeStandardFormat = new('O');

	/// <inheritdoc />
	public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
#if NET8_0_OR_GREATER
		Span<char> charBuffer = stackalloc char[MaximumFormatDateTimeOffsetLength];
		var bytesWritten = reader.CopyString(charBuffer);
		ReadOnlySpan<char> source = charBuffer.Slice(0, bytesWritten);

		var value = DateTimeOffset.Parse(source, null, System.Globalization.DateTimeStyles.AssumeUniversal);
#else
		var rawValue = reader.GetString() ?? string.Empty;
		var value = DateTimeOffset.Parse(rawValue, null, System.Globalization.DateTimeStyles.AssumeUniversal);
#endif

		return value;
	}

	/// <inheritdoc />
	public sealed override DateTimeOffset ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.PropertyName)
			throw new InvalidOperationException($"The json reader is at a token of type {reader.TokenType} instead of the expected PropertyName token type");

#if NET8_0_OR_GREATER
		Span<char> charBuffer = stackalloc char[MaximumFormatDateTimeOffsetLength];
		var bytesWritten = reader.CopyString(charBuffer);
		ReadOnlySpan<char> source = charBuffer.Slice(0, bytesWritten);

		var value = DateTimeOffset.Parse(source, null, System.Globalization.DateTimeStyles.AssumeUniversal);
#else
		var rawValue = reader.GetString() ?? string.Empty;
		var value = DateTimeOffset.Parse(rawValue, null, System.Globalization.DateTimeStyles.AssumeUniversal);
#endif

		return value;
	}

	/// <summary>
	/// <see href="https://github.com/dotnet/runtime/blob/f6a7ebbb81540401e6b5520afa3ba87c2bd6bcfe/src/libraries/System.Text.Json/src/System/Text/Json/Writer/JsonWriterHelper.Date.cs#L43">Copy of method from dotnet runtime JsonWriterHelper</see>
	/// <br />
	/// Trims roundtrippable DateTime(Offset) input.
	/// If the milliseconds part of the date is zero, we omit the fraction part of the date,
	/// else we write the fraction up to 7 decimal places with no trailing zeros. i.e. the output format is
	/// YYYY-MM-DDThh:mm:ss[.s]TZD where TZD = Z or +-hh:mm.
	/// e.g.
	///   ---------------------------------
	///   2017-06-12T05:30:45.768-07:00
	///   2017-06-12T05:30:45.00768Z           (Z is short for "+00:00" but also distinguishes DateTimeKind.Utc from DateTimeKind.Local)
	///   2017-06-12T05:30:45                  (interpreted as local time wrt to current time zone)
	/// </summary>
	public static void TrimDateTimeOffset(Span<byte> buffer, out int bytesWritten)
	{
		const int maxDateTimeLength = MaximumFormatDateTimeLength;

		// Assert buffer is the right length for:
		// YYYY-MM-DDThh:mm:ss.fffffff (JsonConstants.MaximumFormatDateTimeLength)
		// YYYY-MM-DDThh:mm:ss.fffffffZ (JsonConstants.MaximumFormatDateTimeLength + 1)
		// YYYY-MM-DDThh:mm:ss.fffffff(+|-)hh:mm (JsonConstants.MaximumFormatDateTimeOffsetLength)
		Debug.Assert(buffer.Length is maxDateTimeLength or
			(maxDateTimeLength + 1) or
			MaximumFormatDateTimeOffsetLength);

		// Find the last significant digit.
		int curIndex;
		if (buffer[maxDateTimeLength - 1] == '0')
		{
			if (buffer[maxDateTimeLength - 2] == '0')
			{
				if (buffer[maxDateTimeLength - 3] == '0')
				{
					if (buffer[maxDateTimeLength - 4] == '0')
					{
						if (buffer[maxDateTimeLength - 5] == '0')
						{
							if (buffer[maxDateTimeLength - 6] == '0')
							{
								if (buffer[maxDateTimeLength - 7] == '0')
								{
									// All decimal places are 0 so we can delete the decimal point too.
									curIndex = maxDateTimeLength - 7 - 1;
								}
								else { curIndex = maxDateTimeLength - 6; }
							}
							else { curIndex = maxDateTimeLength - 5; }
						}
						else { curIndex = maxDateTimeLength - 4; }
					}
					else { curIndex = maxDateTimeLength - 3; }
				}
				else { curIndex = maxDateTimeLength - 2; }
			}
			else { curIndex = maxDateTimeLength - 1; }
		}
		else
		{
			// There is nothing to trim.
			bytesWritten = buffer.Length;
			return;
		}

		// We are either trimming a DateTimeOffset, or a DateTime with
		// DateTimeKind.Local or DateTimeKind.Utc
		if (buffer.Length == maxDateTimeLength)
		{
			// There is no offset to copy.
			bytesWritten = curIndex;
		}
		else if (buffer.Length == MaximumFormatDateTimeOffsetLength)
		{
			// We have a non-UTC offset (+|-)hh:mm that are 6 characters to copy.
			buffer[curIndex] = buffer[maxDateTimeLength];
			buffer[curIndex + 1] = buffer[maxDateTimeLength + 1];
			buffer[curIndex + 2] = buffer[maxDateTimeLength + 2];
			buffer[curIndex + 3] = buffer[maxDateTimeLength + 3];
			buffer[curIndex + 4] = buffer[maxDateTimeLength + 4];
			buffer[curIndex + 5] = buffer[maxDateTimeLength + 5];
			bytesWritten = curIndex + 6;
		}
		else
		{
			// There is a single 'Z'. Just write it at the current index.
			Debug.Assert(buffer[maxDateTimeLength] == 'Z');

			buffer[curIndex] = (byte)'Z';
			bytesWritten = curIndex + 1;
		}
	}

	/// <inheritdoc />
#pragma warning disable IDE0022 // Use expression body for method
	public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
	{
#pragma warning disable CA1062 // Validate arguments of public methods
		writer.WriteStringValue(value);
#pragma warning restore CA1062 // Validate arguments of public methods
	}
#pragma warning restore IDE0022 // Use expression body for method

	/// <inheritdoc />
	public sealed override void WriteAsPropertyName(Utf8JsonWriter writer, [DisallowNull] DateTimeOffset value, JsonSerializerOptions options)
	{
		Span<byte> buffer = stackalloc byte[MaximumFormatDateTimeOffsetLength];
		var result = Utf8Formatter.TryFormat(value, buffer, out var bytesWritten, s_dateTimeStandardFormat);
		TrimDateTimeOffset(buffer.Slice(0, bytesWritten), out bytesWritten);
#pragma warning disable CA1062 // Validate arguments of public methods
		writer.WritePropertyName(buffer.Slice(0, bytesWritten));
#pragma warning restore CA1062 // Validate arguments of public methods
	}
}
