using Rhinobyte.ReflectionHelpers.Instructions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.ReflectionHelpers
{
	/// <summary>
	/// Reflection helper extension methods for <see cref="MethodBase"/> to support parsing or searching of the method body's intermediate language (IL) bytes.
	/// </summary>
	public static class MethodBaseExtensions
	{
		/// <summary>
		/// Search the <paramref name="methodInfo"/> body's intermediate language (IL) bytes for instruction references to all of the specified <paramref name="memberReferencesToLookFor"/>.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferencesToLookFor">The set of <see cref="MemberInfo"/> references to look for</param>
		/// <returns><see cref="true"/> if at least one instruction reference to each of the search items is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodInfo"/> or <paramref name="memberReferencesToLookFor"/> are null</exception>
		public static bool ContainsReferencesToAll(this MethodBase methodInfo, IEnumerable<MemberInfo> memberReferencesToLookFor)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			_ = memberReferencesToLookFor ?? throw new ArgumentNullException(nameof(memberReferencesToLookFor));
			return new MethodBodyParser(methodInfo).ContainsReferencesToAll(memberReferencesToLookFor);
		}

		/// <summary>
		/// Search the <paramref name="methodInfo"/> body's intermediate language (IL) bytes for an instruction reference to the specified <paramref name="memberReferenceToLookFor"/>.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferenceToLookFor">The <see cref="MemberInfo"/> reference to look for</param>
		/// <returns><see cref="true"/> if a reference is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodInfo"/> or <paramref name="memberReferenceToLookFor"/> are null</exception>
		public static bool ContainsReferenceTo(this MethodBase methodInfo, MemberInfo memberReferenceToLookFor)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			_ = memberReferenceToLookFor ?? throw new ArgumentNullException(nameof(memberReferenceToLookFor));
			return new MethodBodyParser(methodInfo).ContainsReferenceTo(memberReferenceToLookFor);
		}

		/// <summary>
		/// Search the <paramref name="methodInfo"/> body's intermediate language (IL) bytes for an instruction reference to any of the specified <paramref name="memberReferencesToLookFor"/>.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to search within</param>
		/// <param name="memberReferencesToLookFor">The set of <see cref="MemberInfo"/> references to look for</param>
		/// <returns><see cref="true"/> if a reference to any of the items is found, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either <paramref name="methodInfo"/> or <paramref name="memberReferencesToLookFor"/> are null</exception>
		public static bool ContainsReferenceToAny(this MethodBase methodInfo, IEnumerable<MemberInfo> memberReferencesToLookFor)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			_ = memberReferencesToLookFor ?? throw new ArgumentNullException(nameof(memberReferencesToLookFor));
			return new MethodBodyParser(methodInfo).ContainsReferenceToAny(memberReferencesToLookFor);
		}

		/// <summary>
		/// Convenience extension method to check if <paramref name="methodInfo"/> has parameters matching the length, order, and type of <paramref name="parameterTypes"/>.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to check</param>
		/// <param name="parameterTypes">The array of parameter <see cref="Type"/>s to compare against</param>
		/// <returns><see cref="true"/> if the method parameters match <paramref name="parameterTypes"/>, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if either of the <paramref name="methodInfo"/> or <paramref name="parameterTypes"/> arguments are null</exception>
		public static bool HasMatchingParameterTypes(this MethodBase methodInfo, Type[] parameterTypes)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			_ = parameterTypes ?? throw new ArgumentNullException(nameof(parameterTypes));

			var methodParameters = methodInfo.GetParameters();
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
		/// Convenience extension method to check if <paramref name="methodInfo"/> has parameters.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to check</param>
		/// <returns><see cref="true"/> if the method has zero parameters, <see cref="false"/> otherwise</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodInfo"/> is null</exception>
		public static bool HasNoParameters(this MethodBase methodInfo)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			return methodInfo.GetParameters().Length == 0;
		}

		/// <summary>
		/// Parse the <paramref name="methodInfo"/> body's intermediate language (IL) bytes into a collection of <see cref="InstructionBase"/> instances.
		/// </summary>
		/// <param name="methodInfo">The <see cref="MethodBase"/> instance to search within</param>
		/// <returns>The <see cref="IReadOnlyCollection{InstructionBase}"/> of <see cref="InstructionBase"/> parse results</returns>
		/// <exception cref="ArgumentNullException">Thrown if the <paramref name="methodInfo"/> argument is null</exception>
		public static IReadOnlyCollection<InstructionBase> ParseInstructions(this MethodBase methodInfo)
		{
			_ = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
			return new MethodBodyParser(methodInfo).ParseInstructions();
		}
	}
}
