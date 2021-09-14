using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public interface ICustomServiceDescriptor
	{
		Type GetImplementationType();
	}
}
