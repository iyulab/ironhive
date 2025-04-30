using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace WebServer;

public static class ProcessLauncher
{
    public static Process Start(string fileName, string arguments = null)
    {
        var siEx = new STARTUPINFOEX();
        siEx.StartupInfo.cb = Marshal.SizeOf<STARTUPINFOEX>();

        IntPtr lpAttributeList = IntPtr.Zero;
        IntPtr parentHandle = Process.GetCurrentProcess().Handle;

        try
        {
            // 1. Attribute List 크기 알아내기
            int attributeSize = 0;
            InitializeProcThreadAttributeList(IntPtr.Zero, 1, 0, ref attributeSize);

            // 2. Attribute List 할당 및 초기화
            lpAttributeList = Marshal.AllocHGlobal(attributeSize);
            if (!InitializeProcThreadAttributeList(lpAttributeList, 1, 0, ref attributeSize))
                throw new Win32Exception(Marshal.GetLastWin32Error());

            // 3. 부모 프로세스 설정
            if (!UpdateProcThreadAttribute(
                lpAttributeList,
                0,
                (IntPtr)PROC_THREAD_ATTRIBUTE_PARENT_PROCESS,
                parentHandle,
                (IntPtr)IntPtr.Size,
                IntPtr.Zero,
                IntPtr.Zero))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            siEx.lpAttributeList = lpAttributeList;

            var pi = new PROCESS_INFORMATION();

            string commandLine = string.IsNullOrEmpty(arguments) ? fileName : $"{fileName} {arguments}";

            bool success = CreateProcess(
                null,
                commandLine,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                EXTENDED_STARTUPINFO_PRESENT,
                IntPtr.Zero,
                null,
                ref siEx.StartupInfo,
                out pi);

            if (!success)
                throw new Win32Exception(Marshal.GetLastWin32Error());

            return Process.GetProcessById(pi.dwProcessId);
        }
        finally
        {
            if (lpAttributeList != IntPtr.Zero)
            {
                DeleteProcThreadAttributeList(lpAttributeList);
                Marshal.FreeHGlobal(lpAttributeList);
            }
        }
    }

    #region Win32 API

    private const int PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
    private const int EXTENDED_STARTUPINFO_PRESENT = 0x00080000;

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct STARTUPINFOEX
    {
        public STARTUPINFO StartupInfo;
        public IntPtr lpAttributeList;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public int dwProcessId;
        public int dwThreadId;
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool InitializeProcThreadAttributeList(
        IntPtr lpAttributeList,
        int dwAttributeCount,
        int dwFlags,
        ref int lpSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool UpdateProcThreadAttribute(
        IntPtr lpAttributeList,
        uint dwFlags,
        IntPtr Attribute,
        IntPtr lpValue,
        IntPtr cbSize,
        IntPtr lpPreviousValue,
        IntPtr lpReturnSize);

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    private static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        int dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    #endregion
}
