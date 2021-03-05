using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.DataAnnotations.UnitTests
{
	[TestClass]
	public class DateRangeAttributeUnitTests
	{
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
		[DataRow("EnteredOn", "", "1999-12-31")]
		[DataRow("EnteredOn", "1900-01-01", "")]
		[DataRow("BirthDate", null, "1999-12-31")]
		[DataRow("Something", "-500-01-01", null)]
		[DataRow("MinimumGreaterThanMaximum", "1999-01-01", "1900-01-01")]
		public void DateTimeRangeAttribute_FormattedErrorMessage_throws_when_the_attribute_parameters_are_invalid(string memberName, string minimum, string maximum)
		{
			Invoking(() => new DateTimeRangeAttribute(minimum, maximum).FormatErrorMessage(memberName))
				.Should()
				.Throw<InvalidOperationException>()
				.WithMessage($@"[DateTimeRange] attribute minimum/maximum are missing or invalid. [FieldName: ""{memberName}""]");
		}



		[DataTestMethod]
		[DataRow("1900-01-01", "invalid")]
		[DataRow("blue", "1999-12-31")]
		[DataRow("", "1999-12-31")]
		[DataRow("1900-01-01", "")]
		[DataRow(null, "1999-12-31")]
		[DataRow("-500-01-01", null)]
		[DataRow("1999-01-01", "1900-01-01")]
		public void DateTimeRangeAttribute_IsValid_returns_false_when_the_attribute_parameters_are_invalid(string minimum, string maximum)
		{
			var dateTimeRangeAttribute = new DateTimeRangeAttribute(minimum, maximum);
			dateTimeRangeAttribute.IsValid(DateTime.Today).Should().Be(false);
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
				.WithMessage("The [DateTimeRange] attribute must be used on a DateTime member");
		}
	}
}
