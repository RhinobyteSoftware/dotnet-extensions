using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rhinobyte.Extensions.Reflection.IntermediateLanguage
{
	/// <summary>
	/// Default implementation of the <see cref="IMemberReferenceMatchInfo"/> contract.
	/// <para>In addition to checking for an exact (reference equals) match, this implementation will consider the following as a successful match:</para>
	/// <para>If <see cref="MemberReferenceMatchInfo.MemberInfoToLookFor"/> is an instance of <see cref="PropertyInfo"/> and the IL instruction member reference is for the get / set method of the property</para>
	/// <para>If <see cref="MemberReferenceMatchInfo.MatchAgainstBaseClassMembers"/> is true and the IL instruction member references a field/method/property on a base type with the same signature as <see cref="MemberReferenceMatchInfo.MemberInfoToLookFor"/></para>
	/// <para>If <see cref="MemberReferenceMatchInfo.MatchAgainstDeclaringTypeMember"/> is true, <see cref="MemberReferenceMatchInfo.MemberInfoToLookFor"/> has a different <see cref="MemberInfo.ReflectedType"/> than it's <see cref="MemberInfo.DeclaringType"/>, and the IL instruction member references the member from the declared type</para>
	/// </summary>
	public sealed class MemberReferenceMatchInfo : IMemberReferenceMatchInfo
	{
		public MemberReferenceMatchInfo(
			MemberInfo memberInfoToLookFor,
			bool matchAgainstBaseClassMembers,
			bool matchAgainstDeclaringTypeMember)
		{
			MemberInfoToLookFor = memberInfoToLookFor ?? throw new ArgumentNullException(nameof(memberInfoToLookFor));
			MatchAgainstBaseClassMembers = matchAgainstBaseClassMembers;
			MatchAgainstDeclaringTypeMember = matchAgainstDeclaringTypeMember;
		}

		public IReadOnlyCollection<MemberReferenceMatchInfo>? BaseClassMembersToCheck { get; private set; }
		public MemberReferenceMatchInfo? DeclaringTypeMemberToCheck { get; private set; }
		public bool DidSwallowException { get; private set; }
		public bool IsInitialized { get; private set; }
		public bool IsStaticMember { get; private set; }
		public bool MatchAgainstBaseClassMembers { get; }
		public bool MatchAgainstDeclaringTypeMember { get; }
		public MemberInfo MemberInfoToLookFor { get; }
		public MethodInfo? PropertyGetMethod { get; private set; }
		public MethodInfo? PropertySetMethod { get; private set; }

		public bool DoesInstructionReferenceMatch(InstructionBase ilInstruction)
		{
			if (ilInstruction is null)
				return false;

			switch (ilInstruction)
			{
				case FieldReferenceInstruction fieldReferenceInstruction:
					return DoesInstructionReferenceMatch(fieldReferenceInstruction.FieldReference);

				case MethodReferenceInstruction methodReferenceInstruction:
					return DoesInstructionReferenceMatch(methodReferenceInstruction.MethodReference);

				case TypeReferenceInstruction typeReferenceInstruction:
					return DoesInstructionReferenceMatch(typeReferenceInstruction.TypeReference);

				case UnknownMemberReferenceInstruction unknownMemberReferenceInstruction:
					return DoesInstructionReferenceMatch(unknownMemberReferenceInstruction.MemberReference);

				default:
					return false;
			}
		}

		public bool DoesInstructionReferenceMatch(MemberInfo? instructionMemberReference)
		{
			if (instructionMemberReference == null)
				return false;

			if (instructionMemberReference.Equals(MemberInfoToLookFor))
				return true;

			Initialize();

			if (PropertyGetMethod != null && instructionMemberReference.Equals(PropertyGetMethod))
				return true;

			if (PropertySetMethod != null && instructionMemberReference.Equals(PropertySetMethod))
				return true;

			if (MatchAgainstDeclaringTypeMember && DeclaringTypeMemberToCheck is not null && DeclaringTypeMemberToCheck.DoesInstructionReferenceMatch(instructionMemberReference))
				return true;

			if (MatchAgainstBaseClassMembers && BaseClassMembersToCheck?.Count > 0)
			{
				foreach (var baseClassMatchInfo in BaseClassMembersToCheck)
				{
					if (baseClassMatchInfo.DoesInstructionReferenceMatch(instructionMemberReference))
						return true;
				}
			}

			return false;
		}

		public void Initialize()
		{
			if (IsInitialized)
				return;

			if (MemberInfoToLookFor is MethodBase methodReference)
				InitializeForMethodReference(methodReference);
			else if (MemberInfoToLookFor is FieldInfo fieldReference)
				InitializeForFieldReference(fieldReference);
			else if (MemberInfoToLookFor is PropertyInfo propertyReference)
				InitializeForPropertyReference(propertyReference);

			IsInitialized = true;
		}

		private void InitializeForFieldReference(FieldInfo fieldReference)
		{
			IsStaticMember = fieldReference.IsStatic;
			if (IsStaticMember)
				return;

			if (!MatchAgainstDeclaringTypeMember && !MatchAgainstBaseClassMembers)
				return;

			if (MatchAgainstDeclaringTypeMember && fieldReference.DeclaringType != null && fieldReference.DeclaringType != fieldReference.ReflectedType)
			{
				var declaringTypeField = fieldReference.DeclaringType.GetField(fieldReference.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (declaringTypeField != null && declaringTypeField != fieldReference)
					DeclaringTypeMemberToCheck = new MemberReferenceMatchInfo(declaringTypeField, false, false);
			}

			if (MatchAgainstBaseClassMembers)
			{
				var baseType = fieldReference.DeclaringType?.BaseType;
				if (baseType == null)
					return;

				var baseClassMembersToCheck = new HashSet<MemberReferenceMatchInfo>();
				while (baseType != null)
				{
					try
					{
						var baseMembers = baseType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

						if (baseMembers.Length > 0)
						{
							foreach (var baseMember in baseMembers)
							{
								if (baseMember == null || baseMember.Name != fieldReference.Name || baseMember == fieldReference)
									continue;

								// Pass false for match against parameters, can keep the base members flat since we'll build an array of them
								var baseMemberToCheck = new MemberReferenceMatchInfo(fieldReference, false, false);
								baseMemberToCheck.Initialize();
								_ = baseClassMembersToCheck.Add(baseMemberToCheck);
							}
						}
					}
#pragma warning disable CA1031 // Do not catch general exception types
					catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
					{
						DidSwallowException = true;
					}

					baseType = baseType.BaseType;
				}

				BaseClassMembersToCheck = baseClassMembersToCheck;
			}
		}

		private void InitializeForMethodReference(MethodBase methodReference)
		{
			IsStaticMember = methodReference.IsStatic;
			if (IsStaticMember)
				return;

			if (!MatchAgainstDeclaringTypeMember && !MatchAgainstBaseClassMembers)
				return;

			var methodReferenceParameters = methodReference.GetParameters();

			if (MatchAgainstDeclaringTypeMember && methodReference.DeclaringType != null && methodReference.DeclaringType != methodReference.ReflectedType)
			{
				var declaringTypeMethods = methodReference.DeclaringType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

				if (declaringTypeMethods.Length > 0)
				{
					foreach (var declaringTypeMethod in declaringTypeMethods)
					{
						if (declaringTypeMethod.Name != methodReference.Name || declaringTypeMethod == methodReference)
							continue;

						if (declaringTypeMethod.HasMatchingParameterNamesAndTypes(methodReferenceParameters)
							&& declaringTypeMethod.IsPublic == methodReference.IsPublic
							&& declaringTypeMethod.IsPrivate == methodReference.IsPrivate)
						{
							DeclaringTypeMemberToCheck = new MemberReferenceMatchInfo(declaringTypeMethod, false, false);
							break;
						}
					}
				}
			}

			if (MatchAgainstBaseClassMembers)
			{
				var baseType = methodReference.DeclaringType?.BaseType;
				if (baseType == null)
					return;

				var baseClassMembersToCheck = new HashSet<MemberReferenceMatchInfo>();
				while (baseType != null)
				{
					try
					{
						var baseMembers = baseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

						if (baseMembers.Length > 0)
						{
							foreach (var baseMember in baseMembers)
							{
								if (baseMember == null || baseMember.Name != methodReference.Name || baseMember == methodReference)
									continue;

								if (!baseMember.HasMatchingParameterNamesAndTypes(methodReferenceParameters))
									continue;

								// Pass false for match against parameters, can keep the base members flat since we'll build an array of them
								var baseMemberToCheck = new MemberReferenceMatchInfo(baseMember, false, false);
								baseMemberToCheck.Initialize();
								_ = baseClassMembersToCheck.Add(baseMemberToCheck);
							}
						}
					}
#pragma warning disable CA1031 // Do not catch general exception types
					catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
					{
						DidSwallowException = true;
					}

					baseType = baseType.BaseType;
				}

				BaseClassMembersToCheck = baseClassMembersToCheck;
			}
		}

		private void InitializeForPropertyReference(PropertyInfo propertyReference)
		{
			try
			{
				PropertyGetMethod = propertyReference.GetMethod;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
			{
				DidSwallowException = true;
			}

			try
			{
				PropertySetMethod = propertyReference.SetMethod;
			}
#pragma warning disable CA1031 // Do not catch general exception types
			catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
			{

				DidSwallowException = true;
			}

			IsStaticMember = PropertyGetMethod?.IsStatic ?? PropertySetMethod?.IsStatic ?? false;
			if (IsStaticMember)
				return;

			if (MatchAgainstDeclaringTypeMember && propertyReference.DeclaringType != null && propertyReference.DeclaringType != propertyReference.ReflectedType)
			{
				var declaringTypeMember = propertyReference.DeclaringType.GetProperty(propertyReference.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				if (declaringTypeMember != null && declaringTypeMember != propertyReference)
				{
					DeclaringTypeMemberToCheck = new MemberReferenceMatchInfo(declaringTypeMember, false, false);
				}
			}

			if (MatchAgainstBaseClassMembers)
			{
				var baseType = propertyReference.DeclaringType?.BaseType;
				if (baseType == null)
					return;

				var baseClassMembersToCheck = new HashSet<MemberReferenceMatchInfo>();
				while (baseType != null)
				{
					try
					{
						var baseMembers = baseType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

						if (baseMembers.Length > 0)
						{
							foreach (var baseMember in baseMembers)
							{
								if (baseMember == null || baseMember == propertyReference || baseMember.Name != propertyReference.Name)
									continue;

								// Pass false for match against parameters, can keep the base members flat since we'll build an array of them
								var baseMemberToCheck = new MemberReferenceMatchInfo(baseMember, false, false);
								baseMemberToCheck.Initialize();

								_ = baseClassMembersToCheck.Add(baseMemberToCheck);
							}
						}
					}
#pragma warning disable CA1031 // Do not catch general exception types
					catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
					{
						DidSwallowException = true;
					}

					baseType = baseType.BaseType;
				}

				BaseClassMembersToCheck = baseClassMembersToCheck;
			}
		}


	}
}
