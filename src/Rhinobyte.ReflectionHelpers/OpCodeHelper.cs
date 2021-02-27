using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Rhinobyte.ReflectionHelpers
{
	/// <summary>
	/// Static helper class for fast lookup of <see cref="OpCode"/> instances and related information.
	/// </summary>
	/// <summary>
	/// Static helper class for fast lookup of <see cref="OpCode"/> instances and related information.
	/// </summary>
	public static class OpCodeHelper
	{
		public static readonly IReadOnlyDictionary<short, string> DescriptionLookup = new Dictionary<short, string>()
		{
			{ 0, "NO-OP" }, // OpCodes.Nop
			{ 1, "DEBUGGER BREAK - Signals the CLI to inform debugger that a break point has been tripped" }, // OpCodes.Break
			{ 2, "LOAD ARGUMENT (Index 0) - Push the argument at index 0 onto the top of the evaluation stack" }, // OpCodes.Ldarg_0
			{ 3, "LOAD ARGUMENT (Index 1) - Push the argument at index 1 onto the top of the evaluation stack" }, // OpCodes.Ldarg_1
			{ 4, "LOAD ARGUMENT (Index 2) - Push the argument at index 2 onto the top of the evaluation stack" }, // OpCodes.Ldarg_2
			{ 5, "LOAD ARGUMENT (Index 3) - Push the argument at index 3 onto the top of the evaluation stack" }, // OpCodes.Ldarg_3
			{ 6, "LOAD LOCAL VARIABLE (Index 0) - Push the local variable at index 0 onto the top of the evaluation stack" }, // OpCodes.Ldloc_0
			{ 7, "LOAD LOCAL VARIABLE (Index 1) - Push the local variable at index 1 onto the top of the evaluation stack" }, // OpCodes.Ldloc_1
			{ 8, "LOAD LOCAL VARIABLE (Index 2) - Push the local variable at index 2 onto the top of the evaluation stack" }, // OpCodes.Ldloc_2
			{ 9, "LOAD LOCAL VARIABLE (Index 3) - Push the local variable at index 3 onto the top of the evaluation stack" }, // OpCodes.Ldloc_3
			{ 10, "SET LOCAL VARIABLE (Index 0) - Pop the current value from the top of the evaluation stack and store it in the local variable list at index 0" }, // OpCodes.Stloc_0
			{ 11, "SET LOCAL VARIABLE (Index 1) - Pop the current value from the top of the evaluation stack and store it in the local variable list at index 1" }, // OpCodes.Stloc_1
			{ 12, "SET LOCAL VARIABLE (Index 2) - Pop the current value from the top of the evaluation stack and store it in the local variable list at index 2" }, // OpCodes.Stloc_2
			{ 13, "SET LOCAL VARIABLE (Index 3) - Pop the current value from the top of the evaluation stack and store it in the local variable list at index 3" }, // OpCodes.Stloc_3
			{ 14, "LOAD ARGUMENT (Specified Short Form Index) - Load the argument referenced by the specified short form index onto the evaluation stack" }, // OpCodes.Ldarg_S
			{ 15, "LOAD ARGUMENT ADDRESS (Specified Short Form Index) - Load the argument address specified in short form index onto the evaluation stack" }, // OpCodes.Ldarga_S
			{ 16, "SET ARGUMENT (Specified Short Form Index) - Store the current value on top of the evaluation stack into the argument slot at the specified short form index" }, // OpCodes.Starg_S
			{ 17, "LOAD LOCAL VARIABLE (Specified Short Form Index) - Push the local variable value at the specified short form index onto the evaluation stack" }, // OpCodes.Ldloc_S
			{ 18, "LOAD LOCAL VARIABLE ADDRESS (Specified Short Form Index) - Push the address of the local variable at the specified short form index onto the evaluation stack" }, // OpCodes.Ldloca_S
			{ 19, "SET LOCAL VARIABLE (Specified Short Form Index) - Pop the current value on top of the evaluation stack into the local variable list at the specified short form index" }, // OpCodes.Stloc_S
			{ 20, "LOAD NULL REFERENCE - Push a null reference (Type O) onto the evaluation stack" }, // OpCodes.Ldnull
			{ 21, "LOAD INT LITERAL (-1) - Push the value -1 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_M1
			{ 22, "LOAD INT LITERAL (0) - Push the value 0 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_0
			{ 23, "LOAD INT LITERAL (1) - Push the value 1 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_1
			{ 24, "LOAD INT LITERAL (2) - Push the value 2 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_2
			{ 25, "LOAD INT LITERAL (3) - Push the value 3 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_3
			{ 26, "LOAD INT LITERAL (4) - Push the value 4 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_4
			{ 27, "LOAD INT LITERAL (5) - Push the value 5 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_5
			{ 28, "LOAD INT LITERAL (6) - Push the value 6 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_6
			{ 29, "LOAD INT LITERAL (7) - Push the value 7 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_7
			{ 30, "LOAD INT LITERAL (8) - Push the value 8 (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_8
			{ 31, "LOAD INT VALUE (Int8) - Push the supplied Int8 value (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4_S
			{ 32, "LOAD INT VALUE (Int32) - Push the supplied Int32 value (As Int32) onto the evaluation stack" }, // OpCodes.Ldc_I4
			{ 33, "LOAD INT VALUE (Int64) - Push the supplied Int64 value (As Int64) onto the evaluation stack" }, // OpCodes.Ldc_I8
			{ 34, "LOAD FLOAT VALUE (Float32) - Push the supplied Float32 value (As Type F) onto the evaluation stack" }, // OpCodes.Ldc_R4
			{ 35, "LOAD FLOAT VALUE (Float64) - Push the supplied Float64 value (As Type F) onto the evaluation stack" }, // OpCodes.Ldc_R8
			{ 37, "DUPLICATE - Copy top of stack and push copy to top of stack" }, // OpCodes.Dup
			{ 38, "POP STACK - Remove the value currently on top of the evaluation stack" }, // OpCodes.Pop
			{ 39, "JUMP - Exit current method and jump to specified method" }, // OpCodes.Jmp
			{ 40, "CALL METHOD - Call passed method descriptor" }, // OpCodes.Call
			{ 41, "CALL METHOD - Call method indicated on evaluation stack" }, // OpCodes.Calli
			{ 42, "RETURN - Return from current method and push return value (if present) to caller's evaluation stack" }, // OpCodes.Ret
			{ 43, "BRANCH UNCONDITIONALLY (Short Form) - Unconditionally transfer control to specified target instruction (Short Form)" }, // OpCodes.Br_S
			{ 44, "BRANCH WHEN FALSE (Short Form) - Transfer control to specified (short form) target instruction, if the value is falsy (false, a null reference, or zero)" }, // OpCodes.Brfalse_S
			{ 45, "BRANCH WHEN TRUE (Short Form) - Transfer control to specified (short form) target instruction, if the value is truthy (true, not null, or non-zero)" }, // OpCodes.Brtrue_S
			{ 46, "BRANCH WHEN EQUAL (Short Form) - Transfer control to specified (short form) target instruction, if the two values are equal" }, // OpCodes.Beq_S
			{ 47, "BRANCH WHEN GREATER THAN OR EQUAL (Short Form) - Transfer control to specified (short form) target instruction, if the first value is greater than or equal to the second value" }, // OpCodes.Bge_S
			{ 48, "BRANCH WHEN GREATER THAN (Short Form) - Transfer control to specified (short form) target instruction, if the first value is greater than the second value" }, // OpCodes.Bgt_S
			{ 49, "BRANCH WHEN LESS THAN OR EQUAL (Short Form) - Transfer control to specified (short form) target instruction, if the first value is less than or equal to the second value" }, // OpCodes.Ble_S
			{ 50, "BRANCH WHEN LESS THAN (Short Form) - Transfer control to specified (short form) target instruction, if the first value is less than the second value" }, // OpCodes.Blt_S
			{ 51, "BRANCH WHEN NOT EQUAL (Unsigned, Short Form) - Transfer control to specified (short form) target instruction, when the values are not equal [unsigned integer or unordered float]" }, // OpCodes.Bne_Un_S
			{ 52, "BRANCH WHEN GREATER THAN OR EQUAL (Unsigned, Short Form) - Transfer control to specified (short form) target instruction, if the first value is greater than or equal to the second value [unsigned integer or unordered float]" }, // OpCodes.Bge_Un_S
			{ 53, "BRANCH WHEN GREATER THAN (Unsigned, Short Form) - Transfer control to specified (short form) target instruction, if the first value is greater than the second value [unsigned integer or unordered float]" }, // OpCodes.Bgt_Un_S
			{ 54, "BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form) - Transfer control to specified (short form) target instruction, if the first value is less than or equal to the second value [unsigned integer or unordered float]" }, // OpCodes.Ble_Un_S
			{ 55, "BRANCH WHEN LESS THAN (Unsigned, Short Form) - Transfer control to specified (short form) target instruction, if the first value is less than the second value [unsigned integer or unordered float]" }, // OpCodes.Blt_Un_S
			{ 56, "BRANCH UNCONDITIONALLY - Unconditionally transfer control to specified target instruction" }, // OpCodes.Br
			{ 57, "BRANCH WHEN FALSE - Transfer control to specified target instruction, if the value is falsy (false, a null reference, or zero)" }, // OpCodes.Brfalse
			{ 58, "BRANCH WHEN TRUE - Transfer control to specified target instruction, if the value is truthy (true, not null, or non-zero)" }, // OpCodes.Brtrue
			{ 59, "BRANCH WHEN EQUAL - Transfer control to specified target instruction, if the two values are equal" }, // OpCodes.Beq
			{ 60, "BRANCH WHEN GREATER THAN OR EQUAL - Transfer control to specified target instruction, if the first value is greater than or equal to the second value" }, // OpCodes.Bge
			{ 61, "BRANCH WHEN GREATER THAN - Transfer control to specified target instruction, if the first value is greater than the second value" }, // OpCodes.Bgt
			{ 62, "BRANCH WHEN LESS THAN OR EQUAL - Transfer control to specified target instruction, if the first value is less than or equal to the second value" }, // OpCodes.Ble
			{ 63, "BRANCH WHEN LESS THAN - Transfer control to specified target instruction, if the first value is less than the second value" }, // OpCodes.Blt
			{ 64, "BRANCH WHEN NOT EQUAL (Unsigned) - Transfer control to specified target instruction, when the values are not equal [unsigned integer or unordered float]" }, // OpCodes.Bne_Un
			{ 65, "BRANCH WHEN GREATER THAN OR EQUAL (Unsigned) - Transfer control to specified target instruction, if the first value is greater than or equal to the second value [unsigned integer or unordered float]" }, // OpCodes.Bge_Un
			{ 66, "BRANCH WHEN GREATER THAN (Unsigned) - Transfer control to specified target instruction, if the first value is greater than the second value [unsigned integer or unordered float]" }, // OpCodes.Bgt_Un
			{ 67, "BRANCH WHEN LESS THAN OR EQUAL (Unsigned) - Transfer control to specified target instruction, if the first value is less than or equal to the second value [unsigned integer or unordered float]" }, // OpCodes.Ble_Un
			{ 68, "BRANCH WHEN LESS THAN (Unsigned) - Transfer control to specified target instruction, if the first value is less than the second value [unsigned integer or unordered float]" }, // OpCodes.Blt_Un
			{ 69, "SWITCH - Implement A Jump Table" }, // OpCodes.Switch
			{ 70, "LOAD INDIRECT (Int8) - Indirectly load value of type Int8 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_I1
			{ 71, "LOAD INDIRECT (UInt8) - Indirectly load value of type unsigned Int8 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_U1
			{ 72, "LOAD INDIRECT (Int16) - Indirectly load value of type Int16 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_I2
			{ 73, "LOAD INDIRECT (UInt16) - Indirectly load value of type unsigned Int16 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_U2
			{ 74, "LOAD INDIRECT (Int32) - Indirectly load value of type Int32 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_I4
			{ 75, "LOAD INDIRECT (UInt32) - Indirectly load value of type unsigned Int32 onto the evaluation stack (As Int32)" }, // OpCodes.Ldind_U4
			{ 76, "LOAD INDIRECT (Int64) - Indirectly load value of type Int64 onto the evaluation stack (As Int64)" }, // OpCodes.Ldind_I8
			{ 77, "LOAD INDIRECT (Native Int) - Indirectly load value of type Native Int onto the evaluation stack (As Native Int)" }, // OpCodes.Ldind_I
			{ 78, "LOAD INDIRECT (Float32) - Indirectly load value of type Float32 onto the evaluation stack (As Type F)" }, // OpCodes.Ldind_R4
			{ 79, "LOAD INDIRECT (Float64) - Indirectly load value of type Float64 onto the evaluation stack (As Type F)" }, // OpCodes.Ldind_R8
			{ 80, "LOAD INDIRECT (Object) - Indirectly load an object reference onto the evaluation stack (As Type O)" }, // OpCodes.Ldind_Ref
			{ 81, "STORE AT ADDRESS (Object) - Store an object reference value at a supplied address" }, // OpCodes.Stind_Ref
			{ 82, "STORE AT ADDRESS (Int8) - Store a value of type Int8 at a supplied address" }, // OpCodes.Stind_I1
			{ 83, "STORE AT ADDRESS (Int16) - Store a value of type Int16 at a supplied address" }, // OpCodes.Stind_I2
			{ 84, "STORE AT ADDRESS (Int32) - Store a value of type Int32 at a supplied address" }, // OpCodes.Stind_I4
			{ 85, "STORE AT ADDRESS (Int64) - Store a value of type Int64 at a supplied address" }, // OpCodes.Stind_I8
			{ 86, "STORE AT ADDRESS (Float32) - Store a value of type Float32 at a supplied address" }, // OpCodes.Stind_R4
			{ 87, "STORE AT ADDRESS (Float64) - Store a value of type Float64 at a supplied address" }, // OpCodes.Stind_R8
			{ 88, "ADD - Add two values and push the result onto the evalutation stack" }, // OpCodes.Add
			{ 89, "SUBTRACT - Subtract one value from another and push the result onto the evalutation stack" }, // OpCodes.Sub
			{ 90, "MULTIPLY - Multiply two values and push the result onto the evalutation stack" }, // OpCodes.Mul
			{ 91, "DIVIDE - Divide two values and push the result as a floating-point (Type F) or a quotient (Int32) onto the evalutation stack" }, // OpCodes.Div
			{ 92, "DIVIDE - Divide two unsigned integer values and push the result (Int32) onto the evalutation stack" }, // OpCodes.Div_Un
			{ 93, "REMAINDER - Divide two values and push the remainder onto the evaluation stack" }, // OpCodes.Rem
			{ 94, "REMAINDER - Divide two unsigned values and push the remainder onto the evaluation stack" }, // OpCodes.Rem_Un
			{ 95, "BITWISE AND - Compute the bitwise AND of two values and push the result onto the evalutation stack" }, // OpCodes.And
			{ 96, "BITWISE OR - Compute the bitwise OR (bitwise complement) of the two integer values on the top of the stack and push the result onto the evalutation stack" }, // OpCodes.Or
			{ 97, "BITWISE XOR - Compute the bitwise XOR of top two values from the evaluation stack and push the result onto the evalutation stack" }, // OpCodes.Xor
			{ 98, "LEFT SHIFT INTEGER - Shift an integer value to the left (in zeroes) by the specified number of bits and push the result onto the evaluation stack" }, // OpCodes.Shl
			{ 99, "RIGHT SHIFT INTEGER (Signed) - Shift an integer value (in sign) to the right by the specified number of bits and push the result onto the evaluation stack" }, // OpCodes.Shr
			{ 100, "RIGHT SHIFT INTEGER (Unsigned) - Shift an unsigned integer value (in zeroes) to the right by the specified number of bits and push the result onto the evaluation stack" }, // OpCodes.Shr_Un
			{ 101, "NEGATE VALUE - Negate a value and push the result onto the evaluation stack" }, // OpCodes.Neg
			{ 102, "NOT (Bitwise Complement) - Compute the bitwise complement of the single integer value on top of the stack and push the result onto the evaluation stack (As Same Type)" }, // OpCodes.Not
			{ 103, "CONVERT (Int8) - Convert the value on the top of the stack to Int8 then extend (pad) to Int32" }, // OpCodes.Conv_I1
			{ 104, "CONVERT (Int16) - Convert the value on the top of the stack to Int16 then extend (pad) to Int32" }, // OpCodes.Conv_I2
			{ 105, "CONVERT (Int32) - Convert the value on the top of the stack to Int32" }, // OpCodes.Conv_I4
			{ 106, "CONVERT (Int64) - Convert the value on the top of the stack to Int64" }, // OpCodes.Conv_I8
			{ 107, "CONVERT (Float32) - Convert the value on the top of the stack to Float32" }, // OpCodes.Conv_R4
			{ 108, "CONVERT (Float64) - Convert the value on the top of the stack to Float64" }, // OpCodes.Conv_R8
			{ 109, "CONVERT (UInt32) - Convert the value on the top of the stack to UInt32 then extend to Int32" }, // OpCodes.Conv_U4
			{ 110, "CONVERT (UInt64) - Convert the value on the top of the stack to UInt64 then extend to Int64" }, // OpCodes.Conv_U8
			{ 111, "CALL VIRTUAL - Call a late-bound method on an object then push the return value onto the evaluation stack" }, // OpCodes.Callvirt
			{ 112, "COPY OBJECT - Copy the value type at the address of a source object (Type &, Type *, Or Type Native Int) to the address of destination object (Type &, Type *, Or Type Native Int)" }, // OpCodes.Cpobj
			{ 113, "LOAD OBJECT - Copy the value type object pointed to an address to top of evaluation stack" }, // OpCodes.Ldobj
			{ 114, "LOAD STRING - Push a new object reference to a string literal stored in the metadata" }, // OpCodes.Ldstr
			{ 115, "NEW OBJECT - Create a new object or new instance of a value type then push an object reference (Type O) onto the evaluation stack" }, // OpCodes.Newobj
			{ 116, "CAST CLASS - Attempt to cast object passed by reference to the specified class" }, // OpCodes.Castclass
			{ 117, "IS INSTANCE - Test whether an object reference (Type O) is an instance of a particular class" }, // OpCodes.Isinst
			{ 118, "CONVERT (Unsigned Int To Float32) - Convert the unsigned integer value on the top of the evaluation stack to Float32" }, // OpCodes.Conv_R_Un
			{ 121, "UNBOX (Value Type) - Convert the boxed representation of a value type to its unboxed form" }, // OpCodes.Unbox
			{ 122, "THROW - Throw the exception object currently on the evaluation stack" }, // OpCodes.Throw
			{ 123, "LOAD FIELD - Find the value of a field in the object whose reference is currently on the evaluation stack" }, // OpCodes.Ldfld
			{ 124, "LOAD FIELD ADDRESS - Find the address of a field in the object whose reference is currently on the evaluation stack" }, // OpCodes.Ldflda
			{ 125, "STORE FIELD - Replace the value stored in the field of an object reference or pointer with a new value" }, // OpCodes.Stfld
			{ 126, "LOAD STATIC FIELD - Push the value of a static field onto the evaluation stack" }, // OpCodes.Ldsfld
			{ 127, "LOAD STATIC FIELD ADDRESS - Push the address of a static field onto the evaluation stack" }, // OpCodes.Ldsflda
			{ 128, "STORE STATIC FIELD - Replace the value of a static field with the value from the evaluation stack" }, // OpCodes.Stsfld
			{ 129, "STORE OBJECT - Copy a value of a specified type from the evaluation stack into a supplied memory address" }, // OpCodes.Stobj
			{ 130, "CONVERT OR THROW OVERFLOW (Unsigned to Int8) - Convert the unsigned value on the top of the stack to a signed Int8 then extend to Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_I1_Un
			{ 131, "CONVERT OR THROW OVERFLOW (Unsigned to Int16) - Convert the unsigned value on the top of the stack to a signed Int16 then extend to Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_I2_Un
			{ 132, "CONVERT OR THROW OVERFLOW (Unsigned to Int32) - Convert the unsigned value on the top of the stack to a signed Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_I4_Un
			{ 133, "CONVERT OR THROW OVERFLOW (Unsigned to Int64) - Convert the unsigned value on the top of the stack to a signed Int64 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_I8_Un
			{ 134, "CONVERT OR THROW OVERFLOW (Unsigned to UInt8) - Convert the unsigned value on the top of the stack to an unsigned Int8 then extend to Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_U1_Un
			{ 135, "CONVERT OR THROW OVERFLOW (Unsigned to UInt16) - Convert the unsigned value on the top of the stack to an unsigned Int16 then extend to Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_U2_Un
			{ 136, "CONVERT OR THROW OVERFLOW (Unsigned to UInt32) - Convert the unsigned value on the top of the stack to an unsigned Int32 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_U4_Un
			{ 137, "CONVERT OR THROW OVERFLOW (Unsigned to UInt64) - Convert the unsigned value on the top of the stack to an unsigned Int64 - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_U8_Un
			{ 138, "CONVERT OR THROW OVERFLOW (Unsigned to Signed Native Int) - Convert the unsinged value on the top of the stack to a signed Native Int - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_I_Un
			{ 139, "CONVERT OR THROW OVERFLOW (Unsigned to Unsigned Native Int) - Convert the unsinged value on the top of the stack to an unsigned Native Int - Throw an OverflowException on overflow" }, // OpCodes.Conv_Ovf_U_Un
			{ 140, "BOX VALUE - Convert the value type to an object reference (Type O)" }, // OpCodes.Box
			{ 141, "NEW ARRAY - Push an object reference to a new array (zero-based, one-dimensional array of specified element type) onto the evaluation stack" }, // OpCodes.Newarr
			{ 142, "LOAD MULTIPLE ELEMENTS FROM ARRAY - Push the specified number of elements from a zero-based one-dimentional array onto the evaluation stack" }, // OpCodes.Ldlen
			{ 143, "LOAD FROM ARRAY (Element Address) - Push the address of the element at the specified array index onto the evaluation stack as Type & (managed pointer)" }, // OpCodes.Ldelema
			{ 144, "LOAD FROM ARRAY (Int8) - Push the element with type signed Int8 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_I1
			{ 145, "LOAD FROM ARRAY (UInt8) - Push the element with type unsigned Int8 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_U1
			{ 146, "LOAD FROM ARRAY (Int16) - Push the element with type signed Int16 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_I2
			{ 147, "LOAD FROM ARRAY (UInt16) - Push the element with type unsigned Int16 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_U2
			{ 148, "LOAD FROM ARRAY (Int32) - Push the element with type signed Int32 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_I4
			{ 149, "LOAD FROM ARRAY (UInt32) - Push the element with type unsigned Int32 at the specified array index onto the evaluation stack as an Int32" }, // OpCodes.Ldelem_U4
			{ 150, "LOAD FROM ARRAY (Int64) - Push the element with type Int64 at the specified array index onto the evaluation stack as an Int64" }, // OpCodes.Ldelem_I8
			{ 151, "LOAD FROM ARRAY (Native Int) - Push the element with type Native Int at the specified array index onto the evaluation stack as a Native Int" }, // OpCodes.Ldelem_I
			{ 152, "LOAD FROM ARRAY (Float32) - Push the element with type Float32 at the specified array index onto the evaluation stack as Type F (float)" }, // OpCodes.Ldelem_R4
			{ 153, "LOAD FROM ARRAY (Float64) - Push the element with type Float64 at the specified array index onto the evaluation stack as Type F (float)" }, // OpCodes.Ldelem_R8
			{ 154, "LOAD FROM ARRAY (Object Reference) - Push the object reference at the specified array index onto the evaluation stack as Type O (object reference)" }, // OpCodes.Ldelem_Ref
			{ 155, "STORE ARRAY ELEMENT (Native Int) - Replace the array element at the specified index with the native int value on the evaluation stack" }, // OpCodes.Stelem_I
			{ 156, "STORE ARRAY ELEMENT (Int8) - Replace the array element at the specified index with the Int8 value on the evaluation stack" }, // OpCodes.Stelem_I1
			{ 157, "STORE ARRAY ELEMENT (Int16) - Replace the array element at the specified index with the Int16 value on the evaluation stack" }, // OpCodes.Stelem_I2
			{ 158, "STORE ARRAY ELEMENT (Int32) - Replace the array element at the specified index with the Int32 value on the evaluation stack" }, // OpCodes.Stelem_I4
			{ 159, "STORE ARRAY ELEMENT (Int64) - Replace the array element at the specified index with the Int64 value on the evaluation stack" }, // OpCodes.Stelem_I8
			{ 160, "STORE ARRAY ELEMENT (Float32) - Replace the array element at the specified index with the Float32 value on the evaluation stack" }, // OpCodes.Stelem_R4
			{ 161, "STORE ARRAY ELEMENT (Float64) - Replace the array element at the specified index with the Float64 value on the evaluation stack" }, // OpCodes.Stelem_R8
			{ 162, "STORE ARRAY ELEMENT (Object Reference) - Replace the array element at the specified index with the object reference value (Type O) on the evaluation stack" }, // OpCodes.Stelem_Ref
			{ 163, "LOAD FROM ARRAY (Specified Type) - Push the element at the specified array index onto the evaluation stack as the type specified in the instruction" }, // OpCodes.Ldelem
			{ 164, "STORE ARRAY ELEMENT (Specified Type) - Replace the array element at the specified index with the value on the evaluation stack whose type is specified in the instruction" }, // OpCodes.Stelem
			{ 165, "UNBOX (Any Type) - Convert the boxed representation of a type specified in the instruction to its unboxed form" }, // OpCodes.Unbox_Any
			//{ 179, OpCodes.Conv_Ovf_I1 }, // OpCodes.Conv_Ovf_I1
			//{ 180, OpCodes.Conv_Ovf_U1 }, // OpCodes.Conv_Ovf_U1
			//{ 181, OpCodes.Conv_Ovf_I2 }, // OpCodes.Conv_Ovf_I2
			//{ 182, OpCodes.Conv_Ovf_U2 }, // OpCodes.Conv_Ovf_U2
			//{ 183, OpCodes.Conv_Ovf_I4 }, // OpCodes.Conv_Ovf_I4
			//{ 184, OpCodes.Conv_Ovf_U4 }, // OpCodes.Conv_Ovf_U4
			//{ 185, OpCodes.Conv_Ovf_I8 }, // OpCodes.Conv_Ovf_I8
			//{ 186, OpCodes.Conv_Ovf_U8 }, // OpCodes.Conv_Ovf_U8
			//{ 194, OpCodes.Refanyval }, // OpCodes.Refanyval
			//{ 195, OpCodes.Ckfinite }, // OpCodes.Ckfinite
			//{ 198, OpCodes.Mkrefany }, // OpCodes.Mkrefany
			//{ 208, OpCodes.Ldtoken }, // OpCodes.Ldtoken
			//{ 209, OpCodes.Conv_U2 }, // OpCodes.Conv_U2
			//{ 210, OpCodes.Conv_U1 }, // OpCodes.Conv_U1
			//{ 211, OpCodes.Conv_I }, // OpCodes.Conv_I
			//{ 212, OpCodes.Conv_Ovf_I }, // OpCodes.Conv_Ovf_I
			//{ 213, OpCodes.Conv_Ovf_U }, // OpCodes.Conv_Ovf_U
			//{ 214, OpCodes.Add_Ovf }, // OpCodes.Add_Ovf
			//{ 215, OpCodes.Add_Ovf_Un }, // OpCodes.Add_Ovf_Un
			//{ 216, OpCodes.Mul_Ovf }, // OpCodes.Mul_Ovf
			//{ 217, OpCodes.Mul_Ovf_Un }, // OpCodes.Mul_Ovf_Un
			//{ 218, OpCodes.Sub_Ovf }, // OpCodes.Sub_Ovf
			//{ 219, OpCodes.Sub_Ovf_Un }, // OpCodes.Sub_Ovf_Un
			//{ 220, OpCodes.Endfinally }, // OpCodes.Endfinally
			//{ 221, OpCodes.Leave }, // OpCodes.Leave
			//{ 222, OpCodes.Leave_S }, // OpCodes.Leave_S
			//{ 223, OpCodes.Stind_I }, // OpCodes.Stind_I
			//{ 224, OpCodes.Conv_U }, // OpCodes.Conv_U
			//{ 248, OpCodes.Prefix7 }, // OpCodes.Prefix7
			//{ 249, OpCodes.Prefix6 }, // OpCodes.Prefix6
			//{ 250, OpCodes.Prefix5 }, // OpCodes.Prefix5
			//{ 251, OpCodes.Prefix4 }, // OpCodes.Prefix4
			//{ 252, OpCodes.Prefix3 }, // OpCodes.Prefix3
			//{ 253, OpCodes.Prefix2 }, // OpCodes.Prefix2
			//{ 254, OpCodes.Prefix1 }, // OpCodes.Prefix1
			//{ 255, OpCodes.Prefixref }, // OpCodes.Prefixref
			//{ -512, OpCodes.Arglist }, // OpCodes.Arglist
			//{ -511, OpCodes.Ceq }, // OpCodes.Ceq
			//{ -510, OpCodes.Cgt }, // OpCodes.Cgt
			//{ -509, OpCodes.Cgt_Un }, // OpCodes.Cgt_Un
			//{ -508, OpCodes.Clt }, // OpCodes.Clt
			//{ -507, OpCodes.Clt_Un }, // OpCodes.Clt_Un
			//{ -506, OpCodes.Ldftn }, // OpCodes.Ldftn
			//{ -505, OpCodes.Ldvirtftn }, // OpCodes.Ldvirtftn
			//{ -503, OpCodes.Ldarg }, // OpCodes.Ldarg
			//{ -502, OpCodes.Ldarga }, // OpCodes.Ldarga
			//{ -501, OpCodes.Starg }, // OpCodes.Starg
			//{ -500, OpCodes.Ldloc }, // OpCodes.Ldloc
			//{ -499, OpCodes.Ldloca }, // OpCodes.Ldloca
			//{ -498, OpCodes.Stloc }, // OpCodes.Stloc
			//{ -497, OpCodes.Localloc }, // OpCodes.Localloc
			//{ -495, OpCodes.Endfilter }, // OpCodes.Endfilter
			//{ -494, OpCodes.Unaligned }, // OpCodes.Unaligned
			//{ -493, OpCodes.Volatile }, // OpCodes.Volatile
			//{ -492, OpCodes.Tailcall }, // OpCodes.Tailcall
			//{ -491, OpCodes.Initobj }, // OpCodes.Initobj
			//{ -490, OpCodes.Constrained }, // OpCodes.Constrained
			//{ -489, OpCodes.Cpblk }, // OpCodes.Cpblk
			//{ -488, OpCodes.Initblk }, // OpCodes.Initblk
			//{ -486, OpCodes.Rethrow }, // OpCodes.Rethrow
			//{ -484, OpCodes.Sizeof }, // OpCodes.Sizeof
			//{ -483, OpCodes.Refanytype }, // OpCodes.Refanytype
			{ -482, "Specify Subsequent Array Address Operation Is Readonly" } // OpCodes.Readonly
		};

		/// <summary>
		/// Collection of <see cref="OpCodeType.Nternal"/> opcodes
		/// </summary>
		public static readonly IReadOnlyCollection<OpCode> InternalOpcodes = new OpCode[] { OpCodes.Prefix7, OpCodes.Prefix6, OpCodes.Prefix5, OpCodes.Prefix4, OpCodes.Prefix3, OpCodes.Prefix2, OpCodes.Prefix1, OpCodes.Prefixref };

		public static readonly IReadOnlyCollection<short> LocalVariableOpcodeValues = new short[] { 17, 18, 19, -500, -499, -498 };

		/// <summary>
		/// Dictionary to look up the <see cref="OpCodes"/> field name for a given <see cref="OpCode.Value"/>.
		/// </summary>
		public static readonly IReadOnlyDictionary<short, string> NameLookup = new Dictionary<short, string>()
		{
			{ 0, "Nop" },
			{ 1, "Break" },
			{ 2, "Ldarg_0" },
			{ 3, "Ldarg_1" },
			{ 4, "Ldarg_2" },
			{ 5, "Ldarg_3" },
			{ 6, "Ldloc_0" },
			{ 7, "Ldloc_1" },
			{ 8, "Ldloc_2" },
			{ 9, "Ldloc_3" },
			{ 10, "Stloc_0" },
			{ 11, "Stloc_1" },
			{ 12, "Stloc_2" },
			{ 13, "Stloc_3" },
			{ 14, "Ldarg_S" },
			{ 15, "Ldarga_S" },
			{ 16, "Starg_S" },
			{ 17, "Ldloc_S" },
			{ 18, "Ldloca_S" },
			{ 19, "Stloc_S" },
			{ 20, "Ldnull" },
			{ 21, "Ldc_I4_M1" },
			{ 22, "Ldc_I4_0" },
			{ 23, "Ldc_I4_1" },
			{ 24, "Ldc_I4_2" },
			{ 25, "Ldc_I4_3" },
			{ 26, "Ldc_I4_4" },
			{ 27, "Ldc_I4_5" },
			{ 28, "Ldc_I4_6" },
			{ 29, "Ldc_I4_7" },
			{ 30, "Ldc_I4_8" },
			{ 31, "Ldc_I4_S" },
			{ 32, "Ldc_I4" },
			{ 33, "Ldc_I8" },
			{ 34, "Ldc_R4" },
			{ 35, "Ldc_R8" },
			{ 37, "Dup" },
			{ 38, "Pop" },
			{ 39, "Jmp" },
			{ 40, "Call" },
			{ 41, "Calli" },
			{ 42, "Ret" },
			{ 43, "Br_S" },
			{ 44, "Brfalse_S" },
			{ 45, "Brtrue_S" },
			{ 46, "Beq_S" },
			{ 47, "Bge_S" },
			{ 48, "Bgt_S" },
			{ 49, "Ble_S" },
			{ 50, "Blt_S" },
			{ 51, "Bne_Un_S" },
			{ 52, "Bge_Un_S" },
			{ 53, "Bgt_Un_S" },
			{ 54, "Ble_Un_S" },
			{ 55, "Blt_Un_S" },
			{ 56, "Br" },
			{ 57, "Brfalse" },
			{ 58, "Brtrue" },
			{ 59, "Beq" },
			{ 60, "Bge" },
			{ 61, "Bgt" },
			{ 62, "Ble" },
			{ 63, "Blt" },
			{ 64, "Bne_Un" },
			{ 65, "Bge_Un" },
			{ 66, "Bgt_Un" },
			{ 67, "Ble_Un" },
			{ 68, "Blt_Un" },
			{ 69, "Switch" },
			{ 70, "Ldind_I1" },
			{ 71, "Ldind_U1" },
			{ 72, "Ldind_I2" },
			{ 73, "Ldind_U2" },
			{ 74, "Ldind_I4" },
			{ 75, "Ldind_U4" },
			{ 76, "Ldind_I8" },
			{ 77, "Ldind_I" },
			{ 78, "Ldind_R4" },
			{ 79, "Ldind_R8" },
			{ 80, "Ldind_Ref" },
			{ 81, "Stind_Ref" },
			{ 82, "Stind_I1" },
			{ 83, "Stind_I2" },
			{ 84, "Stind_I4" },
			{ 85, "Stind_I8" },
			{ 86, "Stind_R4" },
			{ 87, "Stind_R8" },
			{ 88, "Add" },
			{ 89, "Sub" },
			{ 90, "Mul" },
			{ 91, "Div" },
			{ 92, "Div_Un" },
			{ 93, "Rem" },
			{ 94, "Rem_Un" },
			{ 95, "And" },
			{ 96, "Or" },
			{ 97, "Xor" },
			{ 98, "Shl" },
			{ 99, "Shr" },
			{ 100, "Shr_Un" },
			{ 101, "Neg" },
			{ 102, "Not" },
			{ 103, "Conv_I1" },
			{ 104, "Conv_I2" },
			{ 105, "Conv_I4" },
			{ 106, "Conv_I8" },
			{ 107, "Conv_R4" },
			{ 108, "Conv_R8" },
			{ 109, "Conv_U4" },
			{ 110, "Conv_U8" },
			{ 111, "Callvirt" },
			{ 112, "Cpobj" },
			{ 113, "Ldobj" },
			{ 114, "Ldstr" },
			{ 115, "Newobj" },
			{ 116, "Castclass" },
			{ 117, "Isinst" },
			{ 118, "Conv_R_Un" },
			{ 121, "Unbox" },
			{ 122, "Throw" },
			{ 123, "Ldfld" },
			{ 124, "Ldflda" },
			{ 125, "Stfld" },
			{ 126, "Ldsfld" },
			{ 127, "Ldsflda" },
			{ 128, "Stsfld" },
			{ 129, "Stobj" },
			{ 130, "Conv_Ovf_I1_Un" },
			{ 131, "Conv_Ovf_I2_Un" },
			{ 132, "Conv_Ovf_I4_Un" },
			{ 133, "Conv_Ovf_I8_Un" },
			{ 134, "Conv_Ovf_U1_Un" },
			{ 135, "Conv_Ovf_U2_Un" },
			{ 136, "Conv_Ovf_U4_Un" },
			{ 137, "Conv_Ovf_U8_Un" },
			{ 138, "Conv_Ovf_I_Un" },
			{ 139, "Conv_Ovf_U_Un" },
			{ 140, "Box" },
			{ 141, "Newarr" },
			{ 142, "Ldlen" },
			{ 143, "Ldelema" },
			{ 144, "Ldelem_I1" },
			{ 145, "Ldelem_U1" },
			{ 146, "Ldelem_I2" },
			{ 147, "Ldelem_U2" },
			{ 148, "Ldelem_I4" },
			{ 149, "Ldelem_U4" },
			{ 150, "Ldelem_I8" },
			{ 151, "Ldelem_I" },
			{ 152, "Ldelem_R4" },
			{ 153, "Ldelem_R8" },
			{ 154, "Ldelem_Ref" },
			{ 155, "Stelem_I" },
			{ 156, "Stelem_I1" },
			{ 157, "Stelem_I2" },
			{ 158, "Stelem_I4" },
			{ 159, "Stelem_I8" },
			{ 160, "Stelem_R4" },
			{ 161, "Stelem_R8" },
			{ 162, "Stelem_Ref" },
			{ 163, "Ldelem" },
			{ 164, "Stelem" },
			{ 165, "Unbox_Any" },
			{ 179, "Conv_Ovf_I1" },
			{ 180, "Conv_Ovf_U1" },
			{ 181, "Conv_Ovf_I2" },
			{ 182, "Conv_Ovf_U2" },
			{ 183, "Conv_Ovf_I4" },
			{ 184, "Conv_Ovf_U4" },
			{ 185, "Conv_Ovf_I8" },
			{ 186, "Conv_Ovf_U8" },
			{ 194, "Refanyval" },
			{ 195, "Ckfinite" },
			{ 198, "Mkrefany" },
			{ 208, "Ldtoken" },
			{ 209, "Conv_U2" },
			{ 210, "Conv_U1" },
			{ 211, "Conv_I" },
			{ 212, "Conv_Ovf_I" },
			{ 213, "Conv_Ovf_U" },
			{ 214, "Add_Ovf" },
			{ 215, "Add_Ovf_Un" },
			{ 216, "Mul_Ovf" },
			{ 217, "Mul_Ovf_Un" },
			{ 218, "Sub_Ovf" },
			{ 219, "Sub_Ovf_Un" },
			{ 220, "Endfinally" },
			{ 221, "Leave" },
			{ 222, "Leave_S" },
			{ 223, "Stind_I" },
			{ 224, "Conv_U" },
			{ 248, "Prefix7" },
			{ 249, "Prefix6" },
			{ 250, "Prefix5" },
			{ 251, "Prefix4" },
			{ 252, "Prefix3" },
			{ 253, "Prefix2" },
			{ 254, "Prefix1" },
			{ 255, "Prefixref" },
			{ -512, "Arglist" },
			{ -511, "Ceq" },
			{ -510, "Cgt" },
			{ -509, "Cgt_Un" },
			{ -508, "Clt" },
			{ -507, "Clt_Un" },
			{ -506, "Ldftn" },
			{ -505, "Ldvirtftn" },
			{ -503, "Ldarg" },
			{ -502, "Ldarga" },
			{ -501, "Starg" },
			{ -500, "Ldloc" },
			{ -499, "Ldloca" },
			{ -498, "Stloc" },
			{ -497, "Localloc" },
			{ -495, "Endfilter" },
			{ -494, "Unaligned" },
			{ -493, "Volatile" },
			{ -492, "Tailcall" },
			{ -491, "Initobj" },
			{ -490, "Constrained" },
			{ -489, "Cpblk" },
			{ -488, "Initblk" },
			{ -486, "Rethrow" },
			{ -484, "Sizeof" },
			{ -483, "Refanytype" },
			{ -482, "Readonly" }
		};

		/// <summary>
		/// Array lookup of <see cref="OpCode"/> instances for single byte value opcodes.
		/// The index corresponds with the single byte value of the opcode.
		/// </summary>
		public static readonly OpCode[] SingleByteOpCodeLookup = new OpCode[] { OpCodes.Nop, OpCodes.Break, OpCodes.Ldarg_0, OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3, OpCodes.Ldloc_0, OpCodes.Ldloc_1, OpCodes.Ldloc_2, OpCodes.Ldloc_3, OpCodes.Stloc_0, OpCodes.Stloc_1, OpCodes.Stloc_2, OpCodes.Stloc_3, OpCodes.Ldarg_S, OpCodes.Ldarga_S, OpCodes.Starg_S, OpCodes.Ldloc_S, OpCodes.Ldloca_S, OpCodes.Stloc_S, OpCodes.Ldnull, OpCodes.Ldc_I4_M1, OpCodes.Ldc_I4_0, OpCodes.Ldc_I4_1, OpCodes.Ldc_I4_2, OpCodes.Ldc_I4_3, OpCodes.Ldc_I4_4, OpCodes.Ldc_I4_5, OpCodes.Ldc_I4_6, OpCodes.Ldc_I4_7, OpCodes.Ldc_I4_8, OpCodes.Ldc_I4_S, OpCodes.Ldc_I4, OpCodes.Ldc_I8, OpCodes.Ldc_R4, OpCodes.Ldc_R8, OpCodes.Nop, OpCodes.Dup, OpCodes.Pop, OpCodes.Jmp, OpCodes.Call, OpCodes.Calli, OpCodes.Ret, OpCodes.Br_S, OpCodes.Brfalse_S, OpCodes.Brtrue_S, OpCodes.Beq_S, OpCodes.Bge_S, OpCodes.Bgt_S, OpCodes.Ble_S, OpCodes.Blt_S, OpCodes.Bne_Un_S, OpCodes.Bge_Un_S, OpCodes.Bgt_Un_S, OpCodes.Ble_Un_S, OpCodes.Blt_Un_S, OpCodes.Br, OpCodes.Brfalse, OpCodes.Brtrue, OpCodes.Beq, OpCodes.Bge, OpCodes.Bgt, OpCodes.Ble, OpCodes.Blt, OpCodes.Bne_Un, OpCodes.Bge_Un, OpCodes.Bgt_Un, OpCodes.Ble_Un, OpCodes.Blt_Un, OpCodes.Switch, OpCodes.Ldind_I1, OpCodes.Ldind_U1, OpCodes.Ldind_I2, OpCodes.Ldind_U2, OpCodes.Ldind_I4, OpCodes.Ldind_U4, OpCodes.Ldind_I8, OpCodes.Ldind_I, OpCodes.Ldind_R4, OpCodes.Ldind_R8, OpCodes.Ldind_Ref, OpCodes.Stind_Ref, OpCodes.Stind_I1, OpCodes.Stind_I2, OpCodes.Stind_I4, OpCodes.Stind_I8, OpCodes.Stind_R4, OpCodes.Stind_R8, OpCodes.Add, OpCodes.Sub, OpCodes.Mul, OpCodes.Div, OpCodes.Div_Un, OpCodes.Rem, OpCodes.Rem_Un, OpCodes.And, OpCodes.Or, OpCodes.Xor, OpCodes.Shl, OpCodes.Shr, OpCodes.Shr_Un, OpCodes.Neg, OpCodes.Not, OpCodes.Conv_I1, OpCodes.Conv_I2, OpCodes.Conv_I4, OpCodes.Conv_I8, OpCodes.Conv_R4, OpCodes.Conv_R8, OpCodes.Conv_U4, OpCodes.Conv_U8, OpCodes.Callvirt, OpCodes.Cpobj, OpCodes.Ldobj, OpCodes.Ldstr, OpCodes.Newobj, OpCodes.Castclass, OpCodes.Isinst, OpCodes.Conv_R_Un, OpCodes.Nop, OpCodes.Nop, OpCodes.Unbox, OpCodes.Throw, OpCodes.Ldfld, OpCodes.Ldflda, OpCodes.Stfld, OpCodes.Ldsfld, OpCodes.Ldsflda, OpCodes.Stsfld, OpCodes.Stobj, OpCodes.Conv_Ovf_I1_Un, OpCodes.Conv_Ovf_I2_Un, OpCodes.Conv_Ovf_I4_Un, OpCodes.Conv_Ovf_I8_Un, OpCodes.Conv_Ovf_U1_Un, OpCodes.Conv_Ovf_U2_Un, OpCodes.Conv_Ovf_U4_Un, OpCodes.Conv_Ovf_U8_Un, OpCodes.Conv_Ovf_I_Un, OpCodes.Conv_Ovf_U_Un, OpCodes.Box, OpCodes.Newarr, OpCodes.Ldlen, OpCodes.Ldelema, OpCodes.Ldelem_I1, OpCodes.Ldelem_U1, OpCodes.Ldelem_I2, OpCodes.Ldelem_U2, OpCodes.Ldelem_I4, OpCodes.Ldelem_U4, OpCodes.Ldelem_I8, OpCodes.Ldelem_I, OpCodes.Ldelem_R4, OpCodes.Ldelem_R8, OpCodes.Ldelem_Ref, OpCodes.Stelem_I, OpCodes.Stelem_I1, OpCodes.Stelem_I2, OpCodes.Stelem_I4, OpCodes.Stelem_I8, OpCodes.Stelem_R4, OpCodes.Stelem_R8, OpCodes.Stelem_Ref, OpCodes.Ldelem, OpCodes.Stelem, OpCodes.Unbox_Any, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Conv_Ovf_I1, OpCodes.Conv_Ovf_U1, OpCodes.Conv_Ovf_I2, OpCodes.Conv_Ovf_U2, OpCodes.Conv_Ovf_I4, OpCodes.Conv_Ovf_U4, OpCodes.Conv_Ovf_I8, OpCodes.Conv_Ovf_U8, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Refanyval, OpCodes.Ckfinite, OpCodes.Nop, OpCodes.Nop, OpCodes.Mkrefany, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Ldtoken, OpCodes.Conv_U2, OpCodes.Conv_U1, OpCodes.Conv_I, OpCodes.Conv_Ovf_I, OpCodes.Conv_Ovf_U, OpCodes.Add_Ovf, OpCodes.Add_Ovf_Un, OpCodes.Mul_Ovf, OpCodes.Mul_Ovf_Un, OpCodes.Sub_Ovf, OpCodes.Sub_Ovf_Un, OpCodes.Endfinally, OpCodes.Leave, OpCodes.Leave_S, OpCodes.Stind_I, OpCodes.Conv_U, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Nop, OpCodes.Prefix7, OpCodes.Prefix6, OpCodes.Prefix5, OpCodes.Prefix4, OpCodes.Prefix3, OpCodes.Prefix2, OpCodes.Prefix1, OpCodes.Prefixref };

		/// <summary>
		/// Array lookup of <see cref="OpCode"/> instances for two byte value opcodes.
		/// The index corresponds with the 2nd byte of the opcode value.
		/// </summary>
		public static readonly OpCode[] TwoByteOpCodeLookup = new OpCode[] { OpCodes.Arglist, OpCodes.Ceq, OpCodes.Cgt, OpCodes.Cgt_Un, OpCodes.Clt, OpCodes.Clt_Un, OpCodes.Ldftn, OpCodes.Ldvirtftn, OpCodes.Nop, OpCodes.Ldarg, OpCodes.Ldarga, OpCodes.Starg, OpCodes.Ldloc, OpCodes.Ldloca, OpCodes.Stloc, OpCodes.Localloc, OpCodes.Nop, OpCodes.Endfilter, OpCodes.Unaligned, OpCodes.Volatile, OpCodes.Tailcall, OpCodes.Initobj, OpCodes.Constrained, OpCodes.Cpblk, OpCodes.Initblk, OpCodes.Nop, OpCodes.Rethrow, OpCodes.Nop, OpCodes.Sizeof, OpCodes.Refanytype, OpCodes.Readonly };

		public static int GetOperandSize(OperandType operandType)
		{
			return operandType switch
			{
				OperandType.InlineBrTarget => 4,
				OperandType.InlineField => 4,
				OperandType.InlineI => 4,
				OperandType.InlineI8 => 8,
				OperandType.InlineMethod => 4,
				OperandType.InlineNone => 0,
				OperandType.InlineR => 8,
				OperandType.InlineSig => 4,
				OperandType.InlineString => 4,

				// Note: The actual oprand size of OperandType.InlineSwitch is variable.
				// The first four bytes of the operand contain the size of the jump table array.
				// The following (4 * array size) bytes contain the target offset values that make up the array.
				OperandType.InlineSwitch => 4,

				OperandType.InlineTok => 4,
				OperandType.InlineType => 4,
				OperandType.InlineVar => 2,
				OperandType.ShortInlineBrTarget => 1,
				OperandType.ShortInlineI => 1,
				OperandType.ShortInlineR => 4,
				OperandType.ShortInlineVar => 1,

				// case OperandType.InlinePhi:
				_ => throw new NotSupportedException($"{nameof(OpCodeHelper)}.{nameof(GetOperandSize)}(..) is not supported for an {nameof(OperandType)} value of {operandType}"),
			};
		}
	}
}
