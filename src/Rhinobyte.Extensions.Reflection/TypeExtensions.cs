using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rhinobyte.Extensions.Reflection
{
	public static class TypeExtensions
	{
		internal static string BuildGenericConstraintDisplayValue(string typeArgumentName, Type[] typeConstraints, bool useFullTypeName)
		{
			var genericConstraintBuilder = new StringBuilder()
				.Append("where ")
				.Append(typeArgumentName)
				.Append(" : ");

			var constraintIndex = 0;
			foreach (var constraintType in typeConstraints)
			{
				++constraintIndex;
				if (constraintType is null) { continue; }

				var genericArgumentIndex = 0;
				var constraintName = constraintType.GetDisplayName(constraintType.CustomAttributes, null, null, useFullTypeName, ref genericArgumentIndex);

				_ = genericConstraintBuilder.Append(constraintName);
				if (constraintIndex < typeConstraints.Length)
				{
					_ = genericConstraintBuilder.Append(", ");
				}
			}

			return genericConstraintBuilder.ToString().Trim();
		}

		/// <summary>
		/// Returns a common (code like) type name for many types.
		/// <para>For example:</para>
		/// <para>
		/// <c>typeof(bool?).GetCommonTypeName()</c> will return "bool?" where as <c>typeof(bool?).GetType().Name</c> might return "Nullable`&lt;Boolean&gt;"
		/// </para>
		/// </summary>
		public static string? GetCommonTypeName(this Type? type, bool isNullable = false)
		{
			if (type is null)
				return string.Empty;

			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType is null && isNullable)
			{
				underlyingType = type;
			}

			if (underlyingType != null)
			{
				var commonTypeName = GetCommonTypeNameInternal(underlyingType);
				return commonTypeName is null ? null : commonTypeName + '?';
			}

			return GetCommonTypeNameInternal(type);
		}

		public static string? GetCommonTypeNameInternal(Type type)
		{
			if (type == typeof(void))
				return "void";

			if (type == typeof(bool))
				return "bool";

			if (type == typeof(byte))
				return "byte";

			if (type == typeof(sbyte))
				return "sbyte";

			if (type == typeof(short))
				return "short";

			if (type == typeof(ushort))
				return "ushort";

			if (type == typeof(int))
				return "int";

			if (type == typeof(uint))
				return "uint";

			if (type == typeof(long))
				return "long";

			if (type == typeof(ulong))
				return "ulong";

			if (type == typeof(float))
				return "float";

			if (type == typeof(double))
				return "double";

			if (type == typeof(decimal))
				return "decimal";

			if (type == typeof(string))
				return "string";

			if (type == typeof(object))
				return "object";

			if (type == typeof(ValueType))
				return "struct";

			return null;
		}

		public static MemberInfo? GetDeclaringMember(this Type? type)
		{
			if (type is null) return null;

#if NETSTANDARD2_1
			if (type.IsGenericMethodParameter)
			{
				return type.DeclaringMethod;
			}

			return type.DeclaringType;
#else
			try
			{
				var declaringMethod = type.DeclaringMethod;
				if (declaringMethod != null)
				{
					return declaringMethod;
				}
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types

			try
			{
				return type.DeclaringType;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception) { }
#pragma warning restore CA1031 // Do not catch general exception types

			return null;
#endif
		}

		public static string GetDisplayName(
			this Type? type,
			IEnumerable<CustomAttributeData>? customAttributes = null,
			MemberInfo? declaringMember = null,
			bool useFullTypeName = false)
		{
			var nullableAttributeIndex = 0;
			return GetDisplayName(type, customAttributes ?? type?.CustomAttributes, declaringMember ?? type?.GetDeclaringMember(), null, useFullTypeName, ref nullableAttributeIndex);
		}

		public static string GetDisplayName(
			this Type? type,
			IEnumerable<CustomAttributeData>? customAttributes,
			MemberInfo? declaringMember,
			ICollection<string>? genericConstraints,
			bool useFullTypeName,
			ref int nullableAttributeIndex)
		{
			if (type is null)
				return string.Empty;

			var isNullable = IsNullableType(customAttributes, declaringMember, type, nullableAttributeIndex);

			if (!type.IsGenericType)
			{
				var displayName = GetCommonTypeNameInternal(type) ?? (useFullTypeName ? type.FullName ?? type.Name : type.Name);
				return isNullable
					? displayName + '?'
					: displayName;
			}

			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType != null)
			{
				var displayName = GetCommonTypeNameInternal(underlyingType) ?? (useFullTypeName ? underlyingType.FullName ?? underlyingType.Name : underlyingType.Name);
				return displayName + '?';
			}

			var displayNameBuilder = new StringBuilder(useFullTypeName ? type.FullName ?? type.Name : type.Name);

			var characterIndexToRemove = displayNameBuilder.Length - 1;
			if (useFullTypeName)
			{
				while (displayNameBuilder[characterIndexToRemove] == ']' && displayNameBuilder[characterIndexToRemove - 1] == ']')
				{
					--characterIndexToRemove;
					var didRemove = false;
					while (characterIndexToRemove > 1)
					{
						--characterIndexToRemove;
						if (displayNameBuilder[characterIndexToRemove] == '[' && displayNameBuilder[characterIndexToRemove - 1] == '[')
						{
							--characterIndexToRemove;
							_ = displayNameBuilder.Remove(characterIndexToRemove, displayNameBuilder.Length - characterIndexToRemove);
							didRemove = true;
						}
					}

					if (!didRemove)
					{
						break;
					}
				}
			}

			characterIndexToRemove = displayNameBuilder.Length - 1;
			while (characterIndexToRemove > 0 && char.IsDigit(displayNameBuilder[characterIndexToRemove]))
			{
				--characterIndexToRemove;
				if (displayNameBuilder[characterIndexToRemove] == '`')
				{
					_ = displayNameBuilder.Remove(characterIndexToRemove, displayNameBuilder.Length - characterIndexToRemove);
					break;
				}
			}

			_ = displayNameBuilder.Append('<');

			var typeArguments = type.GetGenericArguments();
			if (typeArguments?.Length > 0)
			{
				var typeArgumentIndex = 0;
				foreach (var typeArgument in typeArguments)
				{
					++typeArgumentIndex;
					++nullableAttributeIndex;
					var typeArgumentName = typeArgument.GetDisplayName(customAttributes, declaringMember, genericConstraints, useFullTypeName, ref nullableAttributeIndex);
					_ = displayNameBuilder.Append(typeArgumentName);

					if (typeArgumentIndex < typeArguments.Length)
					{
						_ = displayNameBuilder.Append(", ");
					}

					if (typeArgument.IsGenericParameter && genericConstraints != null)
					{
						var constraints = typeArgument.GetGenericParameterConstraints();
						if (constraints?.Length > 0)
						{
							genericConstraints.Add(BuildGenericConstraintDisplayValue(typeArgumentName, constraints, useFullTypeName));
						}
					}
				}
			}

			_ = displayNameBuilder.Append('>');
			if (isNullable)
			{
				_ = displayNameBuilder.Append('?');
			}

			return displayNameBuilder.ToString().Trim();
		}

		/// <summary>
		/// Convenience extension method to check if the <paramref name="type"/> is decorated with the <see cref="System.Runtime.CompilerServices.CompilerGeneratedAttribute"/>
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns>true if <paramref name="type"/> is compiler generated, false otherwise</returns>
		public static bool IsCompilerGenerated(this Type type)
		{
			if (type is null)
				return false;

			return type.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);
		}

		/// <summary>
		/// See <see href="https://stackoverflow.com/questions/58453972/how-to-use-net-reflection-to-check-for-nullable-reference-type#58454489"/>
		/// </summary>
		/// <param name="customAttributes">The custom attribute data of the member</param>
		/// <param name="declaringType">The declaring type to check a NullableContextAttribute</param>
		/// <param name="memberType">The type of the member</param>
		/// <param name="nullableAttributeIndex">The index of the generic argument in the NullableAttribute byte[]</param>
		internal static bool IsNullableType(IEnumerable<CustomAttributeData>? customAttributes, MemberInfo? declaringType, Type memberType, int nullableAttributeIndex)
		{
			if (memberType.IsValueType)
				return Nullable.GetUnderlyingType(memberType) != null;

			return IsNullableReferenceType(customAttributes, declaringType, memberType, nullableAttributeIndex);
		}

		/// <summary>
		/// See <see href="https://stackoverflow.com/questions/58453972/how-to-use-net-reflection-to-check-for-nullable-reference-type#58454489"/>
		/// </summary>
		/// <param name="customAttributes">The custom attribute data of the member</param>
		/// <param name="declaringType">The declaring type to check a NullableContextAttribute</param>
		/// <param name="memberType">The type of the member</param>
		/// <param name="nullableAttributeIndex">The index of the generic argument in the NullableAttribute byte[]</param>
		internal static bool IsNullableReferenceType(IEnumerable<CustomAttributeData>? customAttributes, MemberInfo? declaringMember, Type memberType, int nullableAttributeIndex)
		{
			var nullable = customAttributes?.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");

			if (nullable != null && nullable.ConstructorArguments.Count == 1)
			{
				var attributeArgument = nullable.ConstructorArguments[0];
				if (attributeArgument.ArgumentType == typeof(byte[]))
				{
					var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
					if (args.Count > nullableAttributeIndex && args[nullableAttributeIndex].ArgumentType == typeof(byte))
					{
						return (byte)args[nullableAttributeIndex].Value! == 2;
					}
				}
				else if (attributeArgument.ArgumentType == typeof(byte))
				{
					return (byte)attributeArgument.Value! == 2;
				}
			}

			if (memberType.CustomAttributes != null && memberType.CustomAttributes != customAttributes)
			{
				nullable = memberType.CustomAttributes
					.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
			}

			if (nullable != null && nullable.ConstructorArguments.Count == 1)
			{
				var attributeArgument = nullable.ConstructorArguments[0];
				if (attributeArgument.ArgumentType == typeof(byte[]))
				{
					var args = (ReadOnlyCollection<CustomAttributeTypedArgument>)attributeArgument.Value!;
					if (args.Count > 0 && args[0].ArgumentType == typeof(byte))
					{
						return (byte)args[0].Value! == 2;
					}
				}
				else if (attributeArgument.ArgumentType == typeof(byte))
				{
					return (byte)attributeArgument.Value! == 2;
				}
			}

			if (declaringMember is null)
				declaringMember = memberType.GetDeclaringMember();

			for (var nextDeclaringMember = declaringMember; nextDeclaringMember != null; nextDeclaringMember = nextDeclaringMember.DeclaringType)
			{
				var context = nextDeclaringMember.CustomAttributes
					.FirstOrDefault(x => x.AttributeType.FullName == "System.Runtime.CompilerServices.NullableContextAttribute");

				if (context != null &&
					context.ConstructorArguments.Count == 1 &&
					context.ConstructorArguments[0].ArgumentType == typeof(byte))
				{
					return (byte)context.ConstructorArguments[0].Value! == 2;
				}
			}

			// Couldn't find a suitable attribute
			return false;
		}

		/// <summary>
		/// Convenience extension method to determine <paramref name="type"/> is an open generic type
		/// </summary>
		public static bool IsOpenGeneric(this Type type)
		{
			if (type is null)
				return false;

			return type.IsGenericTypeDefinition || type.ContainsGenericParameters;
		}

		/// <summary>
		/// Convenience extension method to determine <paramref name="type"/> is either a <see cref="ValueType"/> or the <see cref="string"/> type
		/// </summary>
		public static bool IsValueTypeOrString(this Type type)
		{
			if (type is null)
				return false;

			return type.IsValueType || type == typeof(string);
		}
	}
}
