using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Rhinobyte.Extensions.DataAnnotations
{
	/// <summary>
	/// 	Specify a <see cref="DateTime"/> range constraint. Requires a minimum and maximum date value specified in string format.
	/// </summary>
	/// <remarks>
	/// 	Eliminates the thread safety problems with using the <see cref="RangeAttribute(Type, string string)"/> constructor overload.
	/// 	See <see href="https://github.com/dotnet/runtime/issues/1143"/>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
#pragma warning disable CA1813 // Avoid unsealed attributes
	public class DateTimeRangeAttribute : ValidationAttribute
#pragma warning restore CA1813 // Avoid unsealed attributes
	{
		private DateTime? _maximum;
		private DateTime? _minimum;
		private readonly string? _maximumStringValue;
		private readonly string? _minimumStringValue;

		protected DateTimeRangeAttribute(DateTime minimum, DateTime maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}

		///	<summary>
		///		Constructor takes the minimum and maximum date values (inclusive).
		///		<para>
		///			The <see cref="IsValid(object, ValidationContext)"/> and <see cref="FormatErrorMessage(string)"/> methods will attempt to parse
		///			the values (and cache the parsed result). If the values cannot be parsed then an exception will be thrown.
		///		</para>
		/// </summary>
		/// <param name="minimum">The minimum date constraint (inclusive).</param>
		/// <param name="maximum">The maximum date constraint (inclusive).</param>
#pragma warning disable CA1019 // Define accessors for attribute arguments
		public DateTimeRangeAttribute(string minimum, string maximum)
#pragma warning restore CA1019 // Define accessors for attribute arguments
			: base(errorMessageAccessor: null)
		{
			_minimumStringValue = minimum;
			_maximumStringValue = maximum;
		}

		/// <summary>
		///     Override of <see cref="ValidationAttribute.FormatErrorMessage" />
		/// </summary>
		/// <param name="name">The user-visible name to include in the formatted message.</param>
		/// <returns>A localized string describing the minimum and maximum values</returns>
		/// <remarks>This override exists to provide a formatted message describing the minimum and maximum values</remarks>
		/// <exception cref="FormatException">Thrown if the current attribute's minimum/maximum string parameters cannot be parsed.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the current attribute's minimum or maximum are not set or are invalid.</exception>
		public override string FormatErrorMessage(string? name)
		{
			ParseRangeValuesIfNecessary(name);

			return string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", name, _minimum, _maximum);
		}

		/// <summary>
		///		Returns <see cref="ValidationResult.Success"/> if the <paramref name="value"/> falls between minimum and maximum, inclusive.
		///		Returns a <see cref="ValidationResult"/> with a descriptive non-null error message, otherwise.
		/// </summary>
		/// <remarks>
		///		<para>
		///			Returns <see cref="ValidationResult.Success"/> for null values. Use the <see cref="RequiredAttribute"/> to assert a value is not empty.
		///		</para>
		///		<para>
		///			Throws an exception if the required minimum and maximum values are missing or invalid.
		///		</para>
		/// </remarks>
		/// <exception cref="FormatException">Thrown if the current attribute's minimum/maximum string parameters cannot be parsed.</exception>
		/// <exception cref="InvalidCastException">Thrown if the <paramref name="value"/> cannot be cast to a <see cref="DateTime"/>.</exception>
		/// <exception cref="InvalidOperationException">Thrown if the current attribute's minimum or maximum are not set or are invalid.</exception>
		protected override ValidationResult IsValid(object value, ValidationContext validationContext)
		{
			ParseRangeValuesIfNecessary(validationContext?.DisplayName);

			// Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
			if (value is null)
			{
				return ValidationResult.Success;
			}

			try
			{
				var dateTimeValue = (DateTime)value;
				if (_minimum <= dateTimeValue && dateTimeValue <= _maximum)
				{
					return ValidationResult.Success;
				}

				var memberNames = validationContext?.MemberName is { } memberName
					? new[] { memberName }
					: null;
				return new ValidationResult(FormatErrorMessage(validationContext?.DisplayName), memberNames);
			}
			catch (InvalidCastException exc)
			{
				var castException = new InvalidCastException($@"The [DateTimeRange] attribute must be used on a DateTime member. [MemberName: ""{validationContext?.DisplayName}""]", exc);
				castException.Data["ValidationValue"] = value;
				throw castException;
			}
		}

		protected void ParseRangeValuesIfNecessary(string? displayName)
		{
			if (_minimum != null && _maximum != null)
			{
				if (_minimum > _maximum)
				{
					throw new InvalidOperationException($@"[DateTimeRange] attribute minimum must be less than or equal to the maximum. [MemberName: ""{displayName}""]");
				}

				return;
			}

			if (string.IsNullOrEmpty(_minimumStringValue) || string.IsNullOrEmpty(_maximumStringValue))
			{
				throw new InvalidOperationException($@"[DateTimeRange] attribute minimum/maximum are required. [MemberName: ""{displayName}""]");
			}

			try
			{
				_minimum = DateTime.Parse(_minimumStringValue, CultureInfo.CurrentCulture);
				_maximum = DateTime.Parse(_maximumStringValue, CultureInfo.CurrentCulture);

				if (_minimum > _maximum)
				{
					throw new InvalidOperationException($@"[DateTimeRange] attribute minimum must be less than or equal to the maximum. [MemberName: ""{displayName}""]");
				}
			}
			catch (FormatException formatException)
			{
				var wrappedFormatException = new FormatException($@"The [DateTimeRange] attribute minimum/maximum parameters must be valid datetime strings. [MemberName: ""{displayName}""]", formatException);
				wrappedFormatException.Data[nameof(_maximumStringValue)] = _maximumStringValue;
				wrappedFormatException.Data[nameof(_minimumStringValue)] = _minimumStringValue;
				throw wrappedFormatException;
			}
		}
	}
}
