﻿using System;
using System.Runtime.InteropServices;

namespace Qtl.RawWacom;

internal readonly partial struct ProcessInformation : IDisposable
{
	private readonly IntPtr _processHandle;

	public ProcessInformation()
	{
		var processId = GetCurrentProcessId();
		_processHandle = OpenProcess(PROCESS_SET_INFORMATION, false, processId);
	}

	public bool SetEfficiencyMode()
	{
		var state = new PROCESS_POWER_THROTTLING_STATE
		{
			ControlMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
			StateMask = PROCESS_POWER_THROTTLING_EXECUTION_SPEED,
			Version = PROCESS_POWER_THROTTLING_CURRENT_VERSION,
		};

		return
			SetPriorityClass(_processHandle, IDLE_PRIORITY_CLASS) &&
			SetProcessThrottlingState(_processHandle, ref state);
	}

	public void Dispose()
	{
		if (_processHandle != IntPtr.Zero)
		{
			_ = CloseHandle(_processHandle);
		}
	}

	private const string LIBRARY = "Kernel32.dll";

	private const int PROCESS_SET_INFORMATION = 0x0200;

	private const int IDLE_PRIORITY_CLASS = 0x00000040;

	private const int PROCESS_POWER_THROTTLING = 4;

	private const int PROCESS_POWER_THROTTLING_CURRENT_VERSION = 1;

	private const int PROCESS_POWER_THROTTLING_EXECUTION_SPEED = 1;

	[LibraryImport(LIBRARY)]
	private static partial int GetCurrentProcessId();

	[LibraryImport(LIBRARY)]
	private static partial IntPtr OpenProcess(int desiredAccess, [MarshalAs(UnmanagedType.Bool)] bool inheritHandle, int processId);

	[LibraryImport(LIBRARY)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetPriorityClass(IntPtr processHandle, int priorityClass);

	[LibraryImport(LIBRARY)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetProcessInformation(IntPtr processHandle, int processInformationClass, ref PROCESS_POWER_THROTTLING_STATE powerThrottlingState, int size);

	private static unsafe bool SetProcessThrottlingState(IntPtr processHandle, ref PROCESS_POWER_THROTTLING_STATE powerThrottlingState) =>
		SetProcessInformation(processHandle, PROCESS_POWER_THROTTLING, ref powerThrottlingState, sizeof(PROCESS_POWER_THROTTLING_STATE));

	[StructLayout(LayoutKind.Sequential)]
	private struct PROCESS_POWER_THROTTLING_STATE
	{
		public int ControlMask;
		public int StateMask;
		public int Version;
	}

	[LibraryImport(LIBRARY)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool CloseHandle(IntPtr handle);
}