public class UserManager : IDisposable
{
    private static readonly string RuntimeClass_Windows_Xbox_System_Internal_UserManager = "Windows.Xbox.System.Internal.UserManager";

    // Interface is a part of the implementation of type Windows.Xbox.System.Internal.UserManager
    private static readonly byte[] IID_IConsoleUserManagement = Guid.Parse("D65AE869-659F-4AF9-8393-99FB0CF7F22C").ToByteArray();

    /*
    // Windows.Xbox.System.Internal.IConsoleUserManagement::get_ConsoleUsers, method index: 6
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementGetConsoleUsersDelegate(IntPtr instance, out IntPtr consoleUsers);
    */

    // Windows.Xbox.System.Internal.IConsoleUserManagement::CreateConsoleUser, method index: 7
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementCreateConsoleUserDelegate(IntPtr instance, IntPtr emailAddress, byte persistCredentials, out uint retval);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::DeleteConsoleUser, method index: 8
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementDeleteConsoleUserDelegate(IntPtr instance, uint consoleUserId);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::UpdateConsoleUser, method index: 9
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementUpdateConsoleUserDelegate(IntPtr instance, uint consoleUserId, IntPtr emailAddress, byte persistCredentials, byte enableKinectSignin);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::ClearNewUserStatus, method index: 10
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementClearNewUserStatusDelegate(IntPtr instance, uint consoleUserId);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::AllocateSponsoredUserId, method index: 11
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementAllocateSponsoredUserIdDelegate(IntPtr instance, out uint retval);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::FreeSponsoredUserId, method index: 12
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementFreeSponsoredUserIdDelegate(IntPtr instance, uint consoleUserId);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::IsUserIdValidForLocalStorage, method index: 13
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementIsUserIdValidForLocalStorageDelegate(IntPtr instance, uint consoleUserId);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::UpdateConsoleUserSignIn, method index: 16
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementUpdateConsoleUserSignInDelegate(IntPtr instance, uint consoleUserId, byte persistCredentials, byte enableKinectSignIn, byte challengeSignIn, byte signOutSpopForKinectSignIn);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::UpdateConsoleUserEmail, method index: 17
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementUpdateConsoleUserEmailDelegate(IntPtr instance, uint consoleUserId, IntPtr emailAddress);

    // Windows.Xbox.System.Internal.IConsoleUserManagement::UpdateConsoleUserAutoSignIn, method index: 18
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    private delegate IntPtr IConsoleUserManagementUpdateConsoleUserAutoSignInDelegate(IntPtr instance, uint consoleUserId, byte autoSignIn);


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsSucceeded(IntPtr result) => (long)result >= 0L;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte BoolToByte(bool value) => value ? (byte)1 : (byte)0;

    private static bool _roInitialized = false;
    private static IntPtr _classId = IntPtr.Zero;
    private static IntPtr _instance = IntPtr.Zero;

    static UserManager()
    {
        if (!IsSucceeded(WinRT.RoInitialize(WinRT.RO_INIT_TYPE.RO_INIT_SINGLETHREADED)))
        {
            throw new Exception("RoInitialize failed");
        }
        _roInitialized = true;

        if (!IsSucceeded(WinRT.WindowsCreateString(RuntimeClass_Windows_Xbox_System_Internal_UserManager, RuntimeClass_Windows_Xbox_System_Internal_UserManager.Length, out _classId)))
        {
            throw new Exception("WindowsCreateString (classID) failed");
        }

        if (!IsSucceeded(WinRT.RoGetActivationFactory(_classId, IID_IConsoleUserManagement, out _instance)))
        {
            throw new Exception("RoGetActivationFactory failed");
        }       
    }

    /*
    public static IReadOnlyList<ConsoleUser> GetConsoleUsers()
    {
        throw new NotImplementedException();
    }
    */

    public static uint CreateConsoleUser(string emailAddress, bool persistCredentials)
    {
        if (!IsSucceeded(WinRT.WindowsCreateString(emailAddress, emailAddress.Length, out IntPtr emailAddrPtr)))
        {
            throw new Exception("Failed to initialize email HSTRING");
        }

        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementCreateConsoleUserDelegate>(_instance, 7);
        method.Invoke(_instance, emailAddrPtr, BoolToByte(persistCredentials), out uint ret);
        WinRT.WindowsDeleteString(emailAddrPtr);
        return ret;
    }

    public static void DeleteConsoleUser(uint consoleUserId)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementDeleteConsoleUserDelegate>(_instance, 8);
        method.Invoke(_instance, consoleUserId);
    }

    public static void UpdateConsoleUser(uint consoleUserId, string emailAddress, bool persistCredentials, bool enableKinectSignin)
    {
        if (!IsSucceeded(WinRT.WindowsCreateString(emailAddress, emailAddress.Length, out IntPtr emailAddrPtr)))
        {
            throw new Exception("Failed to initialize email HSTRING");
        }

        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementUpdateConsoleUserDelegate>(_instance, 9);
        method.Invoke(_instance, consoleUserId, emailAddrPtr, BoolToByte(persistCredentials), BoolToByte(enableKinectSignin));

        WinRT.WindowsDeleteString(emailAddrPtr);
    }

    public static void ClearNewUserStatus(uint consoleUserId)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementClearNewUserStatusDelegate>(_instance, 10);
        method.Invoke(_instance, consoleUserId);
    }

    public static uint AllocateSponsoredUserId()
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementAllocateSponsoredUserIdDelegate>(_instance, 11);
        method.Invoke(_instance, out uint ret);
        return ret;
    }

    public static void FreeSponsoredUserId(uint consoleUserId)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementFreeSponsoredUserIdDelegate>(_instance, 12);
        method.Invoke(_instance, consoleUserId);
    }

    public static void IsUserIdValidForLocalStorage(uint consoleUserId)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementIsUserIdValidForLocalStorageDelegate>(_instance, 13);
        method.Invoke(_instance, consoleUserId);
    }

	public static void UpdateConsoleUserSignIn(uint consoleUserId, bool persistCredentials, bool enableKinectSignIn, bool challengeSignIn, bool signOutSpopForKinectSignIn)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementUpdateConsoleUserSignInDelegate>(_instance, 16);
        method.Invoke(_instance, consoleUserId, BoolToByte(persistCredentials), BoolToByte(enableKinectSignIn), BoolToByte(challengeSignIn), BoolToByte(signOutSpopForKinectSignIn));
    }

	public static void UpdateConsoleUserEmail(uint consoleUserId, string emailAddress)
    {
        if (!IsSucceeded(WinRT.WindowsCreateString(emailAddress, emailAddress.Length, out IntPtr emailAddrPtr)))
        {
            throw new Exception("Failed to initialize email HSTRING");
        }

        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementUpdateConsoleUserEmailDelegate>(_instance, 17);
        method.Invoke(_instance, consoleUserId, emailAddrPtr);

        WinRT.WindowsDeleteString(emailAddrPtr);
    }

	public static void UpdateConsoleUserAutoSignIn(uint consoleUserId, bool autoSignIn)
    {
        var method = WinRT.GetVirtualMethodPointer<IConsoleUserManagementUpdateConsoleUserAutoSignInDelegate>(_instance, 18);
        method.Invoke(_instance, consoleUserId, BoolToByte(autoSignIn));
    }

    public void Dispose()
    {
        if (_instance != IntPtr.Zero)
            Marshal.Release(_instance);
        if (_classId != IntPtr.Zero)
            WinRT.WindowsDeleteString(_classId);
        if (_roInitialized)
            WinRT.RoUninitialize();
    }
}
