﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace Rhinobyte.Extensions.DataAnnotations;

/// <summary>
/// 	Specify a <see cref="DateTime"/> range constraint (inclusive) between the minimum "1900-01-01 00:00:00" and maximum "2079-06-06 23:59:00" values supported by the
/// 	<see href="https://docs.microsoft.com/en-us/sql/t-sql/data-types/smalldatetime-transact-sql?view=sql-server-ver15">Sql Server smalldatetime</see> data type.
/// </summary>
/// <remarks>
/// 	Eliminates the thread safety problems with using the <see cref="RangeAttribute(Type, string, string)"/> constructor overload.
/// 	See <see href="https://github.com/dotnet/runtime/issues/1143"/>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class SqlServerSmallDateTimeAttribute : ValidationAttribute
{
	/// <summary>
	/// The maximum <see cref="DateTime"/> value of "2079-06-06 23:59:00" for a Sql Server smalldatetime data type.
	/// </summary>
	public static readonly DateTime Maximum = new DateTime(2079, 6, 6, 23, 59, 00);

	/// <summary>
	/// The minimum <see cref="DateTime"/> value of "1900-01-01 00:00:00" for a Sql Server smalldatetime data type.
	/// </summary>
	public static readonly DateTime Minimum = new DateTime(1900, 1, 1);

	/// <summary>
	/// Construct a new SqlServerSmallDateTimeAttribute instance.
	/// </summary>
	public SqlServerSmallDateTimeAttribute()
		: base(errorMessageAccessor: null)
	{
	}

	/// <summary>
	///     Override of <see cref="ValidationAttribute.FormatErrorMessage" />
	/// </summary>
	/// <param name="name">The user-visible name to include in the formatted message.</param>
	/// <returns>A localized string describing the minimum and maximum values</returns>
	/// <remarks>This override exists to provide a formatted message describing the minimum and maximum values</remarks>
	public override string FormatErrorMessage(string? name)
		=> string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", name, Minimum, Maximum);

	/// <summary>
	///		Returns <see cref="ValidationResult.Success"/> if the <paramref name="value"/> falls between minimum and maximum, inclusive.
	/// </summary>
	/// <remarks>
	///		Returns <see cref="ValidationResult.Success"/> for null values. Use the <see cref="RequiredAttribute"/> to assert a value is not empty.
	/// </remarks>
	/// <exception cref="InvalidCastException">Thrown if the <paramref name="value"/> cannot be cast to a <see cref="DateTime"/>.</exception>
	protected override ValidationResult IsValid(object? value, ValidationContext? validationContext)
	{
		// Automatically pass if value is null or empty. RequiredAttribute should be used to assert a value is not empty.
		if (value is null)
		{
			return ValidationResult.Success;
		}

		try
		{
			var dateTimeValue = (DateTime)value;
			if (Minimum <= dateTimeValue && dateTimeValue <= Maximum)
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
			var castException = new InvalidCastException($@"The [SqlServerSmallDateTime] attribute must be used on a DateTime member. [MemberName: ""{validationContext?.DisplayName}""]", exc);
			castException.Data["ValidationValue"] = value;
			throw castException;
		}
	}
}
