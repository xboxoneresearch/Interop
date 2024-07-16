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
    public static extern uint XCrdDeleteXVD(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath, uint flags);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdUnmountByPath(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdQueryDevicePath([MarshalAs(UnmanagedType.LPWStr)] out string devicePath, IntPtr hDeviceHandle);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStreamingStart(
        out IntPtr hStreamAdapter,
        IntPtr hAdapter,
        [MarshalAs(UnmanagedType.LPWStr)] string xcrdSourcePath,
        [MarshalAs(UnmanagedType.LPWStr)] string unkPath,
        [MarshalAs(UnmanagedType.LPWStr)] string xcrdDestPath,
        [MarshalAs(UnmanagedType.LPWStr)] string unkPath2,
        [MarshalAs(UnmanagedType.LPWStr)] string unkPath3,
        uint SpecifierCount,
        IntPtr Specifiers,
        uint Flags,
        out ulong DstSize,
        out IntPtr ErrorSource
    );
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStreamingStop(IntPtr hAdapter, IntPtr hInstance, uint flags);
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStreamingQueryActiveInstanceId(IntPtr hAdapter, out IntPtr hInstanceId);
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    // QueryInformation untested
    public static extern uint XCrdStreamingQueryInformation(IntPtr info, IntPtr hAdapter, IntPtr hInstance);


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct XvdStreamingInfo
    {
        public ulong StreamId;                    // 0x00

        ulong Unknown08;                          // 0x08 - UNKNOWN

        public ulong StreamedBytes;               // 0x10
        public ulong SourceStreamedBytes;         // 0x18
        public ulong CacheStreamedBytes;          // 0x20
        public ulong UpdateStreamedBytes;         // 0x28
        public ulong FallbackStreamedBytes;       // 0x30

        ulong Unknown38;                          // 0x38 - UNKNOWN

        public ulong DownloadSize;                // 0x40
        public ulong StagingSize;                 // 0x48
        public ulong BlobSize;                    // 0x50
        public ulong ActiveRegion;                // 0x58
        public ulong ActiveRegionStreamedBytes;   // 0x60
        public ulong RegionCount;                 // 0x68
        public ulong StreamedRegionCount;         // 0x70
        public uint InitialPlayRegionId;          // 0x78
        public uint CacheLastIoStatus;            // 0x7C
        public ulong InitialPlayRegionOffset;     // 0x80
        public ulong PaddingBytes;                // 0x88
        public uint SourceLastIoStatus;           // 0x90
        public uint DestinationLastIoStatus;      // 0x94
        public uint SourceStreaming;              // 0x98
        public uint RegionSpecifierCount;         // 0x9C
        public uint StreamingDisc;                // 0xA0
        public uint NeededDisc;                   // 0xA4
        public ulong SegmentCount;                // 0xA8

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x20)]
        byte[] UnknownB0;                         // 0xB0 - MISSING

        public ulong XipMissBytes;                // 0xC0
        public uint XipMissCount;                 // 0xC8
        public uint XipReadAhead;                 // 0xCC
        public Guid StreamingPlanId;              // 0xD0
        public ulong FsDefragmentedBytes;         // 0xE0
        public ulong FsDefragmentedRequiredBytes; // 0xE8
        public ulong NandDefragmentedBytes;       // 0xF0
        public ulong NandDefragmentRequiredBytes; // 0xF8

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] Reserved;                   // 0x100
        /* END 0x110 */
    }
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

    public uint DeleteXVD(string xvdPath)
    {
        uint result = XCrd.XCrdDeleteXVD(_adapterHandle, xvdPath, 0);
        if (result != 0)
        {
            Console.WriteLine("[x] Failed to delete xvd! Result:" + result.ToString());
        }
        return result;
    }

    string StreamingInfoToString(XCrd.XvdStreamingInfo info)
    {
        StringBuilder sb = new();
        sb.AppendFormat("Active Region                 = 0x{0:X16}\n", info.ActiveRegion);
        sb.AppendFormat("Active Region Streamed Bytes  = 0x{0:X16}\n", info.ActiveRegionStreamedBytes);
        sb.AppendFormat("Blob Size                     = 0x{0:X16}\n", info.BlobSize);
        sb.AppendFormat("Staging Size                  = 0x{0:X16}\n", info.StagingSize);
        sb.AppendFormat("Initial Play Region Id        = 0x{0:X}\n", info.InitialPlayRegionId);
        sb.AppendFormat("Initial Play Region Offset    = 0x{0:X16}\n", info.InitialPlayRegionOffset);
        sb.AppendFormat("Region Count                  = 0x{0:X16}\n", info.RegionCount);
        sb.AppendFormat("Segment Count                 = 0x{0:X16}\n", info.SegmentCount);
        sb.AppendFormat("Region Specifier Count        = 0x{0:X}\n", info.RegionSpecifierCount);
        sb.AppendFormat("Streamed Region Count         = 0x{0:X16}\n", info.StreamedRegionCount);
        sb.AppendFormat("Streamed Bytes                = 0x{0:X16}\n", info.StreamedBytes);
        sb.AppendFormat("Source Streamed Bytes         = 0x{0:X16}\n", info.SourceStreamedBytes);
        sb.AppendFormat("Cache Streamed Bytes          = 0x{0:X16}\n", info.CacheStreamedBytes);
        sb.AppendFormat("Update Streamed Bytes         = 0x{0:X16}\n", info.UpdateStreamedBytes);
        sb.AppendFormat("Fallback Streamed Bytes       = 0x{0:X16}\n", info.FallbackStreamedBytes);
        sb.AppendFormat("Padding Bytes                 = 0x{0:X16}\n", info.PaddingBytes);
        sb.AppendFormat("Stream ID                     = 0x{0:X16}\n", info.StreamId);
        sb.AppendFormat("Download Size                 = 0x{0:X16}\n", info.DownloadSize);
        sb.AppendFormat("Source Last Io Status         = 0x{0:X}\n", info.SourceLastIoStatus);
        sb.AppendFormat("Cache Last Io Status          = 0x{0:X}\n", info.CacheLastIoStatus);
        sb.AppendFormat("Destination Last Io Status    = 0x{0:X}\n", info.DestinationLastIoStatus);
        sb.AppendFormat("Source Streaming              = {0:X}\n", info.SourceStreaming);
        // sb.AppendFormat("Cache Streaming               = {0:X}");
        // sb.AppendFormat("Update Streaming              = {0:X}");
        // sb.AppendFormat("FS Defragmenting              = {0:X}");
        // sb.AppendFormat("NAND Defragmenting            = {0:X}");
        sb.AppendFormat("Streaming Disc                = {0:X}\n", info.StreamingDisc);
        sb.AppendFormat("Needed Disc                   = {0:X}\n", info.NeededDisc);
        sb.AppendFormat("XIP Miss Bytes                = {0:X}\n", info.XipMissBytes);
        sb.AppendFormat("XIP Miss Count                = {0:X}\n", info.XipMissCount);
        sb.AppendFormat("XIP Read Ahead (s)            = {0:X}\n", info.XipReadAhead);
        // sb.AppendFormat("XIP Plan Type                 = {0}");
        sb.AppendFormat("Streaming Plan Id             = {0}\n", info.StreamingPlanId);
        sb.AppendFormat("FS Defragmented Bytes         = {0:X}\n", info.FsDefragmentedBytes);
        sb.AppendFormat("FS Defragment Required Bytes  = {0:X}\n", info.FsDefragmentedRequiredBytes);
        sb.AppendFormat("NAND Defragmented Bytes       = {0:X}\n", info.NandDefragmentedBytes);
        sb.AppendFormat("NAND Defragment Required Bytes= {0:X}\n", info.NandDefragmentRequiredBytes);

        return sb.ToString();
    }

    public uint Stream(string srcPath, string dstPath)
    {
        uint result = XCrd.XCrdStreamingStart(
            out IntPtr hInstanceId,
            _adapterHandle,
            srcPath,
            null,
            dstPath,
            null,
            null,
            0,
            IntPtr.Zero,
            0,
            out ulong dstSize,
            out IntPtr errorSource
        );

        if (result != 0)
        {
            Console.WriteLine("Starting streaming failed: " + result.ToString());
            return result;
        }
        Console.WriteLine("Streaming handle: 0x{0:X}", hInstanceId);

        IntPtr ptrInfoBuf = Marshal.AllocHGlobal(Marshal.SizeOf<XCrd.XvdStreamingInfo>());
        if (ptrInfoBuf == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate XvdStreamingInfo buffer");
            return 1;
        }

        while (true)
        {
            result = XCrd.XCrdStreamingQueryInformation(ptrInfoBuf, _adapterHandle, hInstanceId);
            if (result != 0)
            {
                Console.WriteLine("Failed to query streaming infos: " + result.ToString());
                Marshal.FreeHGlobal(ptrInfoBuf);
                return result;
            }

            var streamingInfo = Marshal.PtrToStructure<XCrd.XvdStreamingInfo>(ptrInfoBuf);
            Console.WriteLine(":: Streaming Info ::\n{0}", StreamingInfoToString(streamingInfo));

            if (streamingInfo.BlobSize == streamingInfo.StreamedBytes)
            {
                Console.WriteLine("Streaming of {0:X16} bytes finished!", streamingInfo.StreamedBytes);
                break;
            }

            // Wait 5 seconds between queries
            Thread.Sleep(1000 * 5);
        }

        Marshal.FreeHGlobal(ptrInfoBuf);

        Console.WriteLine("Stopping streaming");
        result = XCrd.XCrdStreamingStop(_adapterHandle, hInstanceId, 0);
        if (result != 0)
        {
            Console.WriteLine("Failed to stop streaming: " + result.ToString());
        }

        return result;
    }

    public void Dispose()
    {
        if (_adapterHandle != IntPtr.Zero)
        {
            XCrd.XCrdCloseAdapter(_adapterHandle);
        }
    }
}
