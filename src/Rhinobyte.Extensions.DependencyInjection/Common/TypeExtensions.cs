﻿using System;

namespace Rhinobyte.Extensions.DependencyInjection
{
	public static class TypeExtensions
	{
		public static bool IsCompilerGenerated(this Type type)
		{
			if (type is null)
				return false;

			return type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
		}

		public static bool IsOpenGeneric(this Type type)
		{
			if (type is null)
				return false;

			return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
		}

		public static bool IsValueTypeOrString(this Type type)
		{
			if (type is null)
				return false;

			return type.IsValueType || type == typeof(string);
		}
	}
}