using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DataAnnotations.UnitTests
{
	[TestClass]
	public class SqlServerDateTimeAttributeUnitTests
	{
		[DataTestMethod]
		[DataRow("SomeProperty")]
		[DataRow("DateEntered")]
		[DataRow("EnteredOn")]
		[DataRow("UpdatedOn")]
		[DataRow("UpdatedOn")]
		public void SqlServerDateTimeAttribute_FormattedErrorMessage_returns_the_expected_result(string memberName)
		{
			var sqlServerDateTimeAttribute = new SqlServerDateTimeAttribute();

			var expectedResult = string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", memberName, SqlServerDateTimeAttribute.Minimum, SqlServerDateTimeAttribute.Maximum);
			sqlServerDateTimeAttribute.FormatErrorMessage(memberName).Should().Be(expectedResult);
		}

		[DataTestMethod]
		[DataRow("1753-01-01 00:00:00", true)]
		[DataRow("1753-01-01 23:59:59", true)]
		[DataRow("1950-06-06 13:00:00", true)]
		[DataRow("2079-06-06 00:00:00", true)]
		[DataRow("9999-12-31 23:59:59", true)]
		[DataRow("0001-01-01", false)]
		[DataRow("9999-12-31 23:59:59.99999", false)]
		public void SqlServerDateTimeAttribute_IsValid_returns_the_expected_result(string value, bool expectedResult)
		{
			var dateTimeValue = DateTime.Parse(value);

			var sqlServerDateTimeAttribute = new SqlServerDateTimeAttribute();

			sqlServerDateTimeAttribute.IsValid(dateTimeValue).Should().Be(expectedResult);
		}

		[TestMethod]
		public void SqlServerDateTimeAttribute_IsValid_returns_true_for_a_null_value()
		{
			var sqlServerDateTimeAttribute = new SqlServerDateTimeAttribute();
			sqlServerDateTimeAttribute.IsValid(null).Should().Be(true);
		}

		[DataTestMethod]
		[DataRow("stringValue")]
		[DataRow(1900)]
		[DataRow(true)]
		public void SqlServerDateTimeAttribute_IsValid_throws_for_non_datetime_values(object value)
		{
			var sqlServerDateTimeAttribute = new SqlServerDateTimeAttribute();

			Invoking(() => sqlServerDateTimeAttribute.IsValid(value))
				.Should()
				.Throw<InvalidCastException>()
				.WithMessage(@"The [SqlServerDateTime] attribute must be used on a DateTime member. [MemberName: """"]");
		}
	}
}
