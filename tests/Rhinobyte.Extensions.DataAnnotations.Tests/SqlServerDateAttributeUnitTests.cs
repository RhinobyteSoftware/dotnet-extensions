using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Globalization;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DataAnnotations.Tests
{
	[TestClass]
	public class SqlServerDateAttributeUnitTests
	{
		[DataTestMethod]
		[DataRow("SomeProperty")]
		[DataRow("DateEntered")]
		[DataRow("EnteredOn")]
		[DataRow("UpdatedOn")]
		[DataRow("UpdatedOn")]
		public void SqlServerDateAttribute_FormattedErrorMessage_returns_the_expected_result(string memberName)
		{
			var sqlServerDateAttribute = new SqlServerDateAttribute();

			var expectedResult = string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", memberName, SqlServerDateAttribute.Minimum, SqlServerDateAttribute.Maximum);
			sqlServerDateAttribute.FormatErrorMessage(memberName).Should().Be(expectedResult);
		}

		[DataTestMethod]
		[DataRow("1753-01-01 00:00:00", true)]
		[DataRow("1900-01-01 00:00:00", true)]
		[DataRow("1950-06-06 13:00:00", true)]
		[DataRow("2079-06-06 00:00:00", true)]
		[DataRow("0001-01-01", false)]
		[DataRow("1752-12-31 00:00:00", false)]
		[DataRow("9999-12-31 00:00:31", false)]
		[DataRow("9999-12-31 23:59:59", false)]
		public void SqlServerDateAttribute_IsValid_returns_the_expected_result(string value, bool expectedResult)
		{
			var dateTimeValue = DateTime.Parse(value);

			var sqlServerDateAttribute = new SqlServerDateAttribute();

			sqlServerDateAttribute.IsValid(dateTimeValue).Should().Be(expectedResult);
		}

		[TestMethod]
		public void SqlServerDateAttribute_IsValid_returns_true_for_a_null_value()
		{
			var sqlServerDateAttribute = new SqlServerDateAttribute();
			sqlServerDateAttribute.IsValid(null).Should().Be(true);
		}

		[DataTestMethod]
		[DataRow("stringValue")]
		[DataRow(1900)]
		[DataRow(true)]
		public void SqlServerDateAttribute_IsValid_throws_for_non_datetime_values(object value)
		{
			var sqlServerDateAttribute = new SqlServerDateAttribute();

			Invoking(() => sqlServerDateAttribute.IsValid(value))
				.Should()
				.Throw<InvalidCastException>()
				.WithMessage(@"The [SqlServerDate] attribute must be used on a DateTime member. [MemberName: """"]");
		}
	}
}
