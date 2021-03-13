using System;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers
{
	public static class ParameterInfoExtensions
	{
		/// <summary>
		/// Returns true if the <paramref name="parameterInfo"/> represents a <see cref="Nullable{}"/> value type or a nullable reference type, false otherwise.
		/// </summary>
		/// <param name="parameterInfo">The parameterInfo to check for a nullable type</param>
		public static bool IsNullableType(this ParameterInfo parameterInfo)
		{
			_ = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
			return TypeExtensions.IsNullableType(parameterInfo.CustomAttributes, parameterInfo.Member, parameterInfo.ParameterType, 0);
		}

		/// <summary>
		/// Returns true if the <paramref name="parameterInfo"/> represents a nullable reference type, false otherwise.
		/// </summary>
		/// <remarks>
		/// Returns false for a <see cref="Nullable{}"/> value type. Use this method when you want to check explicitly for a
		/// nullable <c>reference</c> type.
		/// </remarks>
		/// <param name="parameterInfo">The parameterInfo to check for a nullable type</param>
		public static bool IsNullableReferenceType(this ParameterInfo parameterInfo)
		{
			_ = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
			return TypeExtensions.IsNullableReferenceType(parameterInfo.CustomAttributes, parameterInfo.Member, parameterInfo.ParameterType, 0);
		}
	}
}
