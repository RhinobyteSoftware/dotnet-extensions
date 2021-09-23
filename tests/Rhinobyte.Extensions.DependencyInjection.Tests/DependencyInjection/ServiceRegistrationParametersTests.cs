using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.DependencyInjection.Tests
{
	[TestClass]
	public class ServiceRegistrationParametersTests
	{
		[TestMethod]
		public void Constructors_throw_ArgumentNullException_for_missing_arguments()
		{
			Invoking(() => new ServiceRegistrationParameters(serviceDescriptor: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*serviceDescriptor*");

			Invoking(() => new ServiceRegistrationParameters(serviceDescriptors: null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null.*serviceDescriptors*");
		}
	}
}
