using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel.DataAnnotations;

using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.CommandLine.Tests;

[TestClass]
public class AdvancedParserTests
{
	[TestMethod]
	public void ParseCommandLineOptions_does_not_throw_for_unknown_args_when_ThrowOnOptionsParserErrors_is_false()
	{
		var advancedParserOptions = new AdvancedParserOptions();
		var parser = new AdvancedParser<TestOptions>(advancedParserOptions);

		var testArguments = new string[] { "/intone=1", "--int-two", "5", "/IsEnabled=true", "--string-option-one", "Some String", "--unknown-one", "unknownonevalue", "--unknown-two", "-u", "/unknownfour", "unknownfourvalue", "/unknownfive=555" };
		var parsedOptions = parser.ParseCommandLineOptions(testArguments);

		parsedOptions.Should().NotBeNull().And.BeOfType<TestOptions>();

		parsedOptions.IntegerOptionOne.Should().Be(1);
		parsedOptions.IntegerOptionTwo.Should().Be(5);
		parsedOptions.IsEnabled.Should().BeTrue();
		parsedOptions.StringOptionOne.Should().Be("Some String");
		parsedOptions.StringOptionTwo.Should().BeNull();
	}

	[TestMethod]
	public void ParseCommandLineOptions_returns_the_expected_result()
	{
		var advancedParserOptions = new AdvancedParserOptions();
		var parser = new AdvancedParser<TestOptions>(advancedParserOptions);

		var testArguments = new string[] { "/intone=1", "--int-two", "5", "/IsEnabled=true", "--string-option-one", "Some String" };
		var parsedOptions = parser.ParseCommandLineOptions(testArguments);

		parsedOptions.Should().NotBeNull().And.BeOfType<TestOptions>();

		parsedOptions.IntegerOptionOne.Should().Be(1);
		parsedOptions.IntegerOptionTwo.Should().Be(5);
		parsedOptions.IsEnabled.Should().BeTrue();
		parsedOptions.StringOptionOne.Should().Be("Some String");
		parsedOptions.StringOptionTwo.Should().BeNull();
	}

	[TestMethod]
	public void ParseCommandLineOptions_throws_for_unknown_args_when_ThrowOnOptionsParserErrors_is_true()
	{
		var advancedParserOptions = new AdvancedParserOptions { ThrowOnOptionsParserErrors = true };
		var parser = new AdvancedParser<TestOptions>(advancedParserOptions);

		var testArguments = new string[] { "/intone=1", "--int-two", "5", "/IsEnabled=true", "--string-option-one", "Some String", "--unknown-one", "unknownonevalue", "--unknown-two", "-u", "/unknownfour", "unknownfourvalue", "/unknownfive=555" };
		Invoking(() => parser.ParseCommandLineOptions(testArguments))
			.Should()
			.Throw<ParseOptionsException>()
			.WithMessage("The command line options parse result contains errors:*");
	}

	[TestMethod]
	public void ParseCommandLineOptions_throws_when_a_required_option_is_not_set()
	{
		var advancedParserOptions = new AdvancedParserOptions();
		var parser = new AdvancedParser<TestOptions>(advancedParserOptions);

		var testArguments = new string[] { "/intone=1", "--int-two", "5", "/IsEnabled=true" };
		Invoking(() => parser.ParseCommandLineOptions(testArguments))
			.Should()
			.Throw<ParseOptionsException>()
			.WithMessage("The following required options have not been provided:*");
	}

	public class TestOptions
	{
		[Required]
		[Option(Aliases = ["--int-one", "/intone"], Description = "[Required] Integer option one")]
		public int IntegerOptionOne { get; set; }

		[Option(Aliases = ["--int-two"], Description = "[Optional] Integer option two")]
		public int IntegerOptionTwo { get; set; }

		public bool IsEnabled { get; set; }

		public bool? NullableBoolOption { get; set; }

		[Required]
		[Option(Aliases = ["--string-option-one", "-s", "/stringoptionone"], Description = "[Required] String option one")]
		public string StringOptionOne { get; set; } = string.Empty;

		[Option(Aliases = ["--string-option-two", "-t", "/stringoptiontwo"], Description = "[Optional] String option two")]
		public string StringOptionTwo { get; set; } = string.Empty;


		public string NotSettable => string.Empty;
	}
}
