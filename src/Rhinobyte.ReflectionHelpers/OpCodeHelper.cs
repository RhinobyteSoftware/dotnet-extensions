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
			{ 0, "No-op" }, // OpCodes.Nop
			{ 1, "Break" }, // OpCodes.Break
			{ 2, "Load Argument - Index 0" }, // OpCodes.Ldarg_0
			{ 3, "Load Argument - Index 1" }, // OpCodes.Ldarg_1
			{ 4, "Load Argument - Index 2" }, // OpCodes.Ldarg_2
			{ 5, "Load Argument - Index 3" }, // OpCodes.Ldarg_3
			{ 6, "Load LocalVariable - Index 0" }, // OpCodes.Ldloc_0
			{ 7, "Load LocalVariable - Index 1" }, // OpCodes.Ldloc_1
			{ 8, "Load LocalVariable - Index 2" }, // OpCodes.Ldloc_2
			{ 9, "Load LocalVariable - Index 3" }, // OpCodes.Ldloc_3
			{ 10, "Pop Stack To LocalVariable - Index 0" }, // OpCodes.Stloc_0
			{ 11, "Pop Stack To LocalVariable - Index 1" }, // OpCodes.Stloc_1
			{ 12, "Pop Stack To LocalVariable - Index 2" }, // OpCodes.Stloc_2
			{ 13, "Pop Stack To LocalVariable - Index 3" }, // OpCodes.Stloc_3
			{ 14, "Load Argument Reference - Specified Short Form Index" },  // OpCodes.Ldarg_S
			{ 15, "Load Argument Address - Short Form" },  // OpCodes.Ldarga_S
			{ 16, "Store Top Of Stack To Argument Slot - Specified Short Form Index" }, // OpCodes.Starg_S
			{ 17, "Load LocalVariable - Specified Short Form Index" }, // OpCodes.Ldloc_S
			{ 18, "Load LocalVariable Address - Short Form" }, // OpCodes.Ldloca_S
			{ 19, "Pop Stack To LocalVariable - Specified Short Form Index" }, // OpCodes.Stloc_S
			{ 20, "Push To Stack:  Null Reference (Type O)" }, // OpCodes.Ldnull
			{ 21, "Push To Stack:  -1 (As Int32)" }, // OpCodes.Ldc_I4_M1
			{ 22, "Push To Stack:  0 (As Int32)" }, // OpCodes.Ldc_I4_0
			{ 23, "Push To Stack:  1 (As Int32)" }, // OpCodes.Ldc_I4_1
			{ 24, "Push To Stack:  2 (As Int32)" }, // OpCodes.Ldc_I4_2
			{ 25, "Push To Stack:  3 (As Int32)" }, // OpCodes.Ldc_I4_3
			{ 26, "Push To Stack:  4 (As Int32)" }, // OpCodes.Ldc_I4_4
			{ 27, "Push To Stack:  5 (As Int32)" }, // OpCodes.Ldc_I4_5
			{ 28, "Push To Stack:  6 (As Int32)" }, // OpCodes.Ldc_I4_6
			{ 29, "Push To Stack:  7 (As Int32)" }, // OpCodes.Ldc_I4_7
			{ 30, "Push To Stack:  8 (As Int32)" }, // OpCodes.Ldc_I4_8
			{ 31, "Push To Stack:  Supplied Int8 Value (As Int32)" }, // OpCodes.Ldc_I4_S
			{ 32, "Push To Stack:  Supplied Int32 Value (As Int32)" }, // OpCodes.Ldc_I4
			{ 33, "Push To Stack:  Supplied Int64 Value (As Int64)" }, // OpCodes.Ldc_I8
			{ 34, "Push To Stack:  Supplied Float32 Value (As Type F / float)" }, // OpCodes.Ldc_R4
			{ 35, "Push To Stack:  Supplied Float64 Value (As Type F / float)" }, // OpCodes.Ldc_R8
			{ 37, "Copy Top Of Stack And Push Copy To Top Of Stack" }, // OpCodes.Dup
			{ 38, "Pop Top Of Stack" }, // OpCodes.Pop
			{ 39, "Exit Current Method And Jump To Specified Method" }, // OpCodes.Jmp
			{ 40, "Call Method - Passed Method Descriptor" }, // OpCodes.Call
			{ 41, "Call Method - Indicated On Evaluation Stack" }, // OpCodes.Calli
			{ 42, "Return From Current Method - Push Return Value (If Present) To Caller's Evaluation Stack" }, // OpCodes.Ret
			{ 43, "Unconditionally Transfer Control To Target Instruction (Short Form)" }, // OpCodes.Br_S
			{ 44, "Transfer Control To Target Instruction, If value is false, a null reference, or zero (Short Form)" }, // OpCodes.Brfalse_S
			{ 45, "Transfer Control To Target Instruction, If value is true, not null, or non-zero (Short Form)" }, // OpCodes.Brtrue_S
			//{ 46, OpCodes.Beq_S }, // OpCodes.Beq_S
			//{ 47, OpCodes.Bge_S }, // OpCodes.Bge_S
			//{ 48, OpCodes.Bgt_S }, // OpCodes.Bgt_S
			//{ 49, OpCodes.Ble_S }, // OpCodes.Ble_S
			//{ 50, OpCodes.Blt_S }, // OpCodes.Blt_S
			//{ 51, OpCodes.Bne_Un_S }, // OpCodes.Bne_Un_S
			//{ 52, OpCodes.Bge_Un_S }, // OpCodes.Bge_Un_S
			//{ 53, OpCodes.Bgt_Un_S }, // OpCodes.Bgt_Un_S
			//{ 54, OpCodes.Ble_Un_S }, // OpCodes.Ble_Un_S
			//{ 55, OpCodes.Blt_Un_S }, // OpCodes.Blt_Un_S
			//{ 56, OpCodes.Br }, // OpCodes.Br
			//{ 57, OpCodes.Brfalse }, // OpCodes.Brfalse
			//{ 58, OpCodes.Brtrue }, // OpCodes.Brtrue
			//{ 59, OpCodes.Beq }, // OpCodes.Beq
			//{ 60, OpCodes.Bge }, // OpCodes.Bge
			//{ 61, OpCodes.Bgt }, // OpCodes.Bgt
			//{ 62, OpCodes.Ble }, // OpCodes.Ble
			//{ 63, OpCodes.Blt }, // OpCodes.Blt
			//{ 64, OpCodes.Bne_Un }, // OpCodes.Bne_Un
			//{ 65, OpCodes.Bge_Un }, // OpCodes.Bge_Un
			//{ 66, OpCodes.Bgt_Un }, // OpCodes.Bgt_Un
			//{ 67, OpCodes.Ble_Un }, // OpCodes.Ble_Un
			//{ 68, OpCodes.Blt_Un }, // OpCodes.Blt_Un
			//{ 69, OpCodes.Switch }, // OpCodes.Switch
			//{ 70, OpCodes.Ldind_I1 }, // OpCodes.Ldind_I1
			//{ 71, OpCodes.Ldind_U1 }, // OpCodes.Ldind_U1
			//{ 72, OpCodes.Ldind_I2 }, // OpCodes.Ldind_I2
			//{ 73, OpCodes.Ldind_U2 }, // OpCodes.Ldind_U2
			//{ 74, OpCodes.Ldind_I4 }, // OpCodes.Ldind_I4
			//{ 75, OpCodes.Ldind_U4 }, // OpCodes.Ldind_U4
			//{ 76, OpCodes.Ldind_I8 }, // OpCodes.Ldind_I8
			//{ 77, OpCodes.Ldind_I }, // OpCodes.Ldind_I
			//{ 78, OpCodes.Ldind_R4 }, // OpCodes.Ldind_R4
			//{ 79, OpCodes.Ldind_R8 }, // OpCodes.Ldind_R8
			//{ 80, OpCodes.Ldind_Ref }, // OpCodes.Ldind_Ref
			//{ 81, OpCodes.Stind_Ref }, // OpCodes.Stind_Ref
			//{ 82, OpCodes.Stind_I1 }, // OpCodes.Stind_I1
			//{ 83, OpCodes.Stind_I2 }, // OpCodes.Stind_I2
			//{ 84, OpCodes.Stind_I4 }, // OpCodes.Stind_I4
			//{ 85, OpCodes.Stind_I8 }, // OpCodes.Stind_I8
			//{ 86, OpCodes.Stind_R4 }, // OpCodes.Stind_R4
			//{ 87, OpCodes.Stind_R8 }, // OpCodes.Stind_R8
			//{ 88, OpCodes.Add }, // OpCodes.Add
			//{ 89, OpCodes.Sub }, // OpCodes.Sub
			//{ 90, OpCodes.Mul }, // OpCodes.Mul
			//{ 91, OpCodes.Div }, // OpCodes.Div
			//{ 92, OpCodes.Div_Un }, // OpCodes.Div_Un
			//{ 93, OpCodes.Rem }, // OpCodes.Rem
			//{ 94, OpCodes.Rem_Un }, // OpCodes.Rem_Un
			//{ 95, OpCodes.And }, // OpCodes.And
			//{ 96, OpCodes.Or }, // OpCodes.Or
			//{ 97, OpCodes.Xor }, // OpCodes.Xor
			//{ 98, OpCodes.Shl }, // OpCodes.Shl
			//{ 99, OpCodes.Shr }, // OpCodes.Shr
			//{ 100, OpCodes.Shr_Un }, // OpCodes.Shr_Un
			//{ 101, OpCodes.Neg }, // OpCodes.Neg
			//{ 102, OpCodes.Not }, // OpCodes.Not
			//{ 103, OpCodes.Conv_I1 }, // OpCodes.Conv_I1
			//{ 104, OpCodes.Conv_I2 }, // OpCodes.Conv_I2
			//{ 105, OpCodes.Conv_I4 }, // OpCodes.Conv_I4
			//{ 106, OpCodes.Conv_I8 }, // OpCodes.Conv_I8
			//{ 107, OpCodes.Conv_R4 }, // OpCodes.Conv_R4
			//{ 108, OpCodes.Conv_R8 }, // OpCodes.Conv_R8
			//{ 109, OpCodes.Conv_U4 }, // OpCodes.Conv_U4
			//{ 110, OpCodes.Conv_U8 }, // OpCodes.Conv_U8
			//{ 111, OpCodes.Callvirt }, // OpCodes.Callvirt
			//{ 112, OpCodes.Cpobj }, // OpCodes.Cpobj
			//{ 113, OpCodes.Ldobj }, // OpCodes.Ldobj
			//{ 114, OpCodes.Ldstr }, // OpCodes.Ldstr
			//{ 115, OpCodes.Newobj }, // OpCodes.Newobj
			//{ 116, OpCodes.Castclass }, // OpCodes.Castclass
			//{ 117, OpCodes.Isinst }, // OpCodes.Isinst
			//{ 118, OpCodes.Conv_R_Un }, // OpCodes.Conv_R_Un
			//{ 121, OpCodes.Unbox }, // OpCodes.Unbox
			//{ 122, OpCodes.Throw }, // OpCodes.Throw
			//{ 123, OpCodes.Ldfld }, // OpCodes.Ldfld
			//{ 124, OpCodes.Ldflda }, // OpCodes.Ldflda
			//{ 125, OpCodes.Stfld }, // OpCodes.Stfld
			//{ 126, OpCodes.Ldsfld }, // OpCodes.Ldsfld
			//{ 127, OpCodes.Ldsflda }, // OpCodes.Ldsflda
			//{ 128, OpCodes.Stsfld }, // OpCodes.Stsfld
			//{ 129, OpCodes.Stobj }, // OpCodes.Stobj
			//{ 130, OpCodes.Conv_Ovf_I1_Un }, // OpCodes.Conv_Ovf_I1_Un
			//{ 131, OpCodes.Conv_Ovf_I2_Un }, // OpCodes.Conv_Ovf_I2_Un
			//{ 132, OpCodes.Conv_Ovf_I4_Un }, // OpCodes.Conv_Ovf_I4_Un
			//{ 133, OpCodes.Conv_Ovf_I8_Un }, // OpCodes.Conv_Ovf_I8_Un
			//{ 134, OpCodes.Conv_Ovf_U1_Un }, // OpCodes.Conv_Ovf_U1_Un
			//{ 135, OpCodes.Conv_Ovf_U2_Un }, // OpCodes.Conv_Ovf_U2_Un
			//{ 136, OpCodes.Conv_Ovf_U4_Un }, // OpCodes.Conv_Ovf_U4_Un
			//{ 137, OpCodes.Conv_Ovf_U8_Un }, // OpCodes.Conv_Ovf_U8_Un
			//{ 138, OpCodes.Conv_Ovf_I_Un }, // OpCodes.Conv_Ovf_I_Un
			//{ 139, OpCodes.Conv_Ovf_U_Un }, // OpCodes.Conv_Ovf_U_Un
			//{ 140, OpCodes.Box }, // OpCodes.Box
			//{ 141, OpCodes.Newarr }, // OpCodes.Newarr
			//{ 142, OpCodes.Ldlen }, // OpCodes.Ldlen
			//{ 143, OpCodes.Ldelema }, // OpCodes.Ldelema
			//{ 144, OpCodes.Ldelem_I1 }, // OpCodes.Ldelem_I1
			//{ 145, OpCodes.Ldelem_U1 }, // OpCodes.Ldelem_U1
			//{ 146, OpCodes.Ldelem_I2 }, // OpCodes.Ldelem_I2
			//{ 147, OpCodes.Ldelem_U2 }, // OpCodes.Ldelem_U2
			//{ 148, OpCodes.Ldelem_I4 }, // OpCodes.Ldelem_I4
			//{ 149, OpCodes.Ldelem_U4 }, // OpCodes.Ldelem_U4
			//{ 150, OpCodes.Ldelem_I8 }, // OpCodes.Ldelem_I8
			//{ 151, OpCodes.Ldelem_I }, // OpCodes.Ldelem_I
			//{ 152, OpCodes.Ldelem_R4 }, // OpCodes.Ldelem_R4
			//{ 153, OpCodes.Ldelem_R8 }, // OpCodes.Ldelem_R8
			//{ 154, OpCodes.Ldelem_Ref }, // OpCodes.Ldelem_Ref
			//{ 155, OpCodes.Stelem_I }, // OpCodes.Stelem_I
			//{ 156, OpCodes.Stelem_I1 }, // OpCodes.Stelem_I1
			//{ 157, OpCodes.Stelem_I2 }, // OpCodes.Stelem_I2
			//{ 158, OpCodes.Stelem_I4 }, // OpCodes.Stelem_I4
			//{ 159, OpCodes.Stelem_I8 }, // OpCodes.Stelem_I8
			//{ 160, OpCodes.Stelem_R4 }, // OpCodes.Stelem_R4
			//{ 161, OpCodes.Stelem_R8 }, // OpCodes.Stelem_R8
			//{ 162, OpCodes.Stelem_Ref }, // OpCodes.Stelem_Ref
			//{ 163, OpCodes.Ldelem }, // OpCodes.Ldelem
			//{ 164, OpCodes.Stelem }, // OpCodes.Stelem
			//{ 165, OpCodes.Unbox_Any }, // OpCodes.Unbox_Any
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
	}
}
