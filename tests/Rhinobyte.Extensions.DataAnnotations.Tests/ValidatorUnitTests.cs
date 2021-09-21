using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DataAnnotations.Tests
{
	[TestClass]
	public class ValidatorUnitTests
	{
		private const string DateTimeRangeMaximum = "1999-12-31 23:59:59";
		private const string DateTimeRangeMinimum = "1900-01-01";

		public static readonly Type[] DateTimeValidationAttributeTypes = new Type[] {
			typeof(DateTimeRangeAttribute),
			typeof(SqlServerDateAttribute),
			typeof(SqlServerDateTimeAttribute),
			typeof(SqlServerSmallDateTimeAttribute)
		};

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_fails_when_above_maximum(Type attributeType)
		{
			var aboveMaximumValue = GetMaximumValueForAttributeType(attributeType);
			if (aboveMaximumValue.Year == 9999)
			{
				aboveMaximumValue = DateTime.MaxValue;
			}
			else
			{
				aboveMaximumValue = aboveMaximumValue.AddSeconds(50);
			}

			var model = CreateTestModel(attributeType, aboveMaximumValue);
			var validationFailures = ValidateObject(model);
			VerifyValidationFailsWithExpectedMessage("PropertyToValidate", GetMinimumValueForAttributeType(attributeType), GetMaximumValueForAttributeType(attributeType), validationFailures);

			var nullableModel = CreateTestModelWithNullableProperty(attributeType, aboveMaximumValue);
			var nullableValidationFailures = ValidateObject(nullableModel);
			VerifyValidationFailsWithExpectedMessage("PropertyToValidate", GetMinimumValueForAttributeType(attributeType), GetMaximumValueForAttributeType(attributeType), nullableValidationFailures);
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_fails_when_below_minimum(Type attributeType)
		{
			var belowMinimumValue = GetMinimumValueForAttributeType(attributeType);
			if (belowMinimumValue == DateTime.MinValue)
			{
				return;
			}

			belowMinimumValue = belowMinimumValue.AddMinutes(-1);

			var model = CreateTestModel(attributeType, belowMinimumValue);
			var validationFailures = ValidateObject(model);
			VerifyValidationFailsWithExpectedMessage("PropertyToValidate", GetMinimumValueForAttributeType(attributeType), GetMaximumValueForAttributeType(attributeType), validationFailures);

			var nullableModel = CreateTestModelWithNullableProperty(attributeType, belowMinimumValue);
			var nullableValidationFailures = ValidateObject(nullableModel);
			VerifyValidationFailsWithExpectedMessage("PropertyToValidate", GetMinimumValueForAttributeType(attributeType), GetMaximumValueForAttributeType(attributeType), nullableValidationFailures);
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_succeeds_for_null_value(Type attributeType)
		{
			var nullableModel = CreateTestModelWithNullableProperty(attributeType, null);
			var nullableValidationFailures = ValidateObject(nullableModel);
			nullableValidationFailures.Should().BeEmpty();
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_succeeds_for_range_maximum_value(Type attributeType)
		{
			var rangeMaximumValue = GetMaximumValueForAttributeType(attributeType);

			var model = CreateTestModel(attributeType, rangeMaximumValue);
			var validationFailures = ValidateObject(model);
			validationFailures.Should().BeEmpty();

			var nullableModel = CreateTestModelWithNullableProperty(attributeType, rangeMaximumValue);
			var nullableValidationFailures = ValidateObject(nullableModel);
			nullableValidationFailures.Should().BeEmpty();
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_succeeds_for_range_minimum_value(Type attributeType)
		{
			var rangeMinimumValue = GetMinimumValueForAttributeType(attributeType);

			var model = CreateTestModel(attributeType, rangeMinimumValue);
			var validationFailures = ValidateObject(model);
			validationFailures.Should().BeEmpty();

			var nullableModel = CreateTestModelWithNullableProperty(attributeType, rangeMinimumValue);
			var nullableValidationFailures = ValidateObject(nullableModel);
			nullableValidationFailures.Should().BeEmpty();
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_succeeds_for_valid_datetime(Type attributeType)
		{
			var validDateTime = new DateTime(1950, 5, 5);

			var model = CreateTestModel(attributeType, validDateTime);
			var validationFailures = ValidateObject(model);
			validationFailures.Should().BeEmpty();

			var nullableModel = CreateTestModelWithNullableProperty(attributeType, validDateTime);
			var nullableValidationFailures = ValidateObject(nullableModel);
			nullableValidationFailures.Should().BeEmpty();
		}



		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public async Task Validation_succeeds_when_run_concurrently(Type attributeType)
		{
			const int concurrencyCount = 200;

			var validDateTime = new DateTime(1950, 5, 5);

			using var syncLock = new SemaphoreSlim(0, concurrencyCount);
			var concurrentTasks = Enumerable.Range(0, concurrencyCount)
				.Select(taskIndex => Task.Run(async () =>
				{
					var model = CreateTestModel(attributeType, validDateTime);

					await syncLock.WaitAsync().ConfigureAwait(false);

					var validationFailures = ValidateObject(model);
					return validationFailures;
				}))
				.ToList();

			syncLock.Release(concurrencyCount);

			var concurrentValidationResults = await Task.WhenAll(concurrentTasks).ConfigureAwait(false);
			concurrentValidationResults.Length.Should().Be(concurrencyCount);
			foreach (var validationResults in concurrentValidationResults)
			{
				validationResults.Should().BeEmpty();
			}
		}

		[DataTestMethod]
		[DynamicData(nameof(TestData_ValidationAttribute_TypesNames), dynamicDataSourceType: DynamicDataSourceType.Method, DynamicDataDisplayName = nameof(FormatDynamicDataTestName))]
		public void Validation_throws_for_non_datetime_property(Type attributeType)
		{
			var validDateTimeString = new DateTime(1950, 1, 1).ToString();

			var modelToValidate = CreateTestModelWithStringProperty(attributeType, validDateTimeString);

			VerifyValidationThrowsInvalidCastException(modelToValidate);
		}

		private static object CreateTestModel(Type attributeType, DateTime propertyValue)
		{
			// Constructing them using reflection doesn't add the attributes so we'll manually do so here:
			if (attributeType == typeof(DateTimeRangeAttribute))
			{
				return new DateTimeRangeAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateAttribute))
			{
				return new SqlServerDateAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateTimeAttribute))
			{
				return new SqlServerDateTimeAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerSmallDateTimeAttribute))
			{
				return new SqlServerSmallDateTimeAttributeModel { PropertyToValidate = propertyValue };
			}

			throw new NotSupportedException($"{nameof(CreateTestModel)} is not supported for {attributeType.Name}");
		}

		private static object CreateTestModelWithNullableProperty(Type attributeType, DateTime? propertyValue)
		{
			// Constructing them using reflection doesn't add the attributes so we'll manually do so here:
			if (attributeType == typeof(DateTimeRangeAttribute))
			{
				return new NullableDateTimeRangeAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateAttribute))
			{
				return new NullableSqlServerDateAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateTimeAttribute))
			{
				return new NullableSqlServerDateTimeAttributeModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerSmallDateTimeAttribute))
			{
				return new NullableSqlServerSmallDateTimeAttributeModel { PropertyToValidate = propertyValue };
			}

			throw new NotSupportedException($"{nameof(CreateTestModelWithNullableProperty)} is not supported for {attributeType.Name}");
		}



		private static object CreateTestModelWithStringProperty(Type attributeType, string propertyValue)
		{
			// Constructing them using reflection doesn't add the attributes so we'll manually do so here:
			if (attributeType == typeof(DateTimeRangeAttribute))
			{
				return new DateTimeRangeAttributeOnStringPropertyModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateAttribute))
			{
				return new SqlServerDateAttributeOnStringPropertyModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerDateTimeAttribute))
			{
				return new SqlServerDateTimeAttributeOnStringPropertyModel { PropertyToValidate = propertyValue };
			}

			if (attributeType == typeof(SqlServerSmallDateTimeAttribute))
			{
				return new SqlServerSmallDateTimeAttributeOnStringPropertyModel { PropertyToValidate = propertyValue };
			}

			throw new NotSupportedException($"{nameof(CreateTestModelWithNullableProperty)} is not supported for {attributeType.Name}");
		}

		public static string FormatDynamicDataTestName(MethodInfo sourceTestMethod, object[] testParams)
		{
			if (testParams != null && testParams.Length > 0 && testParams[0] is Type attributeTypeParameter)
			{
				return $"{attributeTypeParameter.Name}_{sourceTestMethod.Name}";
			}

			return sourceTestMethod.Name;
		}

		private static DateTime GetMaximumValueForAttributeType(Type attributeType)
		{
			if (attributeType == typeof(DateTimeRangeAttribute))
			{
				return DateTime.Parse(DateTimeRangeMaximum);
			}

			if (attributeType == typeof(SqlServerDateAttribute))
			{
				return SqlServerDateAttribute.Maximum;
			}

			if (attributeType == typeof(SqlServerDateTimeAttribute))
			{
				return SqlServerDateTimeAttribute.Maximum;
			}

			if (attributeType == typeof(SqlServerSmallDateTimeAttribute))
			{
				return SqlServerSmallDateTimeAttribute.Maximum;
			}

			throw new NotSupportedException($"{nameof(GetMinimumValueForAttributeType)} is not supported for {attributeType.Name}");
		}

		private static DateTime GetMinimumValueForAttributeType(Type attributeType)
		{
			if (attributeType == typeof(DateTimeRangeAttribute))
			{
				return DateTime.Parse(DateTimeRangeMinimum);
			}

			if (attributeType == typeof(SqlServerDateAttribute))
			{
				return SqlServerDateAttribute.Minimum;
			}

			if (attributeType == typeof(SqlServerDateTimeAttribute))
			{
				return SqlServerDateTimeAttribute.Minimum;
			}

			if (attributeType == typeof(SqlServerSmallDateTimeAttribute))
			{
				return SqlServerSmallDateTimeAttribute.Minimum;
			}

			throw new NotSupportedException($"{nameof(GetMinimumValueForAttributeType)} is not supported for {attributeType.Name}");
		}

		public static IEnumerable<object[]> TestData_ValidationAttribute_TypesNames()
			=> DateTimeValidationAttributeTypes.Select(attributeType => new object[] { attributeType }).ToList();

		private static List<ValidationResult> ValidateObject(object modelToValidate)
		{
			var validationContext = new ValidationContext(modelToValidate, null, null);
			var validationResults = new List<ValidationResult>();

			Validator.TryValidateObject(modelToValidate, validationContext, validationResults, true);
			return validationResults;
		}

		private static void VerifyValidationFailsWithExpectedMessage(string memberName, DateTime minimum, DateTime maximum, List<ValidationResult> validationResults)
		{
			validationResults.Count.Should().Be(1);
			var validationFailure = validationResults[0];

			var expectedErrorMessage = string.Format(CultureInfo.CurrentCulture, "The field {0} must be between {1} and {2}.", memberName, minimum, maximum);
			validationFailure.ErrorMessage.Should().Be(expectedErrorMessage);
			validationFailure.MemberNames.Should().BeEquivalentTo(new string[] { memberName });
		}

		private static void VerifyValidationThrowsInvalidCastException(object modelToValidate)
		{
			var validationContext = new ValidationContext(modelToValidate, null, null);
			var validationResults = new List<ValidationResult>();

			Invoking(() => Validator.TryValidateObject(modelToValidate, validationContext, validationResults, true))
				.Should()
				.Throw<InvalidCastException>()
				.WithMessage("The [*] attribute must be used on a DateTime member. [MemberName: *]");
		}

		public class DateTimeRangeAttributeModel
		{
			[DateTimeRange(DateTimeRangeMinimum, DateTimeRangeMaximum)]
			public DateTime PropertyToValidate { get; set; }
		}

		public class DateTimeRangeAttributeOnStringPropertyModel
		{
			[DateTimeRange(DateTimeRangeMinimum, DateTimeRangeMaximum)]
			public string? PropertyToValidate { get; set; }
		}

		public class NullableDateTimeRangeAttributeModel
		{
			[DateTimeRange(DateTimeRangeMinimum, DateTimeRangeMaximum)]
			public DateTime? PropertyToValidate { get; set; }
		}

		public class SqlServerDateAttributeModel
		{
			[SqlServerDate]
			public DateTime PropertyToValidate { get; set; }
		}

		public class SqlServerDateAttributeOnStringPropertyModel
		{
			[SqlServerDate]
			public string? PropertyToValidate { get; set; }
		}

		public class NullableSqlServerDateAttributeModel
		{
			[SqlServerDate]
			public DateTime? PropertyToValidate { get; set; }
		}

		public class SqlServerDateTimeAttributeModel
		{
			[SqlServerDateTime]
			public DateTime PropertyToValidate { get; set; }
		}

		public class SqlServerDateTimeAttributeOnStringPropertyModel
		{
			[SqlServerDateTime]
			public string? PropertyToValidate { get; set; }
		}

		public class NullableSqlServerDateTimeAttributeModel
		{
			[SqlServerDateTime]
			public DateTime? PropertyToValidate { get; set; }
		}

		public class SqlServerSmallDateTimeAttributeModel
		{
			[SqlServerSmallDateTime]
			public DateTime PropertyToValidate { get; set; }
		}

		public class SqlServerSmallDateTimeAttributeOnStringPropertyModel
		{
			[SqlServerSmallDateTime]
			public string? PropertyToValidate { get; set; }
		}

		public class NullableSqlServerSmallDateTimeAttributeModel
		{
			[SqlServerSmallDateTime]
			public DateTime? PropertyToValidate { get; set; }
		}
	}
}
