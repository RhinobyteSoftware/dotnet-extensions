using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Rhinobyte.DataAnnotations
{
	/// <summary>
	/// 	Specify a <see cref="DateTime"/> range constraint. Requires a minimum and maximum date value specified in string format.
	/// </summary>
	/// <remarks>
	/// 	Eliminates the thread safety problems with using the <see cref="RangeAttribute(Type, string string)"/> constructor overload.
	/// 	See <see href="https://github.com/dotnet/runtime/issues/1143"/>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class DateTimeRangeAttribute : ValidationAttribute
	{
		private readonly DateTime? _maximum;
		private readonly DateTime? _minimum;

		protected DateTimeRangeAttribute(DateTime minimum, DateTime maximum)
		{
			_minimum = minimum;
			_maximum = maximum;
		}

		/// <summary>
		/// Constructor takes the minimum and maximum date values (inclusive) and attempts to parse them using <see cref="DateTime.TryParse(string, out DateTime)"/>.
		/// If the values cannot be parsed then <see cref="IsValid(object?)"/> will return false and <see cref="FormatErrorMessage(string)"/> will throw an exception
		/// indicating that the attribute values we're not correctly set.
		/// </summary>
		/// <param name="minimum">The minimum date constraint (inclusive).</param>
		/// <param name="maximum">The maximum date constraint (inclusive).</param>
		public DateTimeRangeAttribute(string minimum, string maximum)
			: base(ErrorMessageAccessor)
		{
			if (DateTime.TryParse(minimum, out var parsedMinimum))
			{
				_minimum = parsedMinimum;
			}

			if (DateTime.TryParse(maximum, out var parsedMaximum))
			{
				_maximum = parsedMaximum;
			}
		}

		private static string ErrorMessageAccessor() => "The field {0} must be between {1} and {2}.";

		/// <summary>
		///     Override of <see cref="ValidationAttribute.FormatErrorMessage" />
		/// </summary>
		/// <param name="name">The user-visible name to include in the formatted message.</param>
		/// <returns>A localized string describing the minimum and maximum values</returns>
		/// <remarks>This override exists to provide a formatted message describing the minimum and maximum values</remarks>
		/// <exception cref="InvalidOperationException"> is thrown if the current attribute's minimum or maximum are not set.</exception>
		public override string FormatErrorMessage(string name)
		{
			if (_minimum == null || _maximum == null || _minimum > _maximum)
			{
				throw new InvalidOperationException($@"[DateTimeRange] attribute minimum/maximum are missing or invalid. [FieldName: ""{name}""]");
			}

			return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _minimum, _maximum);
		}

		/// <summary>
		///		Returns true if the <paramref name="value"/> falls between minimum and maximum, inclusive.
		/// </summary>
		/// <remarks>
		///		<para>
		///			Returns true for null values. Use the <see cref="RequiredAttribute"/> to assert a value is not empty.
		///		</para>
		///		<para>
		///			Returns false if the minimum or maximum are not set. This is done so that the
		///			<see cref="FormatErrorMessage"/> override can throw an exception containing the field name.
		///		</para>
		/// </remarks>
		/// <exception cref="InvalidCastException">Thrown if the <paramref name="value"/> cannot be cast to a <see cref="DateTime"/>.</exception>
		public override bool IsValid(object? value)
		{
			if (_minimum == null || _maximum == null || _minimum > _maximum)
			{
				// Return false so we can let the FormatErrorMessage() override throw an exception
				// that includes the field name.
				return false;
			}

			// Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
			if (value == null)
			{
				return true;
			}

			try
			{
				var dateTimeValue = (DateTime)value;
				return _minimum <= dateTimeValue && dateTimeValue <= _maximum;
			}
			catch (InvalidCastException exc)
			{
				var castException = new InvalidCastException("The [DateTimeRange] attribute must be used on a DateTime member", exc);
				castException.Data["value"] = value;
				throw castException;
			}
		}
	}
}
