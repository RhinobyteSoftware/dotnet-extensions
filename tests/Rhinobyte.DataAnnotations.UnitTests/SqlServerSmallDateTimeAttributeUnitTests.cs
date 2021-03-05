using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.DataAnnotations.UnitTests
{
	[TestClass]
	public class SqlServerSmallDateTimeAttributeUnitTests
	{
		[DataTestMethod]
		[DataRow("SomeProperty")]
		[DataRow("DateEntered")]
		[DataRow("EnteredOn")]
		[DataRow("UpdatedOn")]
		[DataRow("UpdatedOn")]
		public void SqlServerSmallDateTimeAttribute_FormattedErrorMessage_returns_the_expected_result(string memberName)
		{
			var sqlServerSmallDateTimeAttribute = new SqlServerSmallDateTimeAttribute();

			var expectedResult = string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", memberName, SqlServerSmallDateTimeAttribute.Minimum, SqlServerSmallDateTimeAttribute.Maximum);
			sqlServerSmallDateTimeAttribute.FormatErrorMessage(memberName).Should().Be(expectedResult);
		}

		[DataTestMethod]
		[DataRow("1900-01-01 00:00:00", true)]
		[DataRow("1900-01-01 23:59:59", true)]
		[DataRow("1950-06-06 13:00:00", true)]
		[DataRow("2079-06-06 00:00:00", true)]
		[DataRow("2079-06-06 23:59:00", true)]
		[DataRow("0001-01-01", false)]
		[DataRow("1899-12-31 23:59:59", false)]
		[DataRow("2079-06-06 23:59:59", false)]
		[DataRow("2079-06-07 00:00:00", false)]
		[DataRow("9999-12-31", false)]
		public void SqlServerSmallDateTimeAttribute_IsValid_returns_the_expected_result(string value, bool expectedResult)
		{
			var dateTimeValue = DateTime.Parse(value);

			var sqlServerSmallDateTimeAttribute = new SqlServerSmallDateTimeAttribute();

			sqlServerSmallDateTimeAttribute.IsValid(dateTimeValue).Should().Be(expectedResult);
		}

		[TestMethod]
		public void SqlServerSmallDateTimeAttribute_IsValid_returns_true_for_a_null_value()
		{
			var sqlServerSmallDateTimeAttribute = new SqlServerSmallDateTimeAttribute();
			sqlServerSmallDateTimeAttribute.IsValid(null).Should().Be(true);
		}

		[DataTestMethod]
		[DataRow("stringValue")]
		[DataRow(1900)]
		[DataRow(true)]
		public void SqlServerSmallDateTimeAttribute_IsValid_throws_for_non_datetime_values(object value)
		{
			var sqlServerSmallDateTimeAttribute = new SqlServerSmallDateTimeAttribute();

			Invoking(() => sqlServerSmallDateTimeAttribute.IsValid(value))
				.Should()
				.Throw<InvalidCastException>()
				.WithMessage("The [SqlServerSmallDateTime] attribute must be used on a DateTime member");
		}
	}
}
