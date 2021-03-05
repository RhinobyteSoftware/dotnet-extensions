using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Rhinobyte.DataAnnotations
{
	/// <summary>
	/// 	Specify a <see cref="DateTime"/> range constraint (inclusive) between the minimum "1753-01-01 00:00:00" and maximum "9999-12-31 00:00:00" values supported by the
	/// 	<see href="https://docs.microsoft.com/en-us/sql/t-sql/data-types/date-transact-sql?view=sql-server-ver15">Sql Server date</see> data type.
	/// </summary>
	/// <remarks>
	/// 	Eliminates the thread safety problems with using the <see cref="RangeAttribute(Type, string string)"/> constructor overload.
	/// 	See <see href="https://github.com/dotnet/runtime/issues/1143"/>
	/// </remarks>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
	public class SqlServerDateAttribute : ValidationAttribute
	{
		/// <summary>
		/// The maximum <see cref="DateTime"/> value of "9999-12-31 00:00:00" for a Sql Server date data type.
		/// </summary>
		public static readonly DateTime Maximum = new DateTime(9999, 12, 31);

		/// <summary>
		/// The minimum <see cref="DateTime"/> value of "1753-01-01 00:00:00" for a Sql Server date data type.
		/// </summary>
		public static readonly DateTime Minimum = new DateTime(1753, 1, 1);

		/// <summary>
		/// Construct a new SqlServerDateAttribute instance.
		/// </summary>
		public SqlServerDateAttribute()
			: base(ErrorMessageAccessor)
		{
		}

		private static string ErrorMessageAccessor() => "The field {0} must be between {1} and {2}.";

		/// <summary>
		///     Override of <see cref="ValidationAttribute.FormatErrorMessage" />
		/// </summary>
		/// <param name="name">The user-visible name to include in the formatted message.</param>
		/// <returns>A localized string describing the minimum and maximum values</returns>
		/// <remarks>This override exists to provide a formatted message describing the minimum and maximum values</remarks>
		public override string FormatErrorMessage(string name)
			=> string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Minimum, Maximum);

		/// <summary>
		///		Returns true if the <paramref name="value"/> falls between minimum and maximum, inclusive.
		/// </summary>
		/// <remarks>
		///		Returns true for null values. Use the <see cref="RequiredAttribute"/> to assert a value is not empty.
		/// </remarks>
		/// <exception cref="InvalidCastException">Thrown if the <paramref name="value"/> cannot be cast to a <see cref="DateTime"/>.</exception>
		public override bool IsValid(object? value)
		{
			// Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
			if (value == null)
			{
				return true;
			}

			try
			{
				var dateTimeValue = (DateTime)value;
				return Minimum <= dateTimeValue && dateTimeValue <= Maximum;
			}
			catch (InvalidCastException exc)
			{
				var castException = new InvalidCastException("The [SqlServerDate] attribute must be used on a DateTime member", exc);
				castException.Data["value"] = value;
				throw castException;
			}
		}
	}
}
