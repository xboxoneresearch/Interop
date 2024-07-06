//
// Bless ol' xcrdutil
//
public static class XCrd
{
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdOpenAdapter(out IntPtr hAdapter);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdCloseAdapter(IntPtr hAdapter);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdMount(out IntPtr hDevice, IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath, uint mountFlags);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdUnmount(IntPtr hAdapter, IntPtr hDevice);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdUnmountByPath(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdQueryDevicePath([MarshalAs(UnmanagedType.LPWStr)] out string devicePath, IntPtr hDeviceHandle);

}

public class XCrdManager : IDisposable
{
    const int CHUNK_SIZE = 16 * 1024 * 1024; // 16MB
    readonly IntPtr _adapterHandle;

    public XCrdManager()
    {
        uint result = XCrd.XCrdOpenAdapter(out IntPtr hAdapter);
        if (hAdapter == IntPtr.Zero)
        {
            throw new Exception($"Failed to open XCRD adapter, code: {result:08x}");
        }
        _adapterHandle = hAdapter;
    }

    public string Mount(string xvdPath)
    {
        uint result = XCrd.XCrdMount(out IntPtr hDevice, _adapterHandle, xvdPath, 0);
        if (result != 0 || hDevice == IntPtr.Zero)
        {
            Console.WriteLine("Failed to mount target! Result: " + result);
            return null;
        }

        result = XCrd.XCrdQueryDevicePath(out string devicePath, hDevice);
        if (result != 0)
        {
            Console.WriteLine("Failed to read device path! Result: " + result);
            return null;
        }

        return devicePath;
    }

    /// <summary>
    /// Unmount an XVD based on XCRD path (host-relevant path)
    /// </summary>
    /// <param name="xcrdPath"></param>
    /// <returns>0 on success</returns>
    public uint Unmount(string xcrdPath)
    {
        uint result = XCrd.XCrdUnmountByPath(_adapterHandle, xcrdPath);
        if (result != 0)
        {
            Console.WriteLine("[x] Failed to unmount target xvd!");
            Console.WriteLine("[x] Result: " + result);
            return result;
        }
        Console.WriteLine("Unmounted successfully");
        return 0;
    }

    public void Dispose()
    {
        if (_adapterHandle != IntPtr.Zero)
        {
            XCrd.XCrdCloseAdapter(_adapterHandle);
        }
    }
}
