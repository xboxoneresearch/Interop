
/*
Supports disabling of default firewalls and opening up ports that can be accessed remotely.

.NET port of landaire's hard work: https://github.com/exploits-forsale/solstice/blob/main/crates/solstice_daemon/src/firewall.rs

Example API usage:
```
   FirewallManager.DisableFirewalls();
   FirewallManager.AllowPortThroughFirewall("Debugger", 23946);
   FirewallManager.AllowPortThroughFirewall("SSH", 22);
```

Example E2E usage:
1. Copy .NET to `D:\XBOX\dotnet\`
2. Copy this DLL (`DurangoInteropDotnet.dll`) to `D:\XBOX\payloads\`
3. Copy `msbuild_tasks/disable_firewall.xml` to `D:\XBOX\payloads\disable_firewall.xml`
4. Execute the following commands:
```
    set DOTNET_CLI_TELEMETRY_OPTOUT=1
    set DOTNET_EnableWriteXorExecute=0
    D:\XBOX\dotnet\dotnet.exe msbuild "D:\XBOX\payloads\disable_firewall.xml"
```
*/
using System.Runtime.InteropServices.ComTypes;

namespace DurangoInteropDotnet {

    public static class FirewallManager {

        private static readonly string FWPM_DISPLAY_NAME = "XOR";
        private static readonly string FWPM_DISPLAY_DESCRIPTION = "XOR FWPM Provider";
        private static readonly Guid FWPM_PROVIDER_GUID = new Guid(0xabad1dea, 0x4141, 0x4141, 0x0, 0x0, 0x0c, 0x0f, 0xfe, 0xe0, 0x00, 0x00);
        private static readonly Guid CLSID_HNETCFGFWPOLICY2 = new Guid("e2b3c97f-6ae1-41ac-817a-f6f92166d7dd");
        private static readonly Guid CLSID_INETFWPOLICY2 = new Guid("98325047-C671-4174-8D81-DEFCD3F03186");

