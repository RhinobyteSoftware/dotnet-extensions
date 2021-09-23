using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhinobyte.Extensions.Reflection.IntermediateLanguage;
using Rhinobyte.Extensions.Reflection.Tests.Setup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static FluentAssertions.FluentActions;

namespace Rhinobyte.Extensions.Reflection.Tests.IntermediateLanguage
{
	[TestClass]
	public class MethodBodyParserUnitTests
	{
		/******     TEST METHODS     ****************************
		 ********************************************************/

		[TestMethod]
		public void Constructor_throws_argument_exceptions_for_required_parameter_members()
		{
			Invoking(() => new MethodBodyParser(null!))
				.Should()
				.Throw<ArgumentNullException>()
				.WithMessage("Value cannot be null*method*");

			var mockMethodBase = new MockMethodBase("MockMethod");
			Invoking(() => new MethodBodyParser(mockMethodBase))
				.Should()
				.Throw<ArgumentException>()
				.WithMessage("*.GetMethodBody() returned null for the method:*");

			mockMethodBase.SetMethodBody(new MockMethodBody());
			Invoking(() => new MethodBodyParser(mockMethodBase))
				.Should()
				.Throw<ArgumentException>()
				.WithMessage("MethodBody.GetILAsByteArray() returned null for the method:*");

			var actualMethod = typeof(ExampleMethods).GetMethod("AddTwoValues", BindingFlags.Public | BindingFlags.Static);
			mockMethodBase.SetMethodBody(actualMethod!.GetMethodBody());
			Invoking(() => new MethodBodyParser(mockMethodBase))
				.Should()
				.Throw<ArgumentException>()
				.WithMessage("*.Module property is null");

			mockMethodBase = new MockMethodBase(null, "MockMethodNullDeclaringType");
			mockMethodBase.SetMethodBody(actualMethod.GetMethodBody());
			mockMethodBase.SetModule(actualMethod.Module);
			mockMethodBase.DeclaringType.Should().BeNull();

			// Should be able handle the null DeclaringType without throwing
			var methodBodyParser = new MethodBodyParser(mockMethodBase);
			methodBodyParser.Should().NotBeNull();
		}

		[TestMethod]
		public void ContainsReferencesToAll_doesnt_blow_up1()
		{
			var methodReferencesToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

			var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
			methodBodyParser.ContainsReferencesToAll(methodReferencesToSearchFor).Should().BeFalse();
		}

		[TestMethod]
		public void ContainsReferenceTo_doesnt_blow_up1()
		{
			var methodReferenceToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).First(method => method.Name == "WriteLine");

			var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
			methodBodyParser.ContainsReferenceTo(methodReferenceToSearchFor).Should().BeFalse();
		}

		[TestMethod]
		public void ContainsReferenceToAny_doesnt_blow_up1()
		{
			var methodReferencesToSearchFor = typeof(System.Console).GetMethods(BindingFlags.Public | BindingFlags.Static).ToList();

			var methodBodyParser = new MethodBodyParser(_dynamicJumpTableMethod);
			methodBodyParser.ContainsReferenceToAny(methodReferencesToSearchFor).Should().BeFalse();
		}

