public static class COM
{
    [DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
    public static extern int CoInitializeEx([In, Optional] IntPtr pvReserved, [In] uint dwCoInit);

    [DllImport("combase.dll")]
    public static extern uint CoCreateInstance(ref Guid clsid, [MarshalAs(UnmanagedType.IUnknown)] object inner, uint context, ref Guid uuid, [MarshalAs(UnmanagedType.IUnknown)] out object rReturnedComObject);


    public static T ActivateClass<T>(Guid clsid)
    {
        var iid = typeof(T).GUID;
        var t = COM.CoCreateInstance(ref clsid, null, 4, ref iid, out object factory);
        if (t != 0)
        {
            throw new Exception($"Failed to activate class {typeof(T)}");
        }

        return (T)factory;
    }
}

public static class WinRT
{
    public enum RO_INIT_TYPE
    {
        RO_INIT_SINGLETHREADED = 0,
        RO_INIT_MULTITHREADED = 1,
    }

    [DllImport("api-ms-win-core-winrt-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr RoInitialize(RO_INIT_TYPE initType);

    [DllImport("api-ms-win-core-winrt-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr RoUninitialize();

    [DllImport("api-ms-win-core-winrt-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr RoGetActivationFactory(IntPtr activatableClassId, byte[] iid, out IntPtr factory);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString, int length, out IntPtr hstring);

    [DllImport("api-ms-win-core-winrt-string-l1-1-0.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern IntPtr WindowsDeleteString(IntPtr hstring);

    // A helper to read the virtual function pointer from virtual table of the instance.
    public static T GetVirtualMethodPointer<T>(IntPtr instance, int index)
    {
        var table = Marshal.ReadIntPtr(instance);
        var pointer = Marshal.ReadIntPtr(table, index * IntPtr.Size);
        return Marshal.GetDelegateForFunctionPointer<T>(pointer);
    }
}

public static class Win32
{
    const string KERNELBASE_DLL = "kernelbase.dll";
    const string KERNEL32_DLL = "kernel32.dll";

    [Flags]
    public enum FileAccess : uint
    {
        GENERIC_READ = 0x80000000,
        GENERIC_WRITE = 0x40000000,
        GENERIC_EXECUTE = 0x20000000,
        GENERIC_ALL = 0x10000000
    }

    [Flags]
    public enum FileShare : uint
    {
        FILE_SHARE_READ = 0x00000001,
        FILE_SHARE_WRITE = 0x00000002,
        FILE_SHARE_DELETE = 0x00000004,
    }

    [Flags]
    public enum FileMode : uint
    {
        CREATE_NEW = 1,
        CREATE_ALWAYS = 2,
        OPEN_EXISTING = 3,
        OPEN_ALWAYS = 4,
        TRUNCATE_EXISTING = 5
    }

    [Flags]
    public enum FileAttributes : uint
    {
        NORMAL = 0x00000080,
        HIDDEN = 0x00000002,
        READONLY = 0x00000001,
        ARCHIVE = 0x00000020,
        SYSTEM = 0x00000004
    }


    [DllImport(KERNEL32_DLL, CharSet = CharSet.Ansi, SetLastError = true)]
    public static extern IntPtr CreateFileA(
            [MarshalAs(UnmanagedType.LPStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

    [DllImport(KERNEL32_DLL, SetLastError = true)]
    public static extern bool ReadFile(IntPtr hFile, [Out] byte[] lpBuffer, uint nNumberOfBytesToRead, out uint lpNumberOfBytesRead, IntPtr lpOverlapped);

    public static void RawRead(string path)
    {
        IntPtr hFile = CreateFileA(path, Win32.FileAccess.GENERIC_READ, Win32.FileShare.FILE_SHARE_READ | Win32.FileShare.FILE_SHARE_WRITE, 0, Win32.FileMode.OPEN_EXISTING, 0, 0);
        if (hFile != IntPtr.Zero)
        {
            if (hFile == -1)
            {
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine("GetLastError: " + error);
            }
            Console.WriteLine("[+] Opened handle to: " + path);
            Console.WriteLine("[+] Opened handle to: " + hFile.ToString());

            uint numBytesRead = 0;
            byte[] tmpReadBuffer = new byte[256];

            bool hasRead = ReadFile(hFile, tmpReadBuffer, 256, out numBytesRead, 0);
            if (!hasRead)
            {
                Console.WriteLine("[x] Failed to read target :(");
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine("GetLastError: " + error);
                return;
            }
            Console.WriteLine("[+] Succesfully read target! Bytes read: " + numBytesRead);
        }
    }
}