        public static void DisableFirewalls() {

            uint result = NativeBridge.CoInitializeEx(IntPtr.Zero, NativeBridge.COINIT.COINIT_MULTITHREADED);
            if (result != 0 && result != 1) { // 0 = S_OK, 1 = S_FALSE
                throw new Exception($"CoInitializeEx returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }

            IntPtr pINetFwPolicy2 = IntPtr.Zero;
            result = NativeBridge.CoCreateInstance(CLSID_HNETCFGFWPOLICY2, IntPtr.Zero, NativeBridge.CLSCTX.ALL, CLSID_INETFWPOLICY2, out pINetFwPolicy2);
            if (result != 0) {
                // 0x80040110 = CLASS_E_NOAGGREGATION
                // 0x80040154 = REGDB_E_CLASSNOTREG
                throw new Exception($"CoCreateInstance returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }
            if (pINetFwPolicy2 == IntPtr.Zero) {
                throw new Exception($"pINetFwPolicy2 is null (Error: 0x{Marshal.GetLastWin32Error():X})");
            }
            NativeBridge.INetFwPolicy2 iNetFwPolicy2 = (NativeBridge.INetFwPolicy2)Marshal.GetObjectForIUnknown(pINetFwPolicy2);

            /*
            foreach (NetFwProfileType2 profileType in new NetFwProfileType2[] { NetFwProfileType2.Public, NetFwProfileType2.Private, NetFwProfileType2.Domain }) {
                Console.WriteLine($"Profile Type: {profileType}");
                try {
                    Console.WriteLine($"\tFirewallEnabled: {iNetFwPolicy2.get_FirewallEnabled(profileType)}");
                    Console.WriteLine($"\tBlockAllInboundTraffic: {iNetFwPolicy2.get_BlockAllInboundTraffic(profileType)}");
                    Console.WriteLine($"\tDefaultInboundAction: {iNetFwPolicy2.get_DefaultInboundAction(profileType)}");
                    Console.WriteLine($"\tNotificationsDisabled: {iNetFwPolicy2.get_NotificationsDisabled(profileType)}");
                }
                catch (Exception e) {
                    Console.WriteLine($"{e.Message}");
                }
            }
            */

            //Console.WriteLine($"Updating profiles");
            foreach (NativeBridge.NetFwProfileType2 profileType in new NativeBridge.NetFwProfileType2[] { NativeBridge.NetFwProfileType2.Public, NativeBridge.NetFwProfileType2.Private, NativeBridge.NetFwProfileType2.Domain }) {
                // Console.WriteLine($"Modifying profile Type: {profileType}");

                try {
                    iNetFwPolicy2.set_BlockAllInboundTraffic(profileType, false);
                } catch (Exception e) {
                    throw new Exception("Unable to set BlockAllInboundTraffic", e);
                }

                try {
                    iNetFwPolicy2.set_FirewallEnabled(profileType, false);
                } catch (Exception e) {
                    throw new Exception("Unable to set FirewallEnabled", e);
                }

                try {
                    iNetFwPolicy2.set_DefaultInboundAction(profileType, NativeBridge.NetFwAction.Allow);
                } catch (Exception e) {
                    throw new Exception("Unable to set DefaultInboundAction", e);
                }

                try {
                    iNetFwPolicy2.set_NotificationsDisabled(profileType, true);
                } catch (Exception e) {
                    throw new Exception("Unable to set NotificationsDisabled", e);
                }
            }

            /*
            Console.WriteLine($"#### Verifying profiles");
            foreach (NetFwProfileType2 profileType in new NetFwProfileType2[] { NetFwProfileType2.Public, NetFwProfileType2.Private, NetFwProfileType2.Domain }) {
                Console.WriteLine($"Profile Type: {profileType}");
                try {
                    Console.WriteLine($"\tFirewallEnabled: {iNetFwPolicy2.get_FirewallEnabled(profileType)}");
                    Console.WriteLine($"\tBlockAllInboundTraffic: {iNetFwPolicy2.get_BlockAllInboundTraffic(profileType)}");
                    Console.WriteLine($"\tDefaultInboundAction: {iNetFwPolicy2.get_DefaultInboundAction(profileType)}");
                    Console.WriteLine($"\tNotificationsDisabled: {iNetFwPolicy2.get_NotificationsDisabled(profileType)}");
                }
                catch (Exception e) {
                    Console.WriteLine($"{e.Message}");
                }
            }
            */

            // Console.WriteLine($"Disabled the firewall");
        }

        public static void AllowPortThroughFirewall(string name, ushort port) {

            var engine = OpenFWPSession();
            try {
                InstallFWPMProvider(engine);
                BuildAndAddFWPPortFilter(name, port, NativeBridge.FirewallLayerGuids.FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4, engine);
            }
            finally {
                CloseFWPSession(engine);
            }

            // Console.WriteLine($"Opened up port {port} ({name}) in the firewall");
        }

        private static IntPtr OpenFWPSession() {
            IntPtr IntPtr = IntPtr.Zero;
            var result = NativeBridge.FwpmEngineOpen0(IntPtr.Zero, (uint)NativeBridge.RPC_C_AUTHN_DEFAULT, IntPtr.Zero, IntPtr.Zero, ref IntPtr);
            if (result != 0) {
                throw new Exception($"FwpmEngineOpen0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }
            // Console.WriteLine($"Opened FWPM Engine: 0x{IntPtr:X}");

            return IntPtr;
        }

        private static void CloseFWPSession(IntPtr IntPtr) {
            var result = NativeBridge.FwpmEngineClose0(IntPtr);
            if (result != 0) {
                throw new Exception($"FwpmEngineClose0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }
            // Console.WriteLine($"Closed FWPM Engine: 0x{IntPtr:X}");
        }

        private static void InstallFWPMProvider(IntPtr IntPtr) {

            var provider = new NativeBridge.FWPM_PROVIDER0 {
                providerKey = FWPM_PROVIDER_GUID,
                displayData = new NativeBridge.FWPM_DISPLAY_DATA0 {
                    name = FWPM_DISPLAY_NAME,
                    description = FWPM_DISPLAY_DESCRIPTION,
                },
                flags = NativeBridge.FirewallProviderFlags.Persistent,
            };

            uint result = NativeBridge.FwpmTransactionBegin0(IntPtr, 0);
            if (result != 0) {
                throw new Exception($"FwpmTransactionBegin0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }

            result = NativeBridge.FwpmProviderAdd0(IntPtr, ref provider, IntPtr.Zero);
            if (result == NativeBridge.FWP_E_ALREADY_EXISTS) {
                // Console.WriteLine($"FwpmProviderAdd0 already exists: FWP_E_ALREADY_EXISTS");
            } else if (result != 0) {
                throw new Exception($"FwpmProviderAdd0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }

            result = NativeBridge.FwpmTransactionCommit0(IntPtr); // 0x8032000D = FWP_E_NO_TXN_IN_PROGRESS
            if (result != 0) {
                throw new Exception($"FwpmTransactionCommit0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
            }

            // Console.WriteLine($"Added FWPM Provider");
        }

        private static void BuildAndAddFWPPortFilter(string name, ushort port, Guid layer, IntPtr IntPtr)
        {
            IntPtr pProviderKey = IntPtr.Zero;
            IntPtr pConditionArray = IntPtr.Zero;

            try
            {
                pProviderKey = Marshal.AllocHGlobal(Marshal.SizeOf<Guid>());
                Marshal.StructureToPtr(FWPM_PROVIDER_GUID, pProviderKey, false);

                NativeBridge.FWPM_FILTER0 filter = new NativeBridge.FWPM_FILTER0
                {
                    providerKey = pProviderKey,
                    displayData = new NativeBridge.FWPM_DISPLAY_DATA0
                    {
                        name = $"{FWPM_DISPLAY_NAME}: {name}",
                        description = $"Open port {port} for '{name}'",
                    },
                    layerKey = layer,
                    //flags = FirewallFilterFlags.Persistent,
                };
                filter.action.type = NativeBridge.FirewallActionType.Permit;

                var conditions = new[]
                {
                    new NativeBridge.FWPM_FILTER_CONDITION0
                    {
                        fieldKey = NativeBridge.FirewallLayerGuids.FWPM_CONDITION_IP_LOCAL_PORT,
                        matchType = NativeBridge.FWP_MATCH_TYPE.FWP_MATCH_EQUAL,
                        conditionValue = new NativeBridge.FWP_CONDITION_VALUE0
                        {
                            type = NativeBridge.FWP_DATA_TYPE.FWP_UINT16,
                            anonymous = new NativeBridge.FWP_CONDITION_VALUE0_UNION { uint16 = port },
                        },
                    },
                };

                pConditionArray = Marshal.AllocHGlobal(conditions.Length * Marshal.SizeOf<NativeBridge.FWPM_FILTER_CONDITION0>());
                for (int i = 0; i < conditions.Length; i++)
                {
                    Marshal.StructureToPtr(conditions[i],
                        pConditionArray + (i * Marshal.SizeOf<NativeBridge.FWPM_FILTER_CONDITION0>()), false);
                }

                filter.filterCondition = pConditionArray;
                filter.numFilterConditions = conditions.Length;

                IntPtr FilterId = IntPtr.Zero;
                uint result = NativeBridge.FwpmFilterAdd0(IntPtr, ref filter, IntPtr.Zero, ref FilterId);
                if (result != 0)
                {
                    throw new Exception($"FwpmFilterAdd0 returned a non-zero result: 0x{result:X} (Error: 0x{Marshal.GetLastWin32Error():X})");
                }
            }
            finally {
                if (pProviderKey != IntPtr.Zero) {
                    Marshal.FreeHGlobal(pProviderKey);
                }
                if (pConditionArray != IntPtr.Zero) {
                    Marshal.FreeHGlobal(pConditionArray);
                }
            }

            // Console.WriteLine($"Created Filter Id 0x{FilterId:X}");
        }

        internal static class NativeBridge {

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr LoadLibrary(string lpFileName);

            [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true, CallingConvention = CallingConvention.StdCall)]
            public static extern uint CoInitializeEx([In, Optional] IntPtr pvReserved, [In] COINIT dwCoInit);

            [DllImport("ole32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern uint CoCreateInstance(Guid rclsid, IntPtr pUnkOuter, CLSCTX dwClsContext, Guid riid, out IntPtr ppv);

            public delegate uint FwpmEngineOpen0Delegate(IntPtr serverName, uint authnService, IntPtr authIdentity, IntPtr session, ref IntPtr engineHandle);
            public static uint FwpmEngineOpen0(IntPtr serverName, uint authnService, IntPtr authIdentity, IntPtr session, ref IntPtr engineHandle) {
                return GetDelegate<FwpmEngineOpen0Delegate>("FWPUClnt.dll", "FwpmEngineOpen0")(serverName, authnService, authIdentity, session, ref engineHandle);
            }

            public delegate uint FwpmEngineClose0Delegate(IntPtr engineHandle);
            public static uint FwpmEngineClose0(IntPtr engineHandle) {
                return GetDelegate<FwpmEngineClose0Delegate>("FWPUClnt.dll", "FwpmEngineClose0")(engineHandle);
            }

            public delegate uint FwpmTransactionBegin0Delegate(IntPtr engineHandle, uint flags);
            public static uint FwpmTransactionBegin0(IntPtr engineHandle, uint flags) {
                return GetDelegate<FwpmTransactionBegin0Delegate>("FWPUClnt.dll", "FwpmTransactionBegin0")(engineHandle, flags);
            }

            public delegate uint FwpmTransactionCommit0Delegate(IntPtr engineHandle);
            public static uint FwpmTransactionCommit0(IntPtr engineHandle) {
                return GetDelegate<FwpmTransactionCommit0Delegate>("FWPUClnt.dll", "FwpmTransactionCommit0")(engineHandle);
            }

            public delegate uint FwpmFilterAdd0Delegate(IntPtr engineHandle, ref FWPM_FILTER0 filter, IntPtr sd, ref IntPtr id);
            public static uint FwpmFilterAdd0(IntPtr engineHandle, ref FWPM_FILTER0 filter, IntPtr sd, ref IntPtr id) {
                return GetDelegate<FwpmFilterAdd0Delegate>("FWPUClnt.dll", "FwpmFilterAdd0")(engineHandle, ref filter, sd, ref id);
            }

            public delegate uint FwpmProviderAdd0Delegate(IntPtr engineHandle, ref FWPM_PROVIDER0 provider, IntPtr sd);
            public static uint FwpmProviderAdd0(IntPtr engineHandle, ref FWPM_PROVIDER0 provider, IntPtr sd) {
                return GetDelegate<FwpmProviderAdd0Delegate>("FWPUClnt.dll", "FwpmProviderAdd0")(engineHandle, ref provider, sd);
            }

            // UWP won't allow importing from FWPUClnt.dll
            public static T GetDelegate<T>(string DllName, string FunctionName) {
                var moduleHandle = GetModuleHandle(DllName);
                if (moduleHandle == IntPtr.Zero) {
                    moduleHandle = LoadLibrary(DllName);
                    if (moduleHandle == IntPtr.Zero) {
                        throw new Exception($"Cannot natively load the DLL {DllName} for method delegation");
                    }
                }
                var functionHandle = GetProcAddress(moduleHandle, FunctionName);
                if (functionHandle == IntPtr.Zero) {
                    throw new Exception($"Cannot find {DllName}!{FunctionName}");
                }
                return Marshal.GetDelegateForFunctionPointer<T>(functionHandle);
            }

            public const ulong RPC_C_AUTHN_DEFAULT = 0xFFFFFFFFL; // The system default authentication service
            public const uint FWP_E_ALREADY_EXISTS = 0x80320009;

            public enum COINIT : uint {
                COINIT_MULTITHREADED = 0x0,
                COINIT_APARTMENTTHREADED = 0x2,
                COINIT_DISABLE_OLE1DDE = 0x4,
                COINIT_SPEED_OVER_MEMORY = 0x8,
            }

            [Flags]
            public enum CLSCTX : uint {
                INPROC_SERVER = 0x1,
                INPROC_HANDLER = 0x2,
                LOCAL_SERVER = 0x4,
                INPROC_SERVER16 = 0x8,
                REMOTE_SERVER = 0x10,
                INPROC_HANDLER16 = 0x20,
                RESERVED1 = 0x40,
                RESERVED2 = 0x80,
                RESERVED3 = 0x100,
                RESERVED4 = 0x200,
                NO_CODE_DOWNLOAD = 0x400,
                RESERVED5 = 0x800,
                NO_CUSTOM_MARSHAL = 0x1000,
                ENABLE_CODE_DOWNLOAD = 0x2000,
                NO_FAILURE_LOG = 0x4000,
                DISABLE_AAA = 0x8000,
                ENABLE_AAA = 0x10000,
                FROM_DEFAULT_CONTEXT = 0x20000,
                INPROC = INPROC_SERVER | INPROC_HANDLER,
                SERVER = INPROC_SERVER | LOCAL_SERVER | REMOTE_SERVER,
                ALL = SERVER | INPROC_HANDLER
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct FWPM_FILTER0 {
                public Guid filterKey;
                public FWPM_DISPLAY_DATA0 displayData;
                public FirewallFilterFlags flags;
                public IntPtr providerKey; // GUID*
                public FWP_BYTE_BLOB providerData;
                public Guid layerKey;
                public Guid subLayerKey;
                public FWP_VALUE0 weight;
                public int numFilterConditions;
                public IntPtr filterCondition; // FWPM_FILTER_CONDITION0* 
                public FWPM_ACTION0 action;
                public FWPM_FILTER0_UNION context;
                public IntPtr reserved; // GUID* 
                public ulong filterId;
                public FWP_VALUE0 effectiveWeight;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FWPM_FILTER0_UNION {
                [FieldOffset(0)]
                public ulong rawContext;
                [FieldOffset(0)]
                public Guid providerContextKey;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FWPM_ACTION0_UNION {
                [FieldOffset(0)]
                public Guid filterType;
                [FieldOffset(0)]
                public Guid calloutKey;
                [FieldOffset(0)]
                public byte bitmapIndex;
            }

            [StructLayoutAttribute(LayoutKind.Sequential)]
            public struct FWPM_FILTER_CONDITION0 {
                public Guid fieldKey;
                public FWP_MATCH_TYPE matchType;
                public FWP_CONDITION_VALUE0 conditionValue;
            }

            [StructLayoutAttribute(LayoutKind.Sequential)]
            public struct FWP_CONDITION_VALUE0 {
                public FWP_DATA_TYPE type;
                public FWP_CONDITION_VALUE0_UNION anonymous;
            }

            [StructLayoutAttribute(LayoutKind.Explicit)]
            public struct FWP_CONDITION_VALUE0_UNION {
                [FieldOffsetAttribute(0)]
                public byte uint8;
                [FieldOffsetAttribute(0)]
                public ushort uint16;
                [FieldOffsetAttribute(0)]
                public int uint32;
                [FieldOffsetAttribute(0)]
                public System.IntPtr uint64;
                [FieldOffsetAttribute(0)]
                public byte int8;
                [FieldOffsetAttribute(0)]
                public short int16;
                [FieldOffsetAttribute(0)]
                public int int32;
                [FieldOffsetAttribute(0)]
                public System.IntPtr int64;
                [FieldOffsetAttribute(0)]
                public float float32;
                [FieldOffsetAttribute(0)]
                public System.IntPtr double64;
                [FieldOffsetAttribute(0)]
                public System.IntPtr byteArray16;
                [FieldOffsetAttribute(0)]
                public System.IntPtr byteBlob;
                [FieldOffsetAttribute(0)]
                public System.IntPtr sid;
                [FieldOffsetAttribute(0)]
                public System.IntPtr sd;
                [FieldOffsetAttribute(0)]
                public System.IntPtr tokenInformation;
                [FieldOffsetAttribute(0)]
                public System.IntPtr tokenAccessInformation;
                [FieldOffsetAttribute(0)]
                public System.IntPtr unicodeString;
                [FieldOffsetAttribute(0)]
                public System.IntPtr byteArray6;
                [FieldOffsetAttribute(0)]
                public System.IntPtr v4AddrMask;
                [FieldOffsetAttribute(0)]
                public System.IntPtr v6AddrMask;
                [FieldOffsetAttribute(0)]
                public System.IntPtr rangeValue;
            }

            public enum FWP_DATA_TYPE : int {
                FWP_EMPTY = 0,
                FWP_UINT8 = 1,
                FWP_UINT16 = 2,
                FWP_UINT32 = 3,
                FWP_UINT64 = 4,
                FWP_INT8 = 5,
                FWP_INT16 = 6,
                FWP_INT32 = 7,
                FWP_INT64 = 8,
                FWP_FLOAT = 9,
                FWP_DOUBLE = 10,
                FWP_BYTE_ARRAY16_TYPE = 11,
                FWP_BYTE_BLOB_TYPE = 12,
                FWP_SID = 13,
                FWP_SECURITY_DESCRIPTOR_TYPE = 14,
                FWP_TOKEN_INFORMATION_TYPE = 15,
                FWP_TOKEN_ACCESS_INFORMATION_TYPE = 16,

                /// FWP_UNICODE_STRING_TYPE -> 17
                FWP_UNICODE_STRING_TYPE = 17,

                /// FWP_BYTE_ARRAY6_TYPE -> 18
                FWP_BYTE_ARRAY6_TYPE = 18,

                /// FWP_SINGLE_DATA_TYPE_MAX -> 0xff
                FWP_SINGLE_DATA_TYPE_MAX = 255,

                /// FWP_V4_ADDR_MASK -> 0x100
                FWP_V4_ADDR_MASK = 256,

                /// FWP_V6_ADDR_MASK -> 0x101
                FWP_V6_ADDR_MASK = 257,

                /// FWP_RANGE_TYPE -> 0x102
                FWP_RANGE_TYPE = 258,

                /// FWP_DATA_TYPE_MAX -> 0x103
                FWP_DATA_TYPE_MAX = 259,
            }

            public enum FWP_MATCH_TYPE : int {
                FWP_MATCH_EQUAL = 0,
                FWP_MATCH_GREATER = 1,
                FWP_MATCH_LESS = 2,
                FWP_MATCH_GREATER_OR_EQUAL = 3,
                FWP_MATCH_LESS_OR_EQUAL = 4,
                FWP_MATCH_RANGE = 5,
                FWP_MATCH_FLAGS_ALL_SET = 6,
                FWP_MATCH_FLAGS_ANY_SET = 7,
                FWP_MATCH_FLAGS_NONE_SET = 8,
                FWP_MATCH_EQUAL_CASE_INSENSITIVE = 9,
                FWP_MATCH_NOT_EQUAL = 10,
                FWP_MATCH_TYPE_MAX = 11,
            }


            [StructLayout(LayoutKind.Sequential)]
            public struct FWPM_ACTION0 {
                public FirewallActionType type;
                public FWPM_ACTION0_UNION action;
            }

            [StructLayout(LayoutKind.Explicit)]
            public struct FWP_VALUE0_UNION {
                [FieldOffset(0)]
                public byte uint8;
                [FieldOffset(0)]
                public ushort uint16;
                [FieldOffset(0)]
                public uint uint32;
                [FieldOffset(0)]
                public IntPtr uint64; // UINT64*
                [FieldOffset(0)]
                public sbyte int8;
                [FieldOffset(0)]
                public short int16;
                [FieldOffset(0)]
                public int int32;
                [FieldOffset(0)]
                public IntPtr int64; // INT64* 
                [FieldOffset(0)]
                public float float32;
                [FieldOffset(0)]
                public IntPtr double64; // double* 
                [FieldOffset(0)]
                public IntPtr byteArray16; // FWP_BYTE_ARRAY16* 
                [FieldOffset(0)]
                public IntPtr byteBlob; // FWP_BYTE_BLOB*
                [FieldOffset(0)]
                public IntPtr sid; // SID* 
                [FieldOffset(0)]
                public IntPtr sd; // FWP_BYTE_BLOB* 
                [FieldOffset(0)]
                public IntPtr tokenInformation; // FWP_TOKEN_INFORMATION* 
                [FieldOffset(0)]
                public IntPtr tokenAccessInformation; // FWP_BYTE_BLOB* 
                [FieldOffset(0)]
                public IntPtr unicodeString; // LPWSTR 
                [FieldOffset(0)]
                public IntPtr byteArray6; // FWP_BYTE_ARRAY6* 
                [FieldOffset(0)]
                public IntPtr bitmapArray64; // FWP_BITMAP_ARRAY64*
                [FieldOffset(0)]
                public IntPtr v4AddrMask; // FWP_V4_ADDR_AND_MASK* 
                [FieldOffset(0)]
                public IntPtr v6AddrMask; // FWP_V6_ADDR_AND_MASK* 
                [FieldOffset(0)]
                public IntPtr rangeValue; // FWP_RANGE0* 
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FWP_VALUE0 {
                public FirewallDataType type;
                public FWP_VALUE0_UNION value;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FWP_BYTE_BLOB {
                public int size;
                /* [unique][size_is] */
                public IntPtr data;

                public byte[] ToArray() {
                    if (size <= 0 || data == IntPtr.Zero) {
                        return new byte[0];
                    }
                    byte[] ret = new byte[size];
                    Marshal.Copy(data, ret, 0, ret.Length);
                    return ret;
                }

                public Guid ToGuid() {
                    var bytes = ToArray();
                    if (bytes.Length != 16)
                        return Guid.Empty;
                    return new Guid(bytes);
                }
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct FWPM_DISPLAY_DATA0 {
                [MarshalAs(UnmanagedType.LPWStr)]
                public string name;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string description;
            }

            [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
            public struct FWPM_PROVIDER0 {
                public Guid providerKey;
                public FWPM_DISPLAY_DATA0 displayData;
                public FirewallProviderFlags flags;
                public FWP_BYTE_BLOB providerData;
                [MarshalAs(UnmanagedType.LPWStr)]
                public string serviceName;
            }

            // https://github.com/googleprojectzero/sandbox-attacksurface-analysis-tools/blob/280826ad554f33e5e799d9b860a68c2b7becbc06/NtApiDotNet/Net/Firewall/FirewallLayerGuids.cs#L22
            public static class FirewallLayerGuids {
                public static Guid FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4 = new Guid(0xe1cd9fe7, 0xf4b5, 0x4273, 0x96, 0xc0, 0x59, 0x2e, 0x48, 0x7b, 0x86, 0x50);
                public static Guid FWPM_LAYER_ALE_AUTH_RECV_ACCEPT_V4_DISCARD = new Guid(0x9eeaa99b, 0xbd22, 0x4227, 0x91, 0x9f, 0x00, 0x73, 0xc6, 0x33, 0x57, 0xb1);
                public static Guid FWPM_CONDITION_IP_LOCAL_PORT = new Guid(0x0c1ba1af, 0x5765, 0x453f, 0xaf, 0x22, 0xa8, 0xf7, 0x91, 0xac, 0x77, 0x5b);
            }

            [Flags]
            public enum FirewallProviderFlags {
                None = 0,
                Persistent = 0x00000001,
                Disabled = 0x00000010
            }

            public enum FirewallDataType {
                Empty = 0,
                UInt8 = Empty + 1,
                UInt16 = UInt8 + 1,
                UInt32 = UInt16 + 1,
                UInt64 = UInt32 + 1,
                Int8 = UInt64 + 1,
                Int16 = Int8 + 1,
                Int32 = Int16 + 1,
                Int64 = Int32 + 1,
                Float = Int64 + 1,
                Double = Float + 1,
                ByteArray16 = Double + 1,
                ByteBlob = ByteArray16 + 1,
                Sid = ByteBlob + 1,
                SecurityDescriptor = Sid + 1,
                TokenInformation = SecurityDescriptor + 1,
                TokenAccessInformation = TokenInformation + 1,
                UnicodeString = TokenAccessInformation + 1,
                ByteArray6 = UnicodeString + 1,
                BitmapIndex = ByteArray6 + 1,
                BitmapArray64 = BitmapIndex + 1,
                SingleDataTypeMax = 0xff,
                V4AddrMask = SingleDataTypeMax + 1,
                V6AddrMask = V4AddrMask + 1,
                Range = V6AddrMask + 1
            }
            public enum FirewallActionType : uint {
                Terminating = 0x00001000,
                Block = 0x00000001 | Terminating,
                Permit = 0x00000002 | Terminating,
                CalloutTerminating = 0x00000003 | Callout | Terminating,
                CalloutInspection = 0x00000004 | Callout | NonTerminating,
                CalloutUnknown = 0x00000005 | Callout,
                Continue = 0x00000006 | NonTerminating,
                None = 0x00000007,
                NoneNoMatch = 0x00000008,
                BitmapIndexSet = 0x00000009,
                NonTerminating = 0x00002000,
                Callout = 0x00004000,
                All = 0xFFFFFFFF
            }

            [Flags]
            public enum FirewallFilterFlags {
                None = 0x00000000,
                Persistent = 0x00000001,
                Boottime = 0x00000002,
                HasProviderContext = 0x00000004,
                ClearActionRight = 0x00000008,
                PermitIfCalloutUnregistered = 0x00000010,
                Disabled = 0x00000020,
                Indexed = 0x00000040,
                HasSecurityRealmProviderContext = 0x00000080,
                SystemOSOnly = 0x00000100,
                GameOSOnly = 0x00000200,
                SilentMode = 0x00000400,
                IPSecNoAcquireInitiate = 0x00000800,
            }

            public enum NetFwProfileType2 {
                Domain = 0x00000001,
                Private = 0x00000002,
                Public = 0x00000004,
                All = 0x7FFFFFFF
            }

            public enum NetFwAction {
                Block,
                Allow
            }
            public enum NetFwRuleDirection {
                Inbound = 1,
                Outbound = 2
            }
            public enum NetFwModifyState {
                Ok,
                GroupPolicyOverride,
                InboundBlocked
            }

            [Guid("98325047-C671-4174-8D81-DEFCD3F03186")]
            [ComImport]
            public interface INetFwPolicy2 {
                [DispId(1)]
                int CurrentProfileTypes {
                    [DispId(1)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                }

                [DispId(2)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool get_FirewallEnabled([In] NetFwProfileType2 profileType);

                [DispId(2)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_FirewallEnabled([In] NetFwProfileType2 profileType, [In] bool enabled);

                [DispId(3)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                object get_ExcludedInterfaces([In] NetFwProfileType2 profileType);

                [DispId(3)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_ExcludedInterfaces(
                    [In] NetFwProfileType2 profileType,
                    [In] object interfaces);

                [DispId(4)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool get_BlockAllInboundTraffic([In] NetFwProfileType2 profileType);

                [DispId(4)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_BlockAllInboundTraffic([In] NetFwProfileType2 profileType, [In] bool block);

                [DispId(5)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool get_NotificationsDisabled([In] NetFwProfileType2 profileType);

                [DispId(5)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_NotificationsDisabled([In] NetFwProfileType2 profileType, [In] bool disabled);

                [DispId(6)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool get_UnicastResponsesToMulticastBroadcastDisabled([In] NetFwProfileType2 profileType);

                [DispId(6)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_UnicastResponsesToMulticastBroadcastDisabled(
                    [In] NetFwProfileType2 profileType,
                    [In] bool disabled
                );

                [DispId(7)]
                INetFwRules Rules {
                    [DispId(7)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.Interface)]
                    get;
                }

                [DispId(8)]
                INetFwServiceRestriction ServiceRestriction {
                    [DispId(8)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.Interface)]
                    get;
                }

                [DispId(9)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void EnableRuleGroup(
                    [In] int profileTypesBitmask,
                    [MarshalAs(UnmanagedType.BStr)][In] string group,
                    [In] bool enable
                );

                [DispId(10)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool IsRuleGroupEnabled([In] int profileTypesBitmask, [MarshalAs(UnmanagedType.BStr)][In] string group);

                [DispId(11)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void RestoreLocalFirewallDefaults();

                [DispId(12)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                NetFwAction get_DefaultInboundAction([In] NetFwProfileType2 profileType);

                [DispId(12)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_DefaultInboundAction([In] NetFwProfileType2 profileType, [In] NetFwAction action);

                [DispId(13)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                NetFwAction get_DefaultOutboundAction([In] NetFwProfileType2 profileType);

                [DispId(13)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void set_DefaultOutboundAction([In] NetFwProfileType2 profileType, [In] NetFwAction action);

                [DispId(14)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool get_IsRuleGroupCurrentlyEnabled([MarshalAs(UnmanagedType.BStr)][In] string group);

                [DispId(15)]
                NetFwModifyState LocalPolicyModifyState {
                    [DispId(15)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                }
            }

            [Guid("9C4C6277-5027-441E-AFAE-CA1F542DA009")]
            [ComImport]
            public interface INetFwRules : System.Collections.IEnumerable {
                [DispId(1)]
                int Count {
                    [DispId(1)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                }

                [DispId(2)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                // ReSharper disable once MethodNameNotMeaningful
                void Add(
                    [MarshalAs(UnmanagedType.Interface)] [In]
                    INetFwRule rule
                );

                [DispId(3)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                void Remove([MarshalAs(UnmanagedType.BStr)][In] string name);

                [DispId(4)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                [return: MarshalAs(UnmanagedType.Interface)]
                INetFwRule Item([MarshalAs(UnmanagedType.BStr)][In] string name);

                [DispId(-4)]
                IEnumVARIANT GetEnumeratorVariant();
            }

            [Guid("AF230D27-BABA-4E42-ACED-F524F22CFCE2")]
            [ComImport]
            public interface INetFwRule {
                [DispId(1)]
                string Name {
                    [DispId(1)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(1)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(2)]
                string Description {
                    [DispId(2)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(2)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(3)]
                string ApplicationName {
                    [DispId(3)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(3)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(4)]
                string serviceName {
                    [DispId(4)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(4)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(5)]
                int Protocol {
                    [DispId(5)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(5)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(6)]
                string LocalPorts {
                    [DispId(6)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(6)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(7)]
                string RemotePorts {
                    [DispId(7)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(7)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(8)]
                string LocalAddresses {
                    [DispId(8)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(8)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(9)]
                string RemoteAddresses {
                    [DispId(9)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(9)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(10)]
                string IcmpTypesAndCodes {
                    [DispId(10)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(10)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(11)]
                NetFwRuleDirection Direction {
                    [DispId(11)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(11)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(12)]
                object Interfaces {
                    [DispId(12)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(12)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(13)]
                string InterfaceTypes {
                    [DispId(13)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(13)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(14)]
                bool Enabled {
                    [DispId(14)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(14)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(15)]
                string Grouping {
                    [DispId(15)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.BStr)]
                    get;
                    [DispId(15)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: MarshalAs(UnmanagedType.BStr)]
                    [param: In]
                    set;
                }

                [DispId(16)]
                int Profiles {
                    [DispId(16)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(16)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(17)]
                bool EdgeTraversal {
                    [DispId(17)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(17)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }

                [DispId(18)]
                NetFwAction Action {
                    [DispId(18)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    get;
                    [DispId(18)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [param: In]
                    set;
                }
            }

            [Guid("8267BBE3-F890-491C-B7B6-2DB1EF0E5D2B")]
            [ComImport]
            public interface INetFwServiceRestriction {
                [DispId(1)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                // ReSharper disable once TooManyArguments
                void RestrictService(
                    [MarshalAs(UnmanagedType.BStr)][In] string serviceName,
                    [MarshalAs(UnmanagedType.BStr)][In] string appName,
                    [In] bool restrictService,
                    [In] bool serviceSIDRestricted
                );

                [DispId(2)]
                [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                bool ServiceRestricted(
                    [MarshalAs(UnmanagedType.BStr)][In] string serviceName,
                    [MarshalAs(UnmanagedType.BStr)][In] string appName
                );

                [DispId(3)]
                INetFwRules Rules {
                    [DispId(3)]
                    [MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
                    [return: MarshalAs(UnmanagedType.Interface)]
                    get;
                }
            }
        }
    }
}
