using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rhinobyte.ReflectionHelpers
{
	public static class TypeExtensions
	{
		internal static string BuildGenericConstraintDisplayValue(string typeArgumentName, Type[] typeConstraints, bool useFullTypeName)
		{
			var genericConstraintBuilder = new StringBuilder();
			genericConstraintBuilder
				.Append("where ")
				.Append(typeArgumentName)
				.Append(" : ");

			var constraintIndex = 0;
			foreach (var constraintType in typeConstraints)
			{
				++constraintIndex;
				if (constraintType == null) { continue; }

				var genericArgumentIndex = 0;
				var constraintName = constraintType.GetDisplayName(constraintType.CustomAttributes, null, null, useFullTypeName, ref genericArgumentIndex);

				genericConstraintBuilder.Append(constraintName);
				if (constraintIndex < typeConstraints.Length)
				{
					genericConstraintBuilder.Append(", ");
				}
			}

			return genericConstraintBuilder.ToString().Trim();
		}

		public static string? GetCommonTypeName(this Type? type, bool isNullable = false)
		{
			if (type is null)
				return string.Empty;

			var underlyingType = Nullable.GetUnderlyingType(type);
			if (underlyingType == null && isNullable)
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

			if (type == typeof(System.ValueType))
				return "struct";

			return null;
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
							displayNameBuilder.Remove(characterIndexToRemove, displayNameBuilder.Length - characterIndexToRemove);
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
					displayNameBuilder.Remove(characterIndexToRemove, displayNameBuilder.Length - characterIndexToRemove);
					break;
				}
			}

			displayNameBuilder.Append('<');

			var typeArguments = type.GetGenericArguments();
			if (typeArguments?.Length > 0)
			{
				var typeArgumentIndex = 0;
				foreach (var typeArgument in typeArguments)
				{
					++typeArgumentIndex;
					++nullableAttributeIndex;
					var typeArgumentName = typeArgument.GetDisplayName(customAttributes, declaringMember, genericConstraints, useFullTypeName, ref nullableAttributeIndex);
					displayNameBuilder.Append(typeArgumentName);

					if (typeArgumentIndex < typeArguments.Length)
					{
						displayNameBuilder.Append(", ");
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

			displayNameBuilder.Append('>');
			if (isNullable)
			{
				displayNameBuilder.Append('?');
			}

			return displayNameBuilder.ToString().Trim();
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
			catch (Exception) { }

			try
			{
				return type.DeclaringType;
			}
			catch (Exception) { }

			return null;
#endif
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
					if (args.Count > 0 && args[nullableAttributeIndex].ArgumentType == typeof(byte))
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

			if (declaringMember == null)
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
	}
}
