using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rhinobyte.Extensions.Reflection
{
	/// <summary>
	/// Reflection helper extension methods for <see cref="MethodBase"/> to support parsing or searching of the method body's intermediate language (IL) bytes.
	/// </summary>
	public static class MethodBaseExtensions
	{

		/// <summary>
		/// Search the <paramref name="methodBase"/> body's intermediate language (IL) bytes for an instruction reference to the specified <paramref name="memberReferenceToLookFor"/>.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferenceToLookFor">The <see cref="MemberInfo"/> reference to look for</param>
		/// <returns><see cref="true"/> if a reference is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodBase"/> or <paramref name="memberReferenceToLookFor"/> are null</exception>
		public static bool ContainsReferenceTo(this MethodBase methodBase, MemberInfo memberReferenceToLookFor)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			_ = memberReferenceToLookFor ?? throw new ArgumentNullException(nameof(memberReferenceToLookFor));
			return new MethodBodyParser(methodBase).ContainsReferenceTo(memberReferenceToLookFor);
		}

		/// <summary>
		/// Search the <paramref name="methodBase"/> body's intermediate language (IL) bytes for instruction references to all of the specified <paramref name="memberReferencesToLookFor"/>.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferencesToLookFor">The set of <see cref="MemberInfo"/> references to look for</param>
		/// <returns><see cref="true"/> if at least one instruction reference to each of the search items is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodBase"/> or <paramref name="memberReferencesToLookFor"/> are null</exception>
		public static bool ContainsReferencesToAll(this MethodBase methodBase, IEnumerable<MemberInfo> memberReferencesToLookFor)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			_ = memberReferencesToLookFor ?? throw new ArgumentNullException(nameof(memberReferencesToLookFor));
			return new MethodBodyParser(methodBase).ContainsReferencesToAll(memberReferencesToLookFor);
		}

		/// <summary>
		/// Search the <paramref name="methodBase"/> body's intermediate language (IL) bytes for an instruction reference to any of the specified <paramref name="memberReferencesToLookFor"/>.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferencesToLookFor">The set of <see cref="MemberInfo"/> references to look for</param>
		/// <returns><see cref="true"/> if a reference to any of the items is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodBase"/> or <paramref name="memberReferencesToLookFor"/> are null</exception>
		public static bool ContainsReferenceToAny(this MethodBase methodBase, IEnumerable<MemberInfo> memberReferencesToLookFor)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			_ = memberReferencesToLookFor ?? throw new ArgumentNullException(nameof(memberReferencesToLookFor));
			return new MethodBodyParser(methodBase).ContainsReferenceToAny(memberReferencesToLookFor);
		}

		/// <summary>
		/// Return a string description of the IL instructions for the <paramref name="methodBase"/> body.
		/// An optional <paramref name="instructionFormatter"/> can be specified to control the description format.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> body to describe</param>
		/// <param name="instructionFormatter">[Optional] An <see cref="IInstructionFormatter"/> to control the description formatting for the instructions</param>
		/// <returns>The formatted description of the method body IL instructions</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodBase"/> argument is null</exception>
		public static string DescribeInstructions(this MethodBase methodBase, IInstructionFormatter? instructionFormatter = null)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			var methodInstructions = new MethodBodyParser(methodBase).ParseInstructions();

			return (instructionFormatter ?? new DefaultInstructionFormatter()).DescribeInstructions(methodInstructions);
		}

		/// <summary>
		/// Return the access level of the method. (e.g. <c>"public"</c>, <c>"protected"</c>, etc)
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to check</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodBase"/> argument is null</exception>
		public static string GetAccessLevel(this MethodBase methodBase)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			if (methodBase.IsPublic)
			{
				return "public";
			}

			if (methodBase.IsFamilyOrAssembly)
			{
				return "protected internal";
			}

			if (methodBase.IsFamily)
			{
				return "protected";
			}

			if (methodBase.IsAssembly)
			{
				return "internal";
			}

			if (methodBase.IsPrivate)
			{
				return "private";
			}

			return string.Empty;
		}

		/// <summary>
		/// Return a string representation of the <paramref name="methodBase"/>'s signature.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> to get the signature for</param>
		/// <param name="useFullTypeName">[Optional] Whether or not to use the full type names in the signature</param>
		/// <returns>
		/// The method signature, for example:
		/// <code>
		/// public static string GetSignature(MethodBase methodBase, bool useFullTypeName)
		/// </code>
		/// </returns>
		public static string GetSignature(this MethodBase methodBase, bool useFullTypeName = false)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));

			var stringBuilder = new StringBuilder(methodBase.GetAccessLevel());
			var genericConstraints = new List<string>();

			if (methodBase.IsStatic)
			{
				stringBuilder.Append(" static");
			}

			var methodInfo = methodBase as MethodInfo;
			if (methodInfo != null && methodInfo.IsOverride())
			{
				stringBuilder.Append(" override");
			}
			else if (methodBase.IsAbstract)
			{
				stringBuilder.Append(" abstract");
			}
			else if (methodBase.IsVirtual)
			{
				stringBuilder.Append(" virtual");
			}

			if (methodBase.IsAsync())
			{
				stringBuilder.Append(" async");
			}

			stringBuilder.Append(' ');
			if (methodInfo is null || methodInfo.ReturnType is null)
			{
				stringBuilder.Append("<ReturnType>");
			}
			else
			{
				var nullableAttributeIndex = 0;
				stringBuilder.Append(methodInfo.ReturnType.GetDisplayName(methodInfo.ReturnParameter.CustomAttributes, methodInfo.ReturnParameter.Member, genericConstraints, useFullTypeName, ref nullableAttributeIndex));
			}

			stringBuilder.Append(' ');
			if (useFullTypeName && methodBase.DeclaringType != null)
			{
				var nullableAttributeIndex = 0;
				stringBuilder.Append(methodBase.DeclaringType.GetDisplayName(methodBase.DeclaringType.CustomAttributes, methodBase.DeclaringType.GetDeclaringMember(), genericConstraints, true, ref nullableAttributeIndex)).Append('.');
			}

			stringBuilder.Append(methodBase.Name);

			var genericArguments = methodInfo?.GetGenericArguments();
			if (genericArguments?.Length > 0)
			{
				stringBuilder.Append('<');
				var genericArgumentIndex = 0;
				foreach (var genericArgument in genericArguments)
				{
					++genericArgumentIndex;

					var nullableAttributeIndex = 0;
					var genericArgumentTypeName = genericArgument.GetDisplayName(genericArgument.CustomAttributes, genericArgument.GetDeclaringMember(), genericConstraints, useFullTypeName, ref nullableAttributeIndex);
					stringBuilder.Append(genericArgumentTypeName);
					if (genericArgumentIndex < genericArguments.Length)
					{
						stringBuilder.Append(", ");
					}

					try
					{
						var constraints = genericArgument.GetGenericParameterConstraints();
						if (constraints?.Length > 0)
						{
							genericConstraints.Add(TypeExtensions.BuildGenericConstraintDisplayValue(genericArgumentTypeName, constraints, useFullTypeName));
						}
					}
					catch (Exception)
					{
						// GetGenericParameterConstraints() will throw for open generics even those IsGenericType will be true and IsGenericType can be false for some
						// method argument types that can have constraints... *sigh*
					}
				}

				stringBuilder.Append('>');
			}

			stringBuilder.Append('(');

			var methodParameters = methodBase.GetParameters();
			if (methodParameters.Length > 0)
			{
				var methodParameterIndex = 0;
				foreach (var methodParameter in methodParameters)
				{
					++methodParameterIndex;
					var isNullable = methodParameter.IsNullableType();

					var nullableAttributeIndex = 0;
					stringBuilder
						.Append(methodParameter.ParameterType.GetDisplayName(methodParameter.CustomAttributes, methodParameter.Member, genericConstraints, useFullTypeName, ref nullableAttributeIndex))
						.Append(' ')
						.Append(methodParameter.Name);

					if (methodParameterIndex < methodParameters.Length)
					{
						stringBuilder.Append(", ");
					}
				}
			}

			stringBuilder.Append(')');

			if (genericConstraints.Count == 1)
			{
				stringBuilder.Append(' ').Append(genericConstraints[0]);
			}
			else if (genericConstraints.Count > 1)
			{
				foreach (var constraintString in genericConstraints)
				{
					stringBuilder.Append(Environment.NewLine).Append(constraintString);
				}
			}

			return stringBuilder.ToString().Trim();
		}

		/// <summary>
		/// Convenience extension method to check if <paramref name="methodBase"/> has parameters matching the length, order, and type of <paramref name="parameterTypes"/>.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to check</param>
		/// <param name="parameterTypes">The array of parameter <see cref="Type"/>s to compare against</param>
		/// <returns><see cref="true"/> if the method parameters match <paramref name="parameterTypes"/>, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either of the <paramref name="methodBase"/> or <paramref name="parameterTypes"/> arguments are null</exception>
		public static bool HasMatchingParameterTypes(this MethodBase methodBase, Type[] parameterTypes)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			_ = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));

			var methodParameters = methodBase.GetParameters();
			if (methodParameters.Length != parameterTypes.Length)
			{
				return false;
			}

			for (var parameterIndex = 0; parameterIndex < methodParameters.Length; ++parameterIndex)
			{
				if (methodParameters[parameterIndex].ParameterType != parameterTypes[parameterIndex])
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Convenience extension method to check if <paramref name="methodBase"/> has parameters.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to check</param>
		/// <returns><see cref="true"/> if the method has zero parameters, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodBase"/> is null</exception>
		public static bool HasNoParameters(this MethodBase methodBase)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			return methodBase.GetParameters().Length == 0;
		}

		/// <summary>
		/// Return <c>true</c> if the <paramref name="methodBase"/> is an async method, false otherwise.
		/// </summary>
		/// <remarks>
		/// Checks for the presence of a <see cref="AsyncStateMachineAttribute"/> custom attribute to determine that
		/// a method is async.
		/// </remarks>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to check</param>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodBase"/> argument is null</exception>
		public static bool IsAsync(this MethodBase methodBase)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			return methodBase.IsDefined(typeof(AsyncStateMachineAttribute), true);
		}

		/// <summary>
		/// Return true if the <paramref name="methodInfo"/> is an override of a base method, false otherwise.
		/// </summary>
		/// <param name="methodInfo">The method to check for an override definition</param>
		public static bool IsOverride(this MethodInfo methodInfo)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			return methodInfo.GetBaseDefinition().DeclaringType != methodInfo.DeclaringType;
		}

		/// <summary>
		/// Parse the <paramref name="methodBase"/> body's intermediate language (IL) bytes into a collection of <see cref="InstructionBase"/> instances.
		/// </summary>
		/// <param name="methodBase">The <see cref="MethodBase"/> instance to search within</param>
		/// <returns>The <see cref="IReadOnlyCollection{InstructionBase}"/> of <see cref="InstructionBase"/> parse results</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodBase"/> argument is null</exception>
		public static IReadOnlyCollection<InstructionBase> ParseInstructions(this MethodBase methodBase)
		{
			_ = methodBase ?? throw new ArgumentNullException(nameof(methodBase));
			return new MethodBodyParser(methodBase).ParseInstructions();
		}
	}
}
