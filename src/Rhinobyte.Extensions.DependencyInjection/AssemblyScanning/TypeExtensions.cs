using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public static class TypeExtensions
	{
		public static bool IsOpenGeneric(this Type type)
		{
			if (type is null)
				return false;

			return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
		}
	}
}