		[TestMethod]
		public void ParseInstructions_handles_instance_method_parameters_correctly()
		{
			var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.InstanceMethodWithLotsOfParameters), BindingFlags.Public | BindingFlags.Instance);
			testMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(29);

			var thisKeywordInstruction = instructions.FirstOrDefault(instruction => instruction is ThisKeywordInstruction) as ThisKeywordInstruction;
			thisKeywordInstruction.Should().NotBeNull();
			thisKeywordInstruction!.Method.Should().Be(testMethodInfo);

			var description = new DefaultInstructionFormatter().DescribeInstructions(instructions);
			description.Should().Be(
@"(0) NO-OP
(1) LOAD ARGUMENT (Index 0)  [this keyword]
(2) CALL METHOD  [ExampleMethods.get_LocalIntegerProperty]
(3) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(4) ADD
(5) LOAD ARGUMENT (Index 2)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(6) ADD
(7) LOAD ARGUMENT (Index 3)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(8) ADD
(9) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(10) ADD
(11) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(12) ADD
(13) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(14) ADD
(15) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(16) ADD
(17) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(18) ADD
(19) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(20) ADD
(21) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(22) ADD
(23) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(25) SET LOCAL VARIABLE (Index 1)  [Of type Int32]
(26) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 27]
(27) LOAD LOCAL VARIABLE (Index 1)  [Of type Int32]
(28) RETURN");
		}

		[TestMethod]
		public void ParseInstructions_handles_static_method_parameters_correctly()
		{
			var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.StaticMethodWithLotsOfParameters), BindingFlags.Public | BindingFlags.Static);
			testMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(26);

			var description = new DefaultInstructionFormatter().DescribeInstructions(instructions);
			description.Should().Be(
@"(0) NO-OP
(1) LOAD ARGUMENT (Index 0)  [Parameter #0]  [ParameterReference: System.Int32 value1]
(2) LOAD ARGUMENT (Index 1)  [Parameter #1]  [ParameterReference: System.Int32 value2]
(3) ADD
(4) LOAD ARGUMENT (Index 2)  [Parameter #2]  [ParameterReference: System.Int32 value3]
(5) ADD
(6) LOAD ARGUMENT (Index 3)  [Parameter #3]  [ParameterReference: System.Int32 value4]
(7) ADD
(8) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #4]  [ParameterReference: System.Int32 value5]
(9) ADD
(10) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #5]  [ParameterReference: System.Int32 value6]
(11) ADD
(12) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #6]  [ParameterReference: System.Int32 value7]
(13) ADD
(14) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #7]  [ParameterReference: System.Int32 value8]
(15) ADD
(16) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #8]  [ParameterReference: System.Int32 value9]
(17) ADD
(18) LOAD ARGUMENT (Specified Short Form Index)  [Parameter #9]  [ParameterReference: System.Int32 value10]
(19) ADD
(20) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(21) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(22) SET LOCAL VARIABLE (Index 1)  [Of type Int32]
(23) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 24]
(24) LOAD LOCAL VARIABLE (Index 1)  [Of type Int32]
(25) RETURN");
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result1()
		{
			var methodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddLocalVariables_For_5_And_15), BindingFlags.Public | BindingFlags.Static);
			methodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(methodInfo!).ParseInstructions();
			instructions.Count.Should().Be(12);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result2()
		{
			var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type1), BindingFlags.Public | BindingFlags.Static);
			nullCheckMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(15);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result3()
		{
			var nullCheckMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.NullParameterCheck_Type2), BindingFlags.Public | BindingFlags.Static);
			nullCheckMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(nullCheckMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(11);

			//var results = string.Join($"{System.Environment.NewLine}{System.Environment.NewLine}{System.Environment.NewLine}", instructions.Select(instruction => instruction.FullDescription()));
			//results.Should().NotBeNullOrEmpty();
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result4()
		{
			var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.MethodWithEachTypeOfInstruction), BindingFlags.Public | BindingFlags.Instance);
			testMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
			var instructionsDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);

			var instructionTypes = new HashSet<Type>();
			foreach (var instruction in instructions)
			{
				instructionTypes.Add(instruction.GetType());
			}

			// Release/optimized build will have different IL causing our test expectations will fail
			var debuggableAttribute = typeof(ExampleMethods).Assembly.GetCustomAttribute<System.Diagnostics.DebuggableAttribute>();
			var assemblyIsDebugBuildWithoutOptimizations = debuggableAttribute?.IsJITOptimizerDisabled == true;


			if (assemblyIsDebugBuildWithoutOptimizations)
			{
				instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
				//instructionTypes.Should().Contain(typeof(ByteInstruction));
				instructionTypes.Should().Contain(typeof(DoubleInstruction));
				instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
				instructionTypes.Should().Contain(typeof(FloatInstruction));
				instructionTypes.Should().Contain(typeof(Int32Instruction));
				instructionTypes.Should().Contain(typeof(Int64Instruction));
				instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
				instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
				instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
				//instructionTypes.Should().Contain(typeof(SignatureInstruction));
				instructionTypes.Should().Contain(typeof(SignedByteInstruction));
				instructionTypes.Should().Contain(typeof(SimpleInstruction));
				instructionTypes.Should().Contain(typeof(StringInstruction));
				//instructionTypes.Should().Contain(typeof(SwitchInstruction));
				instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
				instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

				// DEBUG (Non Optimizied) Build
				instructions.Count.Should().Be(320);
				instructionsDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (5)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) LOAD INT VALUE (Int8)  [SByte Value: 12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type Byte]
(5) LOAD INT VALUE (Int8)  [SByte Value: -12]
(6) SET LOCAL VARIABLE (Index 2)  [Of type SByte]
(7) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(8) SET LOCAL VARIABLE (Index 3)  [Of type Int16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(10) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(11) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(12) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(13) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(14) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(15) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(16) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(18) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(19) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(20) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(21) SUBTRACT
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int32]
(25) CALL METHOD  [ExampleMethods.AddTwoValues]
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(28) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(29) MULTIPLY
(30) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(31) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(32) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(33) ADD
(34) LOAD LOCAL VARIABLE (Index 2)  [Of type SByte]
(35) ADD
(36) LOAD LOCAL VARIABLE (Index 3)  [Of type Int16]
(37) ADD
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt16]
(39) ADD
(40) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Int32]
(41) ADD
(42) CONVERT (Int64)
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt32]
(44) CONVERT (UInt64)
(45) ADD
(46) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int64]
(47) ADD
(48) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type UInt64]
(49) ADD
(50) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(51) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(52) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(53) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(54) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(55) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Int64]
(56) CONVERT (Float64)
(57) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type Double]
(58) ADD
(59) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type Single]
(60) CONVERT (Float64)
(61) ADD
(62) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(63) LOAD STRING  [String Value: SomeString]
(64) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(65) LOAD STRING  [String Value: {0}:  {1} {2}  - {3}.{4}: {5}]
(66) LOAD INT LITERAL (6)
(67) NEW ARRAY  [TypeReference: Object]
(68) DUPLICATE
(69) LOAD INT LITERAL (0)
(70) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(71) STORE ARRAY ELEMENT (Object Reference)
(72) DUPLICATE
(73) LOAD INT LITERAL (1)
(74) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type String]
(75) STORE ARRAY ELEMENT (Object Reference)
(76) DUPLICATE
(77) LOAD INT LITERAL (2)
(78) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type Double]
(79) BOX VALUE  [TypeReference: Double]
(80) STORE ARRAY ELEMENT (Object Reference)
(81) DUPLICATE
(82) LOAD INT LITERAL (3)
(83) LOAD TOKEN  [TypeReference: ExampleMethods]
(84) CALL METHOD  [Type.GetTypeFromHandle]
(85) CALL VIRTUAL  [Type.get_FullName]
(86) STORE ARRAY ELEMENT (Object Reference)
(87) DUPLICATE
(88) LOAD INT LITERAL (4)
(89) LOAD STRING  [String Value: LocalStringField]
(90) STORE ARRAY ELEMENT (Object Reference)
(91) DUPLICATE
(92) LOAD INT LITERAL (5)
(93) LOAD ARGUMENT (Index 0)  [this keyword]
(94) LOAD FIELD  [FieldReference: LocalStringField]
(95) STORE ARRAY ELEMENT (Object Reference)
(96) CALL METHOD  [String.Format]
(97) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(98) LOAD LOCAL VARIABLE (Index 1)  [Of type Byte]
(99) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(100) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 18)  [Of type Byte]
(101) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(102) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(103) LOAD INT LITERAL (1)
(104) SUBTRACT
(105) LOAD INT LITERAL (3)
(106) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 112]
(107) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 108]
(108) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 17)  [Of type Byte]
(109) LOAD INT LITERAL (5)
(110) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 117]
(111) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 122]
(112) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(113) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(114) CALL METHOD  [String.Concat]
(115) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(116) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(117) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(118) LOAD STRING  [String Value:   - switch statement case 5]
(119) CALL METHOD  [String.Concat]
(120) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(121) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(122) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(123) LOAD STRING  [String Value:   - switch statement default case]
(124) CALL METHOD  [String.Concat]
(125) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(126) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 127]
(127) LOAD STRING  [String Value: test]
(128) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(129) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(130) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(131) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 20)  [Of type String]
(132) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(133) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(134) CALL METHOD  [<PrivateImplementationDetails>.ComputeStringHash]
(135) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(136) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(137) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(138) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 162]
(139) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(140) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(141) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 150]
(142) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(143) LOAD INT VALUE (Int32)  [Int32 Value: -1792857488]
(144) BRANCH WHEN EQUAL  [TargetInstruction: 209]
(145) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 146]
(146) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(147) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(148) BRANCH WHEN EQUAL  [TargetInstruction: 204]
(149) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(150) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(151) LOAD INT VALUE (Int32)  [Int32 Value: -1759302250]
(152) BRANCH WHEN EQUAL  [TargetInstruction: 219]
(153) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 154]
(154) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(155) LOAD INT VALUE (Int32)  [Int32 Value: -1742524631]
(156) BRANCH WHEN EQUAL  [TargetInstruction: 214]
(157) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 158]
(158) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(159) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(160) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 189]
(161) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(162) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(163) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(164) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 177]
(165) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(166) LOAD INT VALUE (Int32)  [Int32 Value: -1692191774]
(167) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 199]
(168) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 169]
(169) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(170) LOAD INT VALUE (Int32)  [Int32 Value: -1675414155]
(171) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 194]
(172) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 173]
(173) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(174) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(175) BRANCH WHEN EQUAL  [TargetInstruction: 234]
(176) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(177) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(178) LOAD INT VALUE (Int32)  [Int32 Value: -1620742665]
(179) BRANCH WHEN EQUAL  [TargetInstruction: 239]
(180) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 181]
(181) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(182) LOAD INT VALUE (Int32)  [Int32 Value: -1591526060]
(183) BRANCH WHEN EQUAL  [TargetInstruction: 229]
(184) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 185]
(185) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 21)  [Of type UInt32]
(186) LOAD INT VALUE (Int32)  [Int32 Value: -1574748441]
(187) BRANCH WHEN EQUAL  [TargetInstruction: 224]
(188) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(189) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(190) LOAD STRING  [String Value: test1]
(191) CALL METHOD  [String.op_Equality]
(192) BRANCH WHEN TRUE  [TargetInstruction: 244]
(193) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(194) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(195) LOAD STRING  [String Value: test2]
(196) CALL METHOD  [String.op_Equality]
(197) BRANCH WHEN TRUE  [TargetInstruction: 250]
(198) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(199) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(200) LOAD STRING  [String Value: test3]
(201) CALL METHOD  [String.op_Equality]
(202) BRANCH WHEN TRUE  [TargetInstruction: 256]
(203) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(204) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(205) LOAD STRING  [String Value: test4]
(206) CALL METHOD  [String.op_Equality]
(207) BRANCH WHEN TRUE  [TargetInstruction: 262]
(208) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(209) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(210) LOAD STRING  [String Value: test5]
(211) CALL METHOD  [String.op_Equality]
(212) BRANCH WHEN TRUE  [TargetInstruction: 268]
(213) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(214) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(215) LOAD STRING  [String Value: test6]
(216) CALL METHOD  [String.op_Equality]
(217) BRANCH WHEN TRUE  [TargetInstruction: 274]
(218) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(219) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(220) LOAD STRING  [String Value: test7]
(221) CALL METHOD  [String.op_Equality]
(222) BRANCH WHEN TRUE  [TargetInstruction: 280]
(223) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(224) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(225) LOAD STRING  [String Value: test8]
(226) CALL METHOD  [String.op_Equality]
(227) BRANCH WHEN TRUE  [TargetInstruction: 286]
(228) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(229) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(230) LOAD STRING  [String Value: test9]
(231) CALL METHOD  [String.op_Equality]
(232) BRANCH WHEN TRUE  [TargetInstruction: 292]
(233) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(234) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(235) LOAD STRING  [String Value: test10]
(236) CALL METHOD  [String.op_Equality]
(237) BRANCH WHEN TRUE  [TargetInstruction: 298]
(238) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(239) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 19)  [Of type String]
(240) LOAD STRING  [String Value: test11]
(241) CALL METHOD  [String.op_Equality]
(242) BRANCH WHEN TRUE  [TargetInstruction: 304]
(243) BRANCH UNCONDITIONALLY  [TargetInstruction: 310]
(244) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(245) LOAD STRING  [String Value:   - string switch statement case ]
(246) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(247) CALL METHOD  [String.Concat]
(248) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(249) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(250) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(251) LOAD STRING  [String Value:   - string switch statement case ]
(252) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(253) CALL METHOD  [String.Concat]
(254) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(255) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(256) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(257) LOAD STRING  [String Value:   - string switch statement case ]
(258) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(259) CALL METHOD  [String.Concat]
(260) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(261) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(262) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(263) LOAD STRING  [String Value:   - string switch statement case ]
(264) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(265) CALL METHOD  [String.Concat]
(266) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(267) BRANCH UNCONDITIONALLY  [TargetInstruction: 315]
(268) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(269) LOAD STRING  [String Value:   - string switch statement case ]
(270) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(271) CALL METHOD  [String.Concat]
(272) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(273) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(274) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(275) LOAD STRING  [String Value:   - string switch statement case ]
(276) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(277) CALL METHOD  [String.Concat]
(278) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(279) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(280) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(281) LOAD STRING  [String Value:   - string switch statement case ]
(282) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(283) CALL METHOD  [String.Concat]
(284) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(285) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(286) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(287) LOAD STRING  [String Value:   - string switch statement case ]
(288) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(289) CALL METHOD  [String.Concat]
(290) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(291) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(292) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(293) LOAD STRING  [String Value:   - string switch statement case ]
(294) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(295) CALL METHOD  [String.Concat]
(296) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(297) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(298) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(299) LOAD STRING  [String Value:   - string switch statement case ]
(300) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(301) CALL METHOD  [String.Concat]
(302) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(303) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(304) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(305) LOAD STRING  [String Value:   - string switch statement case ]
(306) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 16)  [Of type String]
(307) CALL METHOD  [String.Concat]
(308) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(309) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(310) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(311) LOAD STRING  [String Value:   - string switch statement default case]
(312) CALL METHOD  [String.Concat]
(313) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(314) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 315]
(315) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 15)  [Of type String]
(316) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type String]
(317) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 318]
(318) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 22)  [Of type String]
(319) RETURN");
			}
			else
			{
				instructionTypes.Should().Contain(typeof(BranchTargetInstruction));
				//instructionTypes.Should().Contain(typeof(ByteInstruction));
				instructionTypes.Should().Contain(typeof(DoubleInstruction));
				instructionTypes.Should().Contain(typeof(FieldReferenceInstruction));
				instructionTypes.Should().Contain(typeof(FloatInstruction));
				instructionTypes.Should().Contain(typeof(Int32Instruction));
				instructionTypes.Should().Contain(typeof(Int64Instruction));
				instructionTypes.Should().Contain(typeof(LocalVariableInstruction));
				instructionTypes.Should().Contain(typeof(MethodReferenceInstruction));
				instructionTypes.Should().Contain(typeof(ParameterReferenceInstruction));
				//instructionTypes.Should().Contain(typeof(SignatureInstruction));
				instructionTypes.Should().Contain(typeof(SimpleInstruction));
				instructionTypes.Should().Contain(typeof(StringInstruction));
				//instructionTypes.Should().Contain(typeof(SwitchInstruction));
				instructionTypes.Should().Contain(typeof(ThisKeywordInstruction));
				instructionTypes.Should().Contain(typeof(TypeReferenceInstruction));

				// RELEASE (Optimizied) Build
				instructions.Count.Should().Be(292);
				instructionsDescription.Should().Be(
@"(0) LOAD INT LITERAL (5)
(1) LOAD INT VALUE (Int8)  [SByte Value: 12]
(2) SET LOCAL VARIABLE (Index 0)  [Of type Byte]
(3) LOAD INT VALUE (Int8)  [SByte Value: -12]
(4) SET LOCAL VARIABLE (Index 1)  [Of type SByte]
(5) LOAD INT VALUE (Int32)  [Int32 Value: -300]
(6) SET LOCAL VARIABLE (Index 2)  [Of type Int16]
(7) LOAD INT VALUE (Int32)  [Int32 Value: 65000]
(8) SET LOCAL VARIABLE (Index 3)  [Of type UInt16]
(9) LOAD INT VALUE (Int32)  [Int32 Value: -70000]
(10) LOAD INT VALUE (Int32)  [Int32 Value: 70000]
(11) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(12) LOAD INT VALUE (Int64)  [Int64 Value: -3000000000]
(13) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(14) LOAD INT VALUE (Int64)  [Int64 Value: -8446744073709551616]
(15) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(16) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(17) LOAD INT VALUE (Int64)  [Int64 Value: 9223372036854775807]
(18) SUBTRACT
(19) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(20) DUPLICATE
(21) CALL METHOD  [ExampleMethods.AddTwoValues]
(22) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(23) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(24) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(25) MULTIPLY
(26) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(27) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(28) ADD
(29) LOAD LOCAL VARIABLE (Index 1)  [Of type SByte]
(30) ADD
(31) LOAD LOCAL VARIABLE (Index 2)  [Of type Int16]
(32) ADD
(33) LOAD LOCAL VARIABLE (Index 3)  [Of type UInt16]
(34) ADD
(35) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 7)  [Of type Int32]
(36) ADD
(37) CONVERT (Int64)
(38) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 4)  [Of type UInt32]
(39) CONVERT (UInt64)
(40) ADD
(41) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 5)  [Of type Int64]
(42) ADD
(43) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 6)  [Of type UInt64]
(44) ADD
(45) LOAD FLOAT VALUE (Float64)  [Double Value: 0.5]
(46) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(47) LOAD FLOAT VALUE (Float32)  [Float Value: 0.5]
(48) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(49) CONVERT (Float64)
(50) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 8)  [Of type Double]
(51) ADD
(52) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 9)  [Of type Single]
(53) CONVERT (Float64)
(54) ADD
(55) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(56) LOAD STRING  [String Value: SomeString]
(57) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(58) LOAD STRING  [String Value: {0}:  {1} {2}  - {3}.{4}: {5}]
(59) LOAD INT LITERAL (6)
(60) NEW ARRAY  [TypeReference: Object]
(61) DUPLICATE
(62) LOAD INT LITERAL (0)
(63) LOAD ARGUMENT (Index 1)  [Parameter #0]  [ParameterReference: System.String prefix]
(64) STORE ARRAY ELEMENT (Object Reference)
(65) DUPLICATE
(66) LOAD INT LITERAL (1)
(67) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 11)  [Of type String]
(68) STORE ARRAY ELEMENT (Object Reference)
(69) DUPLICATE
(70) LOAD INT LITERAL (2)
(71) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 10)  [Of type Double]
(72) BOX VALUE  [TypeReference: Double]
(73) STORE ARRAY ELEMENT (Object Reference)
(74) DUPLICATE
(75) LOAD INT LITERAL (3)
(76) LOAD TOKEN  [TypeReference: ExampleMethods]
(77) CALL METHOD  [Type.GetTypeFromHandle]
(78) CALL VIRTUAL  [Type.get_FullName]
(79) STORE ARRAY ELEMENT (Object Reference)
(80) DUPLICATE
(81) LOAD INT LITERAL (4)
(82) LOAD STRING  [String Value: LocalStringField]
(83) STORE ARRAY ELEMENT (Object Reference)
(84) DUPLICATE
(85) LOAD INT LITERAL (5)
(86) LOAD ARGUMENT (Index 0)  [this keyword]
(87) LOAD FIELD  [FieldReference: LocalStringField]
(88) STORE ARRAY ELEMENT (Object Reference)
(89) CALL METHOD  [String.Format]
(90) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(91) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(92) LOAD INT LITERAL (1)
(93) SUBTRACT
(94) LOAD INT LITERAL (3)
(95) BRANCH WHEN LESS THAN OR EQUAL (Unsigned, Short Form)  [TargetInstruction: 100]
(96) LOAD LOCAL VARIABLE (Index 0)  [Of type Byte]
(97) LOAD INT LITERAL (5)
(98) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 105]
(99) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 110]
(100) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(101) LOAD STRING  [String Value:   - switch statement case 1/2/3/4]
(102) CALL METHOD  [String.Concat]
(103) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(104) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 114]
(105) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(106) LOAD STRING  [String Value:   - switch statement case 5]
(107) CALL METHOD  [String.Concat]
(108) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(109) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 114]
(110) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(111) LOAD STRING  [String Value:   - switch statement default case]
(112) CALL METHOD  [String.Concat]
(113) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(114) LOAD STRING  [String Value: test]
(115) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(116) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(117) CALL METHOD  [<PrivateImplementationDetails>.ComputeStringHash]
(118) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(119) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(120) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(121) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 142]
(122) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(123) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(124) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 132]
(125) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(126) LOAD INT VALUE (Int32)  [Int32 Value: -1792857488]
(127) BRANCH WHEN EQUAL  [TargetInstruction: 185]
(128) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(129) LOAD INT VALUE (Int32)  [Int32 Value: -1776079869]
(130) BRANCH WHEN EQUAL  [TargetInstruction: 180]
(131) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(132) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(133) LOAD INT VALUE (Int32)  [Int32 Value: -1759302250]
(134) BRANCH WHEN EQUAL  [TargetInstruction: 195]
(135) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(136) LOAD INT VALUE (Int32)  [Int32 Value: -1742524631]
(137) BRANCH WHEN EQUAL  [TargetInstruction: 190]
(138) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(139) LOAD INT VALUE (Int32)  [Int32 Value: -1725747012]
(140) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 165]
(141) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(142) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(143) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(144) BRANCH WHEN GREATER THAN (Unsigned, Short Form)  [TargetInstruction: 155]
(145) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(146) LOAD INT VALUE (Int32)  [Int32 Value: -1692191774]
(147) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 175]
(148) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(149) LOAD INT VALUE (Int32)  [Int32 Value: -1675414155]
(150) BRANCH WHEN EQUAL (Short Form)  [TargetInstruction: 170]
(151) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(152) LOAD INT VALUE (Int32)  [Int32 Value: -1637520284]
(153) BRANCH WHEN EQUAL  [TargetInstruction: 210]
(154) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(155) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(156) LOAD INT VALUE (Int32)  [Int32 Value: -1620742665]
(157) BRANCH WHEN EQUAL  [TargetInstruction: 215]
(158) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(159) LOAD INT VALUE (Int32)  [Int32 Value: -1591526060]
(160) BRANCH WHEN EQUAL  [TargetInstruction: 205]
(161) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 14)  [Of type UInt32]
(162) LOAD INT VALUE (Int32)  [Int32 Value: -1574748441]
(163) BRANCH WHEN EQUAL  [TargetInstruction: 200]
(164) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(165) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(166) LOAD STRING  [String Value: test1]
(167) CALL METHOD  [String.op_Equality]
(168) BRANCH WHEN TRUE  [TargetInstruction: 220]
(169) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(170) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(171) LOAD STRING  [String Value: test2]
(172) CALL METHOD  [String.op_Equality]
(173) BRANCH WHEN TRUE  [TargetInstruction: 226]
(174) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(175) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(176) LOAD STRING  [String Value: test3]
(177) CALL METHOD  [String.op_Equality]
(178) BRANCH WHEN TRUE  [TargetInstruction: 232]
(179) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(180) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(181) LOAD STRING  [String Value: test4]
(182) CALL METHOD  [String.op_Equality]
(183) BRANCH WHEN TRUE  [TargetInstruction: 238]
(184) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(185) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(186) LOAD STRING  [String Value: test5]
(187) CALL METHOD  [String.op_Equality]
(188) BRANCH WHEN TRUE  [TargetInstruction: 244]
(189) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(190) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(191) LOAD STRING  [String Value: test6]
(192) CALL METHOD  [String.op_Equality]
(193) BRANCH WHEN TRUE  [TargetInstruction: 250]
(194) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(195) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(196) LOAD STRING  [String Value: test7]
(197) CALL METHOD  [String.op_Equality]
(198) BRANCH WHEN TRUE  [TargetInstruction: 256]
(199) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(200) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(201) LOAD STRING  [String Value: test8]
(202) CALL METHOD  [String.op_Equality]
(203) BRANCH WHEN TRUE  [TargetInstruction: 262]
(204) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(205) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(206) LOAD STRING  [String Value: test9]
(207) CALL METHOD  [String.op_Equality]
(208) BRANCH WHEN TRUE  [TargetInstruction: 268]
(209) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(210) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(211) LOAD STRING  [String Value: test10]
(212) CALL METHOD  [String.op_Equality]
(213) BRANCH WHEN TRUE  [TargetInstruction: 274]
(214) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(215) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(216) LOAD STRING  [String Value: test11]
(217) CALL METHOD  [String.op_Equality]
(218) BRANCH WHEN TRUE  [TargetInstruction: 280]
(219) BRANCH UNCONDITIONALLY  [TargetInstruction: 286]
(220) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(221) LOAD STRING  [String Value:   - string switch statement case ]
(222) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(223) CALL METHOD  [String.Concat]
(224) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(225) BRANCH UNCONDITIONALLY  [TargetInstruction: 290]
(226) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(227) LOAD STRING  [String Value:   - string switch statement case ]
(228) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(229) CALL METHOD  [String.Concat]
(230) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(231) BRANCH UNCONDITIONALLY  [TargetInstruction: 290]
(232) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(233) LOAD STRING  [String Value:   - string switch statement case ]
(234) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(235) CALL METHOD  [String.Concat]
(236) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(237) BRANCH UNCONDITIONALLY  [TargetInstruction: 290]
(238) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(239) LOAD STRING  [String Value:   - string switch statement case ]
(240) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(241) CALL METHOD  [String.Concat]
(242) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(243) BRANCH UNCONDITIONALLY  [TargetInstruction: 290]
(244) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(245) LOAD STRING  [String Value:   - string switch statement case ]
(246) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(247) CALL METHOD  [String.Concat]
(248) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(249) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(250) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(251) LOAD STRING  [String Value:   - string switch statement case ]
(252) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(253) CALL METHOD  [String.Concat]
(254) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(255) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(256) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(257) LOAD STRING  [String Value:   - string switch statement case ]
(258) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(259) CALL METHOD  [String.Concat]
(260) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(261) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(262) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(263) LOAD STRING  [String Value:   - string switch statement case ]
(264) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(265) CALL METHOD  [String.Concat]
(266) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(267) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(268) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(269) LOAD STRING  [String Value:   - string switch statement case ]
(270) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(271) CALL METHOD  [String.Concat]
(272) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(273) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(274) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(275) LOAD STRING  [String Value:   - string switch statement case ]
(276) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(277) CALL METHOD  [String.Concat]
(278) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(279) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(280) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(281) LOAD STRING  [String Value:   - string switch statement case ]
(282) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 13)  [Of type String]
(283) CALL METHOD  [String.Concat]
(284) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(285) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 290]
(286) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(287) LOAD STRING  [String Value:   - string switch statement default case]
(288) CALL METHOD  [String.Concat]
(289) SET LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(290) LOAD LOCAL VARIABLE (Specified Short Form Index)  (Index 12)  [Of type String]
(291) RETURN");
			}
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result5()
		{
			var testMethodInfo = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.MethodWithGotoLabels), BindingFlags.Public | BindingFlags.Instance);
			testMethodInfo.Should().NotBeNull();

			var instructions = new MethodBodyParser(testMethodInfo!).ParseInstructions();
			instructions.Count.Should().Be(46);

			var instructionDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);
			instructionDescription.Should().Be(
@"(0) NO-OP
(1) LOAD INT LITERAL (0)
(2) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(3) NO-OP
(4) LOAD STRING  [String Value: First Label]
(5) CALL METHOD  [Console.WriteLine]
(6) NO-OP
(7) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(8) LOAD INT LITERAL (1)
(9) ADD
(10) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(11) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(12) LOAD INT LITERAL (3)
(13) COMPARE (LessThan)
(14) SET LOCAL VARIABLE (Index 1)  [Of type Boolean]
(15) LOAD LOCAL VARIABLE (Index 1)  [Of type Boolean]
(16) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 18]
(17) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 3]
(18) NO-OP
(19) LOAD STRING  [String Value: Second Label]
(20) CALL METHOD  [Console.WriteLine]
(21) NO-OP
(22) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(23) LOAD INT LITERAL (1)
(24) ADD
(25) SET LOCAL VARIABLE (Index 0)  [Of type Int32]
(26) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(27) LOAD INT LITERAL (6)
(28) COMPARE (LessThan)
(29) SET LOCAL VARIABLE (Index 2)  [Of type Boolean]
(30) LOAD LOCAL VARIABLE (Index 2)  [Of type Boolean]
(31) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 33]
(32) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 3]
(33) LOAD LOCAL VARIABLE (Index 0)  [Of type Int32]
(34) LOAD INT VALUE (Int8)  [SByte Value: 9]
(35) COMPARE (LessThan)
(36) SET LOCAL VARIABLE (Index 3)  [Of type Boolean]
(37) LOAD LOCAL VARIABLE (Index 3)  [Of type Boolean]
(38) BRANCH WHEN FALSE (Short Form)  [TargetInstruction: 40]
(39) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 18]
(40) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 41]
(41) NO-OP
(42) LOAD STRING  [String Value: Last Label]
(43) CALL METHOD  [Console.WriteLine]
(44) NO-OP
(45) RETURN");
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result6()
		{
			_dynamicJumpTableMethod.Should().NotBeNull();
			var instructions = new MethodBodyParser(_dynamicJumpTableMethod).ParseInstructions();
			instructions.Count.Should().Be(15);

			var instructionsDescription = new DefaultInstructionFormatter().DescribeInstructions(instructions);
			instructionsDescription.Should().Be(
@"(0) LOAD ARGUMENT (Index 0)  [Parameter #0]  [ParameterReference: System.Int32 ]
(1) SWITCH  [TargetInstructions: 3, 5, 7, 9, 11]  [TargetOffsets: 28, 35, 42, 49, 56]
(2) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 13]
(3) LOAD STRING  [String Value: are no bananas]
(4) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(5) LOAD STRING  [String Value: is one banana]
(6) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(7) LOAD STRING  [String Value: are two bananas]
(8) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(9) LOAD STRING  [String Value: are three bananas]
(10) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(11) LOAD STRING  [String Value: are four bananas]
(12) BRANCH UNCONDITIONALLY (Short Form)  [TargetInstruction: 14]
(13) LOAD STRING  [String Value: are many bananas]
(14) RETURN");
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result7()
		{
			var methodToParse = typeof(NativeInteropExampleMethods).GetMethod(nameof(NativeInteropExampleMethods.MethodThatUsesNativeInteropCall), BindingFlags.Public | BindingFlags.Static);
			methodToParse.Should().NotBeNull();

			var instructions = new MethodBodyParser(methodToParse!).ParseInstructions();
			instructions.Count.Should().Be(23);
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result8()
		{
			var methodToParse = typeof(ExampleMethods).GetMethod(nameof(ExampleMethods.AddTwoValues_Of_7_And_14_Using_Delegate_Function), BindingFlags.Public | BindingFlags.Static);
			methodToParse.Should().NotBeNull();

			var instructions = new MethodBodyParser(methodToParse!).ParseInstructions();
			instructions.Count.Should().Be(13);
		}

		[TestMethod]
		public void ParseInstructions_returns_the_expected_result9()
		{
			var dynamicType = DynamicTypeBuilder.BuildTypeWithInlineSignatureMethod();
			var dynamicMethod = dynamicType!.GetMethod("InlineSignatureMethod", BindingFlags.Public | BindingFlags.Static);

			var instructions = new MethodBodyParser(dynamicMethod!).ParseInstructions();
			instructions.Count.Should().Be(2);
			instructions.Any(instruction => instruction is SignatureInstruction).Should().BeTrue();
		}

		/******     TEST SETUP     *****************************
		 *******************************************************/
		private static Type _dynamicTypeWithJumpTableMethod = null!; // Nullability hacks, InitializeTestClass will always set these
		private static MethodInfo _dynamicJumpTableMethod = null!;

		[ClassInitialize]
		public static void InitializeTestClass(TestContext testContext)
		{
			_dynamicTypeWithJumpTableMethod = DynamicTypeBuilder.BuildTypeWithJumpTableMethod()!;
			_dynamicJumpTableMethod = _dynamicTypeWithJumpTableMethod!.GetMethod("JumpTableMethod", BindingFlags.Public | BindingFlags.Static)!;
		}
	}
}
