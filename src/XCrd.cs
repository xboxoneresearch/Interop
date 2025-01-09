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

    // 1 page == 0x1000 (4k)
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdCreateXVD(
        IntPtr hAdapter,
        [MarshalAs(UnmanagedType.LPWStr)] string crdPath,
        Guid someId,
        XvdCreateFlags createFlags,
        ulong drivePages,
        XvdType xvdType,
        XvdContentType xvdContentType,
        out Guid outputGuid,
        Guid productId,
        string sandboxId,
        uint sandboxLen,
        [MarshalAs(UnmanagedType.LPWStr)] string templateXvd
    );

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdDeleteXVD(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath, uint flags);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdUnmountByPath(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string crdPath);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdQueryCrdInformation(IntPtr hAdapter, int xcrdId, int xcrdQueryClass, out IntPtr info, ulong length);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdQueryXvdInformation(IntPtr hAdapter, string crdPath, ulong infoClass, out IntPtr infoBuffer, ulong length);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdQueryDevicePath([MarshalAs(UnmanagedType.LPWStr)] out string devicePath, IntPtr hDeviceHandle);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdReadUserDataXVD(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string srcPath, ulong offset, IntPtr buffer, ref uint length);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStorageReadBlob(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string srcPath, IntPtr buf, ref uint readSize);
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStorageWriteBlob(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string dstPath, byte[] buf, uint writeSize);
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStorageMoveBlob(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string src, [MarshalAs(UnmanagedType.LPWStr)] string dst, uint flags);
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdStorageDeleteBlob(IntPtr hAdapter, [MarshalAs(UnmanagedType.LPWStr)] string path);

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdXBlobCreate(
        IntPtr hAdapter,
        [MarshalAs(UnmanagedType.LPWStr)] string path,
        ulong size,
        uint flags
    );
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdXBlobCopy(
        IntPtr hAdapter,
        [MarshalAs(UnmanagedType.LPWStr)] string srcPath,
        [MarshalAs(UnmanagedType.LPWStr)] string dstPath,
        ulong offset,
        ulong length,
        uint flags,
        ref ulong bytesRead
    );
    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdXBlobDelete(
        IntPtr hAdapter,
        [MarshalAs(UnmanagedType.LPWStr)] string path
    );

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdFindFirstBlob(
            out IntPtr hEnum,
            ref IntPtr data,
            uint xcrdId,
            [MarshalAs(UnmanagedType.LPWStr)] string path,
            uint flags
    );

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdFindNextBlob(
        IntPtr hEnum, ref IntPtr data
    );

    [DllImport("xcrdapi.dll", PreserveSig = false)]
    public static extern uint XCrdFindCloseBlob(IntPtr hEnum);

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

    [Flags]
    public enum XCrdAttributes : uint
    {
        DetectBufferAvailable = 1,
        Directory = 2,
        DosPathAvailable = 4
    }

    public enum XCrdQueryInformationType
    {
        XCrdQueryBasicInfo = 0,
        XCrdQueryXvcKeyIdInfo = 1,
        XCrdQueryMinSysVer = 2,
        XCrdQueryBasicInfoHeaderOnly = 3,
        XCrdQueryPlsInfo = 4,
        XCrdQueryVersionId = 5,
        XCrdQueryConsistencyChecked = 6,
        XCrdQueryXasCounters = 7,
        XCrdQueryXVCSCounters = 8,
        XCrdQuerySigningInfo = 9,
        XCrdQueryUpdateInfo = 10,
        XCrdQueryBaseXvdCounters = 11,
        XCrdQueryXvdPointerInfo = 12,
        XCrdQuerySupportedPlatforms = 17,
        XCrdSystemQueryXCloudFeatureBits = 18,
        XCrdQueryXvdLocalPointerInfo = 19
    }

    public enum XvdType
    {
        XvdTypeFixed = 0,
        XvdTypeDynamic,
        XvdTypeMax
    }

    public enum XvdContentType : uint
    {
        Data = 0,
        Title = 1,
        SystemOS = 2,
        EraOS = 3,
        Scratch = 4,
        ResetData = 5,
        Application = 6,
        HostOS = 7,
        X360STFS = 8,
        X360FATX = 9,
        X360GDFX = 0xA,
        Updater = 0xB,
        OfflineUpdater = 0xC,
        Template = 0xD,
        MteHost = 0xE,
        MteApp = 0xF,
        MteTitle = 0x10,
        MteEraOS = 0x11,
        EraTools = 0x12,
        SystemTools = 0x13,
        SystemAux = 0x14,
        AcousticModel = 0x15,
        SystemCodecsVolume = 0x16,
        QasltPackage = 0x17,
        AppDlc = 0x18,
        TitleDlc = 0x19,
        UniversalDlc = 0x1A,
        SystemDataVolume = 0x1B,
        TestVolume = 0x1C,
        HardwareTestVolume = 0x1D,
        KioskContent = 0x1E,
        HostProfiler = 0x20,
        Uwa = 0x21,
        Unknown22 = 0x22,
        Unknown23 = 0x23,
        Unknown24 = 0x24,
        ServerAgent = 0x25
    }

    public enum XvdVolumeFlags : uint
    {
        ReadOnly = 1,
        EncryptionDisabled = 2, // data decrypted, no encrypted CIKs
        DataIntegrityDisabled = 4, // unsigned and unhashed
        LegacySectorSize = 8,
        ResiliencyEnabled = 0x10,
        SraReadOnly = 0x20,
        RegionIdInXts = 0x40,
        TitleSpecific = 0x80,
        PointerXvd = 0x100,
        StreamingRoamable = 0x200,
        DiffusiveDisabled = 0x400,
        SpoofedDuid = 0x800,
        Reserved0 = 0x1000,        // old TrimSupported
        TrimSupported = 0x2000,
        RoamingEnabled = 0x4000,
        Unknown0 = 0x8000,
    }

    public enum XvdCreateFlags: ulong
    {
        EncryptionEnabled = 1,
        DataIntegrityEnabled = 2,
        // Unsure about this, could be read-only
        Unknown = 4,
        NativePaths = 0x20,
        ResiliencyEnabled = 0x40,
        TitleSpecific = 0x100,
        SraReadOnly = 0x200,
        EphemeralKey = 0x10_000,
        StreamingRoamableEnabled = 0x20_000,
        TrimEnabled = 0x100_000,
        RoamingEnabled = 0x200_000,
        EphemeralKeyShared = 0x400_000,

        // ACL
        ACL_APPCONTAINER = 0x80,
        ACL_SYSTEM = 0x1_000,
        ACL_WER = 0x1_080
    }

    public enum StreamingSource : uint
    {
        Cache,
        Update,
        FsDefragmenting,
        NandDefragmenting
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct XCrdXvdBasicInfo
    {
        public ushort FormatVersion;
        public byte SupportedPlatforms;
        public byte Unknown;
        public XvdType Type;
        public XvdContentType ContentType;
        public XvdVolumeFlags Flags;
        public ulong CreationTime;
        public ulong DriveSize;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] VDUID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] UVUID;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] PDUID;

        public ulong PackageVersion;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public char[] SandboxId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public byte[] ProductId;

        public ulong BlobSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct XCrdBuffer
    {
        public uint Length;   // 0x00
        public uint Type;     // 0x04
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct Utf16StringPtr
    {
        public uint StringPtr; // 0x00
        public uint StringLen; // 0x04
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct XCrdInfo
    {
        // Skipping \\ prefix of filename
        public Utf16StringPtr Filename;      // 0x00
        public Utf16StringPtr DosPath;       // 0x08
        public Utf16StringPtr CrdPath;       // 0x10
        public IntPtr Buffer;             // 0x18
        public ulong Size;                // 0x20
        public ulong CreationTime;        // 0x28
        public XCrdAttributes Attributes; // 0x30
        /* END 0x34 */
    }


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

    public static readonly string[] XcrdIdMapping = new string[]{
        "[XTE:]",
        "[XUC:]",
        "[XSS:]",
        "[XSU:]",
        null,
        "[XFG:]",
        null,
        "[XTF:]",
        "[XBL:]",
        "[XOD:]",
        "[XVE:]",
        "[XT0:]",
        "[XT1:]",
        "[XT2:]",
        "[XT3:]",
        "[XT4:]",
        "[XT5:]",
        "[XT6:]",
        "[XT7:]",
        "[XE0:]",
        "[XE1:]",
        "[XE2:]",
        "[XE3:]",
        "[XE4:]",
        "[XE5:]",
        "[XE6:]",
        "[XE7:]",
        "[XBX:]",
        "[XSR:]",
        null,
        "[XAS:]",
        "[XRT:]",
        "[XRU:]",
        "[XRS:]",
        "[XRI:]",
        null,
        "[XCC:]"
    };
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

    // 1 page = 0x1000 (4k)
    public uint CreateXVD(string crdPath, Guid someId, XCrd.XvdCreateFlags createFlags, ulong drivePages, XCrd.XvdType xvdType, XCrd.XvdContentType contentType, out Guid outputGuid, Guid productId, string sandboxId)
    {
        uint result = XCrd.XCrdCreateXVD(_adapterHandle, crdPath, someId, createFlags, drivePages, xvdType, contentType, out outputGuid, productId, sandboxId, (uint)sandboxId.Length, null);
        if (result != 0)
        {
            Console.WriteLine("[x] Failed to create xvd! Result:" + result.ToString());
        }
        return result;
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

    public IntPtr QueryXvdInfo(string xvdPath)
    {
        uint result = XCrd.XCrdQueryXvdInformation(_adapterHandle, xvdPath, 0, out IntPtr infoBuffer, 128);
        if (result != 0)
        {
            Console.WriteLine("Failed to query xvd info: " + result.ToString());
            return IntPtr.Zero;
        }

        return infoBuffer;
    }

    public uint ReadUserdata(string srcPath, ulong offset, int length, string dstPath)
    {
        IntPtr buf = Marshal.AllocHGlobal(length);
        if (buf == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate buffer");
            return 1;
        }

        uint readLen = (uint)length;
        uint result = XCrd.XCrdReadUserDataXVD(_adapterHandle, srcPath, offset, buf, ref readLen);
        if (result != 0)
        {
            Console.WriteLine("Failed to read userdata: " + result.ToString());
            Marshal.FreeHGlobal(buf);
            return result;
        }
        else if (readLen != length) {
            Console.WriteLine("Mismatch: expected " + length + " bytes, got " + readLen + " bytes");
        }

        byte[] byteBuffer = new byte[readLen];
        Marshal.Copy(buf, byteBuffer, 0, (int)readLen);
        File.WriteAllBytes(dstPath, byteBuffer);

        Marshal.FreeHGlobal(buf);
        return 0;
    }

    /*
    Reads VBI file from System XVD

    Src paths to try:

    - F:\system.xvd
    - R:\A\system.xvd or [XSU:]\A\system.xvd
    - R:\B\system.xvd or [XSU:]\B\system.xvd

    */
    public uint ReadVbi(string srcPath, string dstPath)
    {
        byte[] lenBuf = new byte[0x10];
        IntPtr lenBufPtr = Marshal.AllocHGlobal(lenBuf.Length);
        if (lenBufPtr == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate len-buffer");
            return 1;
        }

        // Read first 0x10 bytes from header (offset 0x8 SizeOfHeaders, offset 0xC ImageSize)
        uint readLen = (uint)lenBuf.Length;
        uint result = XCrd.XCrdReadUserDataXVD(_adapterHandle, srcPath, 0, lenBufPtr, ref readLen);
        if (result != 0 || readLen != lenBuf.Length)
        {
            Console.WriteLine($"Failed to read userdata header: {result} readLen: {readLen}");
            Marshal.FreeHGlobal(lenBufPtr);
            return result;
        }

        // Copy header bytes to managed memory
        Marshal.Copy(lenBufPtr, lenBuf, 0, lenBuf.Length);

        // Read sizes from header
        uint size = BitConverter.ToUInt32(lenBuf, 0x8); // SizeOfHeader
        size += BitConverter.ToUInt32(lenBuf, 0xC); // ImageSize

        Console.WriteLine($"Full VBI Size: {size} bytes");

        // Read final VBI
        byte[] vbiBuf = new byte[size];
        IntPtr vbiBufPtr = Marshal.AllocHGlobal((int)size);
        if (vbiBufPtr == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate vbi-buffer");
            return 1;
        }

        readLen = size;
        result = XCrd.XCrdReadUserDataXVD(_adapterHandle, srcPath, 0, vbiBufPtr, ref readLen);
        if (result != 0 || readLen != size)
        {
            Console.WriteLine($"Failed to read userdata (3): {result} readLen: {readLen}");
            Marshal.FreeHGlobal(lenBufPtr);
            Marshal.FreeHGlobal(vbiBufPtr);
            return result;
        }

        Marshal.Copy(vbiBufPtr, vbiBuf, 0, (int)readLen);

        File.WriteAllBytes(dstPath, vbiBuf);

        Marshal.FreeHGlobal(lenBufPtr);
        Marshal.FreeHGlobal(vbiBufPtr);

        return result;
    }

    public uint EnumBlobs(uint xcrdId, string path, uint flags)
    {
        uint ret;
        if (xcrdId >= 0x25) {
            return 0x80070057;
        }

        var xcrdName = XCrd.XcrdIdMapping[xcrdId];

        Console.WriteLine($"Enumerating blob at {xcrdName}/{path}");

        if (path == "*") {
            path = null;
        }

        IntPtr pInfoBuffer = Marshal.AllocHGlobal(0x200);
        ret = XCrd.XCrdFindFirstBlob(out IntPtr hEnum, ref pInfoBuffer, xcrdId, path, flags);
        if (ret < 0) {
            Console.WriteLine("Failed to find first blob: 0x{ret:X}");
            return ret;
        }

        int pos = 0;
        do {
            Console.WriteLine("--- Entry {0} ---", pos);
            Console.WriteLine(XcrdInfoToString(pInfoBuffer));
            ret = XCrd.XCrdFindNextBlob(hEnum, ref pInfoBuffer);
            pos++;
        } while (ret != 0x80070012);

        XCrd.XCrdFindCloseBlob(hEnum);
        return ret;
    }

    string ReadUnicodeString(IntPtr ptr, uint offset, uint len)
    {
        byte[] buffer = new byte[len];
        GCHandle pinnedArray = GCHandle.Alloc(buffer, GCHandleType.Pinned);
        IntPtr ptrToBuffer = pinnedArray.AddrOfPinnedObject();
        Marshal.Copy(IntPtr.Add(ptr, (int)offset), buffer, 0, (int)len);
        return Encoding.Unicode.GetString(buffer);
    }

    string XcrdInfoToString(IntPtr ptrInfo)
    {
        var info = Marshal.PtrToStructure<XCrd.XCrdInfo>(ptrInfo);

        StringBuilder sb = new();
        sb.AppendFormat("{0}\n", ReadUnicodeString(ptrInfo, info.Filename.StringPtr, info.Filename.StringLen));
        sb.AppendFormat("Attributes ({0:X}) :\n", info.Attributes);
        if (info.Attributes.HasFlag(XCrd.XCrdAttributes.DetectBufferAvailable)) {
            sb.AppendLine("- DetectBufferAvailable"); 
        }
        if (info.Attributes.HasFlag(XCrd.XCrdAttributes.Directory)) {
            sb.AppendLine("- Directory"); 
        }
        if (info.Attributes.HasFlag(XCrd.XCrdAttributes.DosPathAvailable)) {
            sb.AppendLine("- DosPathAvailable"); 
        }
        sb.AppendFormat("Size: {0:X}\n", info.Size);
        //sb.AppendFormat("Crd ID: {0} (0x{0:X})\n", info.XCrdId);
        sb.AppendFormat("Crd Path: {0}\n", ReadUnicodeString(ptrInfo, info.CrdPath.StringPtr, info.CrdPath.StringLen));
        if (info.Attributes.HasFlag(XCrd.XCrdAttributes.DosPathAvailable))
        {
            sb.AppendLine("DOS Path: <Not available> (0)");
        }
        else
        {
            sb.AppendFormat("DOS Path: {0}\n", ReadUnicodeString(ptrInfo, info.DosPath.StringPtr, info.DosPath.StringLen));
        }
        sb.AppendFormat("Creation Time: {0}\n", info.CreationTime);
        //sb.AppendFormat("Detect HR: 0x{0:X}\n", info.DetectHr);
        sb.AppendLine("--------------------------------------");

        return sb.ToString();
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

    public uint GetBlobSize(string srcHostPath)
    {
        uint fileSize = 0;
        // Get filesize
        uint result = XCrd.XCrdStorageReadBlob(_adapterHandle, srcHostPath, IntPtr.Zero, ref fileSize);
        if (result != 0)
        {
            Console.WriteLine("Failed to enumerate filesize: " + result.ToString());
            return 0;
        }
        return fileSize;
    }

    public uint ReadBlob(string srcHostPath, string dstPath)
    {
        // Get filesize
        uint fileSize = GetBlobSize(srcHostPath);
        if (fileSize != 0)
        {
            Console.WriteLine("Failed to enumerate filesize for {0}", srcHostPath);
            return 1;
        }

        Console.WriteLine("FileSize: 0x{0:X} ({0})", fileSize);
        IntPtr buf = Marshal.AllocHGlobal((int)fileSize);
        if (buf == IntPtr.Zero)
        {
            Console.WriteLine("Failed to allocate buffer");
            return 1;
        }

        uint result = XCrd.XCrdStorageReadBlob(_adapterHandle, srcHostPath, buf, ref fileSize);
        if (result != 0)
        {
            Console.WriteLine("Failed to read blob: " + result.ToString());
            Marshal.FreeHGlobal(buf);
            return result;
        }

        uint position = 0;
        byte[] chunkBuffer = new byte[CHUNK_SIZE];
        using var outFile = File.Open(dstPath, FileMode.Create);
        while (position < fileSize)
        {
            uint numBytes = Math.Min(CHUNK_SIZE, fileSize - position);
            Marshal.Copy(IntPtr.Add(buf, (int)position), chunkBuffer, 0, (int)numBytes);
            outFile.Write(chunkBuffer, 0, (int)numBytes);
            position += numBytes;
        }

        Marshal.FreeHGlobal(buf);
        return 0;
    }

    public uint WriteBlob(string dstHostPath, byte[] data)
    {
        uint result = XCrd.XCrdStorageWriteBlob(_adapterHandle, dstHostPath, data, (uint)data.Length);
        if (result != 0)
        {
            Console.WriteLine("Failed to write blob: " + result.ToString());
        }

        return result;
    }

    public uint MoveBlob(string srcPath, string dstPath)
    {
        uint result = XCrd.XCrdStorageMoveBlob(_adapterHandle, srcPath, dstPath, 0);
        if (result != 0)
        {
            Console.WriteLine("Moving blob failed: " + result.ToString());
        }
        return result;
    }

    public uint DeleteBlob(string filePath)
    {
        uint result = XCrd.XCrdStorageDeleteBlob(_adapterHandle, filePath);
        if (result != 0)
        {
            Console.WriteLine("Deleting blob failed: " + result.ToString());
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
