using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DataAnnotations.Tests
{
	[TestClass]
	public class DateRangeAttributeUnitTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[DataTestMethod]
		[DataRow("SomeProperty", "1900-01-01", "1999-12-31 23:59:59")]
		[DataRow("DateEntered", "1900-01-01", "2079-06-06 23:59:00")]
		[DataRow("EnteredOn", "1753-01-01", "9999-12-31 23:59:59")]
		[DataRow("UpdatedOn", "0001-01-01", "9999-12-31 23:59:59")]
		[DataRow("UpdatedOn", "0001-01-01", "0001-01-01")]
		public void DateTimeRangeAttribute_FormattedErrorMessage_returns_the_expected_result(string memberName, string minimum, string maximum)
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttribute(minimum, maximum);

			var expectedResult = string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", memberName, DateTime.Parse(minimum), DateTime.Parse(maximum));
			dateTimeRangeAttribute.FormatErrorMessage(memberName).Should().Be(expectedResult);
		}

		[DataTestMethod]
		[DataRow("SomeProperty", "1900-01-01", "invalid")]
		[DataRow("DateEntered", "blue", "1999-12-31")]
		public void DateTimeRangeAttribute_FormattedErrorMessage_throws_format_excpetion_when_the_attribute_parameters_are_invalid(string memberName, string minimum, string maximum)
		{
			Invoking(() => new DateTimeRangeAttribute(minimum, maximum).FormatErrorMessage(memberName))
				.Should()
				.Throw<FormatException>()
				.WithMessage($@"The [DateTimeRange] attribute minimum/maximum parameters must be valid datetime strings. [MemberName: ""{memberName}""]");
		}

		[TestMethod]
		public void DateTimeRangeAttribute_FormattedErrorMessage_throws_invalid_operation_exception_if_the_minimum_is_greater_than_the_maximum()
		{
			Invoking(() => new DateTimeRangeAttribute("2001-01-01", "1900-01-01").FormatErrorMessage("MinimumGreaterThanMaximum"))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(@"[DateTimeRange] attribute minimum must be less than or equal to the maximum. [MemberName: ""MinimumGreaterThanMaximum""]");
		}

		[DataTestMethod]
		[DataRow("EnteredOn", "", "1999-12-31")]
		[DataRow("EnteredOn", "1900-01-01", "")]
		[DataRow("BirthDate", null, "1999-12-31")]
		[DataRow("Something", "-500-01-01", null)]
		public void DateTimeRangeAttribute_FormattedErrorMessage_throws_invalid_operation_exception_when_the_attribute_parameters_are_null_or_empty(string memberName, string minimum, string maximum)
		{
			Invoking(() => new DateTimeRangeAttribute(minimum, maximum).FormatErrorMessage(memberName))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($@"[DateTimeRange] attribute minimum/maximum are required. [MemberName: ""{memberName}""]");
		}

		[DataTestMethod]
		[DataRow("1900-01-01 00:00:00", true)]
		[DataRow("1900-01-01 23:59:59", true)]
		[DataRow("1950-06-06 13:00:00", true)]
		[DataRow("1999-12-31 00:00:00", true)]
		[DataRow("1999-12-31 23:59:59", true)]
		[DataRow("0001-01-01", false)]
		[DataRow("1899-12-31 23:59:59", false)]
		[DataRow("2000-01-01", false)]
		[DataRow("9999-12-31", false)]
		public void DateTimeRangeAttribute_IsValid_returns_the_expected_result(string value, bool expectedResult)
		{
			var dateTimeValue = DateTime.Parse(value);

			var dateTimeRangeAttribute = new DateTimeRangeAttribute("1900-01-01", "1999-12-31 23:59:59");

			dateTimeRangeAttribute.IsValid(dateTimeValue).Should().Be(expectedResult);
		}

		[TestMethod]
		public void DateTimeRangeAttribute_IsValid_returns_true_for_a_null_value()
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttribute("1900-01-01", "1999-12-31 23:59:59");
			dateTimeRangeAttribute.IsValid(null).Should().Be(true);
		}

		[DataTestMethod]
		[DataRow("stringValue")]
		[DataRow(1900)]
		[DataRow(true)]
		public void DateTimeRangeAttribute_IsValid_throws_for_non_datetime_values(object value)
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttribute("1900-01-01", "1999-12-31 23:59:59");

			Invoking(() => dateTimeRangeAttribute.IsValid(value))
				.Should()
				.Throw<InvalidCastException>()
				.WithMessage(@"The [DateTimeRange] attribute must be used on a DateTime member. [MemberName: """"]");
		}

		[TestMethod]
		public void DateTimeRangeAttribute_IsValid_throws_if_the_minimum_is_greater_than_the_maximum()
		{
			Invoking(() => new DateTimeRangeAttribute("2001-01-01", "1900-01-01").IsValid(DateTime.Today))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(@"[DateTimeRange] attribute minimum must be less than or equal to the maximum. [MemberName: """"]");
		}

		[DataTestMethod]
		[DataRow("1900-01-01", "invalid")]
		[DataRow("blue", "1999-12-31")]
		public void DateTimeRangeAttribute_IsValid_throws_when_the_attribute_parameters_are_invalid(string minimum, string maximum)
		{
			Invoking(() => new DateTimeRangeAttribute(minimum, maximum).IsValid(DateTime.Today))
				.Should()
				.Throw<FormatException>()
				.WithMessage(@"The [DateTimeRange] attribute minimum/maximum parameters must be valid datetime strings. [MemberName: """"]");
		}

		[DataTestMethod]
		[DataRow("", "1999-12-31")]
		[DataRow("1900-01-01", "")]
		[DataRow(null, "1999-12-31")]
		[DataRow("-500-01-01", null)]
		public void DateTimeRangeAttribute_IsValid_throws_when_the_attribute_parameters_are_null_or_empty(string minimum, string maximum)
		{
			Invoking(() => new DateTimeRangeAttribute(minimum, maximum).IsValid(DateTime.Today))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage(@"[DateTimeRange] attribute minimum/maximum are required. [MemberName: """"]");
		}

		[DataTestMethod]
		[DataRow("1900-01-01 00:00:00", true)]
		[DataRow("1900-01-01 23:59:59", true)]
		[DataRow("1950-06-06 13:00:00", true)]
		[DataRow("1999-12-31 00:00:00", true)]
		[DataRow("1999-12-31 23:59:59", true)]
		[DataRow("0001-01-01", false)]
		[DataRow("1899-12-31 23:59:59", false)]
		[DataRow("2000-01-01", false)]
		[DataRow("9999-12-31", false)]
		public void IsValid_returns_the_expected_result_when_attribute_is_constructed_using_the_protected_constructor_with_DateTime_arguments(string value, bool expectedResult)
		{
			var dateTimeValue = DateTime.Parse(value);

			var minimum = DateTime.Parse("1900-01-01");
			var maximum = DateTime.Parse("1999-12-31 23:59:59");
			var dateTimeRangeAttribute = new DateTimeRangeAttributeSubclass(minimum, maximum);

			dateTimeRangeAttribute.IsValid(dateTimeValue).Should().Be(expectedResult);
		}

		[TestMethod]
		public void ParseRangeValuesIfNecessary_does_not_throw_when_minimum_is_equal_to_the_maximum()
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttributeSubclass("1900-01-01", "1900-01-01");
			Invoking(() => dateTimeRangeAttribute.ParseRangeValuesIfNecessary("MockDisplayName"))
				.Should()
				.NotThrow();
		}

		[TestMethod]
		public void ParseRangeValuesIfNecessary_does_not_throw_when_minimum_is_less_than_maximum()
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttributeSubclass("1900-01-01", "1999-12-31 23:59:59");
			Invoking(() => dateTimeRangeAttribute.ParseRangeValuesIfNecessary("MockDisplayName"))
				.Should()
				.NotThrow();
		}

		[TestMethod]
		public void ParseRangeValuesIfNecessary_throws_InvalidOperationException_if_minimum_is_greater_than_maximum()
		{
			var minimum = DateTime.Parse("1900-01-01");
			var maximum = DateTime.Parse("1999-12-31 23:59:59");
			var dateTimeRangeAttribute = new DateTimeRangeAttributeSubclass(maximum, minimum); // flip them so min > max
			Invoking(() => dateTimeRangeAttribute.ParseRangeValuesIfNecessary("MockDisplayName"))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage("[DateTimeRange] attribute minimum must be less than or equal to the maximum.*");
		}


		/******     TEST SETUP     *****************************
		 *******************************************************/
		public class DateTimeRangeAttributeSubclass : DateTimeRangeAttribute
		{
			public DateTimeRangeAttributeSubclass(DateTime minimum, DateTime maximum)
				: base(minimum, maximum) { }

			public DateTimeRangeAttributeSubclass(string minimum, string maximum)
				: base(minimum, maximum) { }

			public new void ParseRangeValuesIfNecessary(string displayName)
				=> base.ParseRangeValuesIfNecessary(displayName);
		}
	}
}
