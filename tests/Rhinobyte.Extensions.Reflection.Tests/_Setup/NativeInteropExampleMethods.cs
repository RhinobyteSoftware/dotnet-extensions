using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;

namespace Rhinobyte.Extensions.Reflection.Tests.Setup;

#if NET8_0_OR_GREATER
internal static partial class NativeInteropExampleMethods
#else
internal static class NativeInteropExampleMethods
#endif
{
	/// <summary>
	/// Process access flags used in conjunction with the StandardAccessFlags for process access.
	/// <seealso href="https://docs.microsoft.com/en-us/windows/win32/procthread/process-security-and-access-rights" />
	/// </summary>
	[Flags]
	internal enum ProcessAccessFlags : uint
	{
		None = 0x00000000,

		All = 0x001F0FFF,

		/// <summary>
		/// Required to create a process.
		/// </summary>
		CreateProcess = 0x00000080,

		/// <summary>
		/// Required to create a thread.
		/// </summary>
		CreateThread = 0x00000002,

		/// <summary>
		/// Required to duplicate a handle using DuplicateHandle.
		/// </summary>
		DuplicateHandle = 0x00000040,

		/// <summary>
		/// Required to retrieve certain information about a process, such as its token, exit code, and priority class
		/// (see OpenProcessToken).
		/// </summary>
		QueryInformation = 0x00000400,

		/// <summary>
		/// Required to retrieve certain information about a process (see GetExitCodeProcess, GetPriorityClass,
		/// IsProcessInJob, QueryFullProcessImageName).
		/// 
		/// A handle that has the <see cref="ProcessAccessFlags.QueryInformation" /> access right
		/// is automatically granted <see cref="ProcessAccessFlags.QueryLimitedInformation" />.
		/// </summary>
		/// <remarks>Windows Server 2003 and Windows XP:  This access right is not supported.</remarks>
		QueryLimitedInformation = 0x00001000,

		/// <summary>
		/// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
		/// </summary>
		SetInformation = 0x00000200,

		/// <summary>
		/// Required to set memory limits using SetProcessWorkingSetSize.
		/// </summary>
		SetQuota = 0x00000100,

		/// <summary>
		/// Required to suspend or resume a process.
		/// </summary>
		SuspendResume = 0x00000800,

		/// <summary>
		/// Required to wait for the process to terminate using the wait functions.
		/// </summary>
		Synchronize = 0x00100000,

		/// <summary>
		/// Required to terminate a process using TerminateProcess.
		/// </summary>
		Terminate = 0x00000001,

		/// <summary>
		/// Required to perform an operation on the address space of a process (see VirtualProtectEx and
		/// WriteProcessMemory).
		/// </summary>
		VirtualMemoryOperation = 0x00000008,

		/// <summary>
		/// Required to read memory in a process using ReadProcessMemory.
		/// </summary>
		VirtualMemoryRead = 0x00000010,

		/// <summary>
		/// Required to write to memory in a process using WriteProcessMemory.
		/// </summary>
		VirtualMemoryWrite = 0x00000020
	}

	internal const ProcessAccessFlags READ_PROCESS_MEMORY_FLAGS =
		ProcessAccessFlags.QueryInformation
		| ProcessAccessFlags.VirtualMemoryOperation
		| ProcessAccessFlags.VirtualMemoryRead;


#pragma warning disable SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time - Reason: Want to test the method body IL parser against both a [DllImport] and a newer source generated [LibraryImport] method when targetting .NET 8
	/// <summary>
	/// Platform invoke call into kernel32.dll IsWow64Process.
	/// </summary>
	/// <param name="processHandle">The handle of the process to check for wow 64 emulation.</param>
	/// <param name="isWow64Process">Result will be true if the process is running in the Wow64 emulator (e.g. 32bit process) or false if it is not running in the emulator (e.g. 64 bit process)</param>
	/// <returns>true if the call succeeded, false if the call failed</returns>
	[DllImport("kernel32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	[return: MarshalAs(UnmanagedType.Bool)]
	internal static extern bool IsWow64Process([In] SafeProcessHandle processHandle, [Out, MarshalAs(UnmanagedType.Bool)] out bool isWow64Process);
#pragma warning restore SYSLIB1054 // Use 'LibraryImportAttribute' instead of 'DllImportAttribute' to generate P/Invoke marshalling code at compile time

#if NET8_0_OR_GREATER
	[LibraryImport("kernel32.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static partial SafeProcessHandle OpenProcess(ProcessAccessFlags desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);
#else
	[DllImport("kernel32.dll", SetLastError = true)]
	[DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
	public static extern SafeProcessHandle OpenProcess([In] ProcessAccessFlags desiredAccess, [In] bool inheritHandle, [In] int processId);
#endif


	public static bool MethodThatUsesNativeInteropCall(int processId)
	{
		using var processHandle = OpenProcess(ProcessAccessFlags.QueryInformation, false, processId);
		var didSucceed = IsWow64Process(processHandle, out var isWow64EmulatedProcess);

		return didSucceed && isWow64EmulatedProcess;
	}
}
