using System;

namespace ExampleLibrary1
{
	public interface IManuallyConfiguredType
	{
	}

	public class ManuallyConfiguredType : IManuallyConfiguredType
	{
		public ManuallyConfiguredType(Uri someUri) { }
	}
}
