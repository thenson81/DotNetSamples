﻿using Microsoft.Diagnostics.Runtime.Interop;
using System;
using System.Runtime.InteropServices;
using WindowsPE;
using KernelStructOffset;

namespace NetDbgExt
{
    // DbgShell/ClrMemDiag/Debugger/
    // https://github.com/microsoft/DbgShell/tree/master/ClrMemDiag/Debugger

    // .load C:\...\DotNetSamples\WinConsole\Debugger\NetDbgExt\bin\Debug\x64\NetDbgExt.dll
    // .unload C:\...\DotNetSamples\WinConsole\Debugger\NetDbgExt\bin\Debug\x64\NetDbgExt.dll
    public class UnmanagedMain
    {
        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static uint DebugExtensionInitialize(IntPtr version, IntPtr flags)
        {
            version.WriteValue(DEBUG_EXTENSION_VERSION(1, 0));
            flags.WriteValue(0);

            NativeMethods.OutputDebugString("NetDbgExt.DebugExtensionInitialize");
            return 0;
        }

        public static uint DEBUG_EXTENSION_VERSION(int Major, int Minor)
        {
            return (uint)((((Major) & 0xffff) << 16) | ((Minor) & 0xffff));
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static void DebugExtensionNotify(DebugNotifySession notify, long argument)
        {
            switch (notify)
            {
                case DebugNotifySession.Active:
                    NativeMethods.OutputDebugString($"DEBUG_NOTIFY_SESSION_ACTIVE: {argument}");
                    break;
                case DebugNotifySession.Inactive:
                    NativeMethods.OutputDebugString($"DEBUG_NOTIFY_SESSION_INACTIVE: {argument}");
                    break;
                case DebugNotifySession.Accessible:
                    NativeMethods.OutputDebugString($"DEBUG_NOTIFY_SESSION_ACCESSIBLE: {argument}");
                    break;
                case DebugNotifySession.InAccessible:
                    NativeMethods.OutputDebugString($"DEBUG_NOTIFY_SESSION_INACCESSIBLE: {argument}");
                    break;
            }
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
        public static void DebugExtensionUninitialize()
        {
            NativeMethods.OutputDebugString("NetDbgExt.DebugExtensionUninitialize");
        }

        [DllExport(CallingConvention = CallingConvention.StdCall)]
#pragma warning disable IDE1006 // Naming Styles
        public static uint printdt(IDebugClient pDebugClient, [MarshalAs(UnmanagedType.LPStr)] string args)
#pragma warning restore IDE1006 // Naming Styles
        {
            if (!(pDebugClient is IDebugControl dbgControl))
            {
                return 0;
            }

            int result = dbgControl.Evaluate(args, DEBUG_VALUE_TYPE.INT64, out DEBUG_VALUE dbgValue, out _);
            if (result != 0)
            {
                return 0;
            }

            ulong u64Value = dbgValue.I64;
            DateTime dt = Util.ToDateTime((long)u64Value);

            dbgControl.Output(DEBUG_OUTPUT.NORMAL, $"0x{u64Value:x}, 0n{u64Value} ==> {dt}\n");

            return 0;
        }
    }
}
