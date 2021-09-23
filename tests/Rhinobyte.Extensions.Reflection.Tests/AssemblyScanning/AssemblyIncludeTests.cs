using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.AssemblyScanning;
using System;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.AssemblyScanning
{
	[TestClass]
	public class AssemblyIncludeTests
	{
		[TestMethod]
		public void Constructor_throws_ArgumentNullException_for_a_null_assembly_argument()
		{
			Invoking(() => new AssemblyInclude(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*assemblyToInclude*");
		}

		[TestMethod]
		public void Equals_returns_the_expected_result()
		{
			var assemblyInclude1 = new AssemblyInclude(typeof(AssemblyIncludeTests).Assembly);
			assemblyInclude1.Equals(null).Should().BeFalse();
			assemblyInclude1.Equals(assemblyInclude1).Should().BeTrue();

			var assemblyInclude2 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			assemblyInclude1.Equals(assemblyInclude2).Should().BeFalse();
			assemblyInclude2.Equals(assemblyInclude1).Should().BeFalse();

			var assemblyInclude3 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			assemblyInclude3.Equals(assemblyInclude2).Should().BeTrue();
			assemblyInclude2.Equals(assemblyInclude3).Should().BeTrue();
		}

		[TestMethod]
		public void GetHashCode_returns_the_expected_result()
		{
			var assemblyInclude1 = new AssemblyInclude(typeof(AssemblyIncludeTests).Assembly);
			assemblyInclude1.GetHashCode().Should().Be(typeof(AssemblyIncludeTests).Assembly.GetHashCode());

			var assemblyInclude2 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			assemblyInclude2.GetHashCode().Should().Be(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly.GetHashCode());

			var assemblyInclude3 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			assemblyInclude3.GetHashCode().Should().Be(assemblyInclude2.GetHashCode());
		}

		[TestMethod]
		public void IsEqualTo_operator_returns_the_expected_result()
		{
			var assemblyInclude1 = new AssemblyInclude(typeof(AssemblyIncludeTests).Assembly);
#pragma warning disable CS8073 // Expression is always true
			(assemblyInclude1 == null).Should().BeFalse();
			(null == assemblyInclude1).Should().BeFalse();
#pragma warning restore CS8073 // Expression is always true
#pragma warning disable CS1718 // Comparison made to same variable
			(assemblyInclude1 == assemblyInclude1).Should().BeTrue();
#pragma warning restore CS1718 // Comparison made to same variable

			var assemblyInclude2 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			(assemblyInclude1 == assemblyInclude2).Should().BeFalse();
			(assemblyInclude2 == assemblyInclude1).Should().BeFalse();

			var assemblyInclude3 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			(assemblyInclude2 == assemblyInclude3).Should().BeTrue();
			(assemblyInclude3 == assemblyInclude2).Should().BeTrue();
		}

		[TestMethod]
		public void IsNotEqualTo_operator_returns_the_expected_result()
		{
			var assemblyInclude1 = new AssemblyInclude(typeof(AssemblyIncludeTests).Assembly);
#pragma warning disable CS8073 // Expression is always true
			(assemblyInclude1 != null).Should().BeTrue();
			(null != assemblyInclude1).Should().BeTrue();
#pragma warning restore CS8073 // Expression is always true
#pragma warning disable CS1718 // Comparison made to same variable
			(assemblyInclude1 != assemblyInclude1).Should().BeFalse();
#pragma warning restore CS1718 // Comparison made to same variable

			var assemblyInclude2 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			(assemblyInclude1 != assemblyInclude2).Should().BeTrue();
			(assemblyInclude2 != assemblyInclude1).Should().BeTrue();

			var assemblyInclude3 = new AssemblyInclude(typeof(Rhinobyte.Extensions.Reflection.AssemblyScanning.AssemblyInclude).Assembly);
			(assemblyInclude2 != assemblyInclude3).Should().BeFalse();
			(assemblyInclude3 != assemblyInclude2).Should().BeFalse();
		}
	}
}
