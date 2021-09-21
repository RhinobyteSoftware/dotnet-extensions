using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class UnknownMemberReferenceInstructionTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/
		[TestMethod]
		public void ToString_gracefully_handles_null()
		{
			var unknownMemberReferenceInstruction = new UnknownMemberReferenceInstruction(0, 0, OpCodes.Nop, null);
			unknownMemberReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();

			var bogusMemberInfo = new BogusMemberInfo();
			unknownMemberReferenceInstruction = new UnknownMemberReferenceInstruction(0, 0, OpCodes.Nop, bogusMemberInfo);
			unknownMemberReferenceInstruction.ToString().Should().NotBeNullOrWhiteSpace();
		}

		/******     TEST SETUP     *****************************
		 *******************************************************/
		public class BogusMemberInfo : System.Reflection.MemberInfo
		{
			public override Type? DeclaringType => null;

			public override MemberTypes MemberType => MemberTypes.Custom;

			public override string Name => null!;

			public override Type? ReflectedType => null;

			public override object[] GetCustomAttributes(bool inherit) => Array.Empty<object>();
			public override object[] GetCustomAttributes(Type attributeType, bool inherit) => Array.Empty<object>();
			public override bool IsDefined(Type attributeType, bool inherit) => false;
		}
	}
}
