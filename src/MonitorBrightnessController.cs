using System;
using System.Runtime.InteropServices;

namespace Qtl.BrightnessController;

public static unsafe partial class MonitorBrightnessController
{
	public static float GetPrimaryMonitorBrightness()
	{
		var primaryMonitorHandle = GetPrimaryMonitorHandle();
		var physicalPrimaryMonitorHandle = GetPhysicalMonitorHandle(primaryMonitorHandle);

		try
		{
			return GetMonitorBrightness(physicalPrimaryMonitorHandle);
		}
		finally
		{
			DestroyPhysicalMonitorHandle(physicalPrimaryMonitorHandle);
		}
	}

	public static void SetPrimaryMonitorBrightness(float value)
	{
		var primaryMonitorHandle = GetPrimaryMonitorHandle();
		var physicalPrimaryMonitorHandle = GetPhysicalMonitorHandle(primaryMonitorHandle);

		try
		{
			SetMonitorBrightness(physicalPrimaryMonitorHandle, value);
		}
		finally
		{
			DestroyPhysicalMonitorHandle(physicalPrimaryMonitorHandle);
		}
	}

	private static IntPtr GetPrimaryMonitorHandle()
	{
		const int MONITOR_DEFAULTTOPRIMARY = 1;
		var point = default(POINT);
		return MonitorFromPoint(ref point, MONITOR_DEFAULTTOPRIMARY);
	}

	private static float GetMonitorBrightness(IntPtr physicalMonitorHandle)
	{
		if (!GetMonitorBrightness(physicalMonitorHandle, out var min, out var current, out var max))
		{
			ThrowForLastError();
		}

		return (current - min) / (float)(max - min);
	}

	private static void SetMonitorBrightness(IntPtr physicalMonitorHandle, float value)
	{
		if (!GetMonitorBrightness(physicalMonitorHandle, out var min, out var current, out var max))
		{
			ThrowForLastError();
		}

		var newValue = min + (uint)MathF.Floor((max - min) * value);

		if (!SetMonitorBrightness(physicalMonitorHandle, newValue))
		{
			ThrowForLastError();
		}
	}

	private static IntPtr GetPhysicalMonitorHandle(IntPtr monitorHandle)
	{
		var physicalMonitors = stackalloc PHYSICAL_MONITOR[PHYSICAL_MONITOR_COUNT];
		if (!GetPhysicalMonitorsFromHMONITOR(monitorHandle, PHYSICAL_MONITOR_COUNT, physicalMonitors))
		{
			throw new NotImplementedException("Only supports one physical monitor per monitor handle.");
		}

		return physicalMonitors[0].hPhysicalMonitor;
	}

	private static void DestroyPhysicalMonitorHandle(IntPtr physicalMonitorHandle)
	{
		var physicalMonitors = stackalloc PHYSICAL_MONITOR[PHYSICAL_MONITOR_COUNT];
		physicalMonitors[0].hPhysicalMonitor = physicalMonitorHandle;
		_ = DestroyPhysicalMonitors(PHYSICAL_MONITOR_COUNT, physicalMonitors);
	}

	private static void ThrowForLastError() => throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error())!;

	private const int PHYSICAL_MONITOR_COUNT = 1;

	[LibraryImport("User32.dll", SetLastError = true)]
	private static partial IntPtr MonitorFromPoint(ref POINT point, int flags);

	[StructLayout(LayoutKind.Sequential)]
	private struct POINT
	{
		public int X;
		public int Y;
	}

	[LibraryImport("Dxva2.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetNumberOfPhysicalMonitorsFromHMONITOR(
		IntPtr hMonitor,
		out uint pdwNumberOfPhysicalMonitors);

	[LibraryImport("Dxva2.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetPhysicalMonitorsFromHMONITOR(
		IntPtr hMonitor,
		uint dwPhysicalMonitorArraySize,
		PHYSICAL_MONITOR* pPhysicalMonitorArray);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct PHYSICAL_MONITOR
	{
		public IntPtr hPhysicalMonitor;
		public fixed char szPhysicalMonitorDescription[128];
	}

	[LibraryImport("Dxva2.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool GetMonitorBrightness(
		IntPtr hMonitor,
		out uint pdwMinimumBrightness,
		out uint pdwCurrentBrightness,
		out uint pdwMaximumBrightness);


	[LibraryImport("Dxva2.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool SetMonitorBrightness(
		IntPtr hMonitor,
		uint brightness);

	[LibraryImport("Dxva2.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static partial bool DestroyPhysicalMonitors(
		uint dwPhysicalMonitorArraySize,
		PHYSICAL_MONITOR* pPhysicalMonitorArray);
}
