using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup
{
	public static class DynamicTypeBuilder
	{
		/// <summary>
		/// Dynamically build a method with a jump table so we can test the parser against OpCodes.Switch scenarios
		/// <para>IL generator code from <see href="https://docs.microsoft.com/en-us/dotnet/api/system.reflection.emit.opcodes.switch?view=net-5.0#examples"/></para>
		/// </summary>
		public static Type? BuildTypeWithJumpTableMethod()
		{
			// Build a method that contains a jump table / OpCodes.Switch instance
			var dynamicAssemblyName = new AssemblyName();
			dynamicAssemblyName.Name = "TestDynamicAssembly";
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("JumpTableDemo");
			var typeBuilder = moduleBuilder.DefineType("JumpTableDemoType", TypeAttributes.Public);
			var methodBuilder = typeBuilder.DefineMethod("JumpTableMethod", MethodAttributes.Public | MethodAttributes.Static, typeof(string), new Type[] { typeof(int) });
			var ilGenerator = methodBuilder.GetILGenerator();

			var defaultCaseLabel = ilGenerator.DefineLabel();
			var endOfMethodLabel = ilGenerator.DefineLabel();

			// We are initializing our jump table. Note that the labels
			// will be placed later using the MarkLabel method.
			var jumpTable = new Label[] { ilGenerator.DefineLabel(), ilGenerator.DefineLabel(), ilGenerator.DefineLabel(), ilGenerator.DefineLabel(), ilGenerator.DefineLabel() };
			ilGenerator.Emit(OpCodes.Ldarg_0);
			ilGenerator.Emit(OpCodes.Switch, jumpTable);
			// Branch on default case
			ilGenerator.Emit(OpCodes.Br_S, defaultCaseLabel);
			// Case arg0 = 0
			ilGenerator.MarkLabel(jumpTable[0]);
			ilGenerator.Emit(OpCodes.Ldstr, "are no bananas");
			ilGenerator.Emit(OpCodes.Br_S, endOfMethodLabel);

			// Case arg0 = 1
			ilGenerator.MarkLabel(jumpTable[1]);
			ilGenerator.Emit(OpCodes.Ldstr, "is one banana");
			ilGenerator.Emit(OpCodes.Br_S, endOfMethodLabel);

			// Case arg0 = 2
			ilGenerator.MarkLabel(jumpTable[2]);
			ilGenerator.Emit(OpCodes.Ldstr, "are two bananas");
			ilGenerator.Emit(OpCodes.Br_S, endOfMethodLabel);

			// Case arg0 = 3
			ilGenerator.MarkLabel(jumpTable[3]);
			ilGenerator.Emit(OpCodes.Ldstr, "are three bananas");
			ilGenerator.Emit(OpCodes.Br_S, endOfMethodLabel);

			// Case arg0 = 4
			ilGenerator.MarkLabel(jumpTable[4]);
			ilGenerator.Emit(OpCodes.Ldstr, "are four bananas");
			ilGenerator.Emit(OpCodes.Br_S, endOfMethodLabel);

			// Default case
			ilGenerator.MarkLabel(defaultCaseLabel);
			ilGenerator.Emit(OpCodes.Ldstr, "are many bananas");

			ilGenerator.MarkLabel(endOfMethodLabel);
			ilGenerator.Emit(OpCodes.Ret);

			return typeBuilder.CreateType();
		}

		public static Type? BuildTypeWithInlineSignatureMethod()
		{
			// Build a method that contains a jump table / OpCodes.Switch instance
			var dynamicAssemblyName = new AssemblyName();
			dynamicAssemblyName.Name = "TestDynamicAssembly";
			var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicAssemblyName, AssemblyBuilderAccess.Run);
			var moduleBuilder = assemblyBuilder.DefineDynamicModule("InlineSignatureDemo");
			var typeBuilder = moduleBuilder.DefineType("InlineSignatureDemoType", TypeAttributes.Public);
			var methodBuilder = typeBuilder.DefineMethod("InlineSignatureMethod", MethodAttributes.Public | MethodAttributes.Static, typeof(string), new Type[] { typeof(int) });
			var ilGenerator = methodBuilder.GetILGenerator();

			var endOfMethodLabel = ilGenerator.DefineLabel();

			var signatureBlobHelper = SignatureHelper.GetMethodSigHelper(CallingConventions.Standard, typeof(string));
			ilGenerator.Emit(OpCodes.Calli, signatureBlobHelper);

			ilGenerator.MarkLabel(endOfMethodLabel);
			ilGenerator.Emit(OpCodes.Ret);

			return typeBuilder.CreateType();
		}
	}
}
