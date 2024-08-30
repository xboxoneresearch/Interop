public static class WinUser
{
    public enum SID_NAME_USE : uint
    {
        SidTypeUser =  1,
        SidTypeGroup,
        SidTypeDomain,
        SidTypeAlias,
        SidTypeWellKnownGroup,
        SidTypeDeletedAccount,
        SidTypeInvalid,
        SidTypeUnknown,
        SidTypeComputer,
        SidTypeLabel
    }

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool LookupAccountName(
        string lpSystemName,
        string lpAccountName,
        IntPtr Sid,
        ref uint cbSid,
        StringBuilder referencedDomainName,
        ref uint cchReferencedDomainName,
        out SID_NAME_USE peUse);

    [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int CreateProfile(
        [MarshalAs(UnmanagedType.LPWStr)] string pszUserSid,
        [MarshalAs(UnmanagedType.LPWStr)] string pszUserName,
        [Out][MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszProfilePath,
        uint cchProfilePath
    );

    [DllImport("userenv.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool DeleteProfileW(
        [MarshalAs(UnmanagedType.LPWStr)] string lpszSidString,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszProfilePath,
        [MarshalAs(UnmanagedType.LPWStr)] string lpszComputerName
    );

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int NetUserAdd(
        [MarshalAs(UnmanagedType.LPWStr)] string servername,
        UInt32 level,
        ref USER_INFO_1 userinfo,
        out UInt32 parm_err
    );

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int NetUserDel(
        [MarshalAs(UnmanagedType.LPWStr)] string servername,
        [MarshalAs(UnmanagedType.LPWStr)] string username
    );

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int NetLocalGroupAddMembers(
        string servername,
        string groupname,
        UInt32 level,
        ref LOCALGROUP_MEMBERS_INFO_3 members,
        UInt32 totalentries
    );

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int NetLocalGroupDelMembers(
        string servername,
        string groupname,
        UInt32 level,
        ref LOCALGROUP_MEMBERS_INFO_3 members,
        UInt32 totalentries
    );

    [DllImport("netapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern int NetUserEnum(
        [MarshalAs(UnmanagedType.LPWStr)] string servername,
        int level,
        int filter,
        out IntPtr bufptr,
        int prefmaxlen,
        out int entriesread,
        out int totalentries,
        out int resume_handle
    );

    [DllImport("netapi32.dll")]
    public static extern int NetApiBufferFree(IntPtr buffer);

    // Constants for NetUserEnum function
    public const int FILTER_NORMAL_ACCOUNT = 0x0002;
    public const int NERR_Success = 0;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct USER_INFO_0
    {
        public string name;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct USER_INFO_1
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string sUsername;
        [MarshalAs(UnmanagedType.LPWStr)] public string sPassword;
        public uint uiPasswordAge;
        public uint uiPriv;
        [MarshalAs(UnmanagedType.LPWStr)] public string sHome_Dir;
        [MarshalAs(UnmanagedType.LPWStr)] public string sComment;
        public uint uiFlags;
        [MarshalAs(UnmanagedType.LPWStr)] public string sScript_Path;
    }


    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct LOCALGROUP_MEMBERS_INFO_3
    {
        public IntPtr lgrmi3_domainandname;
    }
}

public static class WinUserManager
{
    /// <summary>
    /// Add user by username and password
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns>0 on success</returns>
    public static int CreateAccount(string username, string password)
    {
        WinUser.USER_INFO_1 userInfo = new WinUser.USER_INFO_1
        {
            sUsername = username,
            sPassword = password,
            uiPasswordAge = 0,
            uiPriv = 1, // USER_PRIV_USER
            sHome_Dir = null,
            sComment = "xboxacc",
            uiFlags = 0x0040 | 0x0200, // UF_PASSWD_CANT_CHANGE or 0
            sScript_Path = null
        };

        Console.WriteLine("Adding user...");

        int result = WinUser.NetUserAdd(null, 1, ref userInfo, out _);
        if (result != 0)
        {
            Console.WriteLine("NetUserAdd failed with error code: " + result);
            return result;
        }

        AddAccountToGroup(username, "Administrators");
        return 0;
    }

    /// <summary>
    /// Add Account to group by name
    /// </summary>
    /// <param name="username">Target username</param>
    /// <param name="group">Group to add to</param>
    /// <returns>0 on success</returns>
    public static int AddAccountToGroup(string username, string group)
    {
        WinUser.LOCALGROUP_MEMBERS_INFO_3 membersInfo = new WinUser.LOCALGROUP_MEMBERS_INFO_3
        {
            lgrmi3_domainandname = Marshal.StringToCoTaskMemUni(username)
        };
        int result = WinUser.NetLocalGroupAddMembers(null, group, 3, ref membersInfo, 1);
        if (result != 0)
        {
            Console.WriteLine("NetLocalGroupAddMembers failed with error code: " + result);
        }
        else {
            Console.WriteLine($"User added to group: {group}");
        }
        Marshal.FreeCoTaskMem(membersInfo.lgrmi3_domainandname);
        return result;
    }

    /// <summary>
    /// Remove Account from group by name
    /// </summary>
    /// <param name="username">Target username</param>
    /// <param name="group">Group to remove</param>
    /// <returns>0 on success</returns>
    public static int RemoveAccountFromGroup(string username, string group)
    {
        WinUser.LOCALGROUP_MEMBERS_INFO_3 membersInfo = new WinUser.LOCALGROUP_MEMBERS_INFO_3
        {
            lgrmi3_domainandname = Marshal.StringToCoTaskMemUni(username)
        };
        int result = WinUser.NetLocalGroupDelMembers(null, group, 3, ref membersInfo, 1);
        if (result != 0)
        {
            Console.WriteLine("NetLocalGroupRemoveMembers failed with error code: " + result);
        }
        else {
            Console.WriteLine($"User remove from group: {group}");
        }
        Marshal.FreeCoTaskMem(membersInfo.lgrmi3_domainandname);
        return result;
    }

    /// <summary>
    /// Delete useraccount by name
    /// </summary>
    /// <param name="username"></param>
    /// <returns>0 on success</returns>
    public static int DeleteAccount(string username)
    {
        Console.WriteLine("Deleting user...");

        int result = WinUser.NetUserDel(null, username);
        if (result != 0)
        {
            Console.WriteLine("NetUserDel failed with error code: " + result);
            return result;
        }

        Console.WriteLine("User deleted!");
        return 0;
    }

    public static List<string> ListLocalUsers()
    {
        List<string> localUsers = new List<string>();

        // Call NetUserEnum function
        IntPtr buffer;
        int entriesRead, totalEntries;
        int resumeHandle;

        int result = WinUser.NetUserEnum(null, 0, WinUser.FILTER_NORMAL_ACCOUNT, out buffer, -1, out entriesRead, out totalEntries, out resumeHandle);

        if (result == WinUser.NERR_Success)
        {
            // Get the array of USER_INFO_0 structures
            WinUser.USER_INFO_0[] userInfos = new WinUser.USER_INFO_0[entriesRead];
            IntPtr iter = buffer;

            for (int i = 0; i < entriesRead; i++)
            {
                userInfos[i] = (WinUser.USER_INFO_0)Marshal.PtrToStructure(iter, typeof(WinUser.USER_INFO_0));
                iter += Marshal.SizeOf(typeof(WinUser.USER_INFO_0));
            }

            // Add the usernames to the list
            foreach (WinUser.USER_INFO_0 userInfo in userInfos)
            {
                localUsers.Add(userInfo.name);
            }
        }

        // Free the memory allocated by NetUserEnum
        WinUser.NetApiBufferFree(buffer);

        return localUsers;
    }

    public static string GetSidString(string domain, string userName)
    {
        // Initialize variables needed for the LookupAccountName function
        const int ERROR_INSUFFICIENT_BUFFER =  122;
        uint sidLength =  0;
        uint domainNameLength =  0;
        WinUser.SID_NAME_USE sidType;

        // First call to LookupAccountName to get the size needed for the buffer
        if (!WinUser.LookupAccountName(domain, userName, IntPtr.Zero, ref sidLength, null, ref domainNameLength, out sidType))
        {
            int error = Marshal.GetLastWin32Error();
            if (error != ERROR_INSUFFICIENT_BUFFER)
            {
                throw new System.ComponentModel.Win32Exception(error);
            }
        }

        // Allocate memory for the buffers
        IntPtr sidBytesPtr = Marshal.AllocCoTaskMem((int)sidLength);
        StringBuilder domainName = new StringBuilder((int)domainNameLength);

        // Second call to LookupAccountName to retrieve the actual SID and domain name
        if (!WinUser.LookupAccountName(domain, userName, sidBytesPtr, ref sidLength, domainName, ref domainNameLength, out sidType))
        {
            Marshal.FreeCoTaskMem(sidBytesPtr);
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        // Convert the SID bytes to a string representation
        byte[] sidBytes = new byte[sidLength];
        Marshal.Copy(sidBytesPtr, sidBytes,  0, (int)sidLength);
        string sidString = new SecurityIdentifier(sidBytes,  0).ToString();
        Marshal.FreeCoTaskMem(sidBytesPtr);
        return sidString;
    }

    public static int AddProfile(string username)
    {
        string userSid = GetSidString(Environment.MachineName, username);

        // Create a string builder to hold the profile path
        StringBuilder profilePathBuffer = new StringBuilder(260);
        uint bufferSize = (uint)profilePathBuffer.Capacity;
        
        // Call CreateProfile
        int result = WinUser.CreateProfile(userSid, username, profilePathBuffer, bufferSize);
        
        // Check the result
        if (result ==  0)
        {
            // Successfully created profile
            Console.WriteLine($"Profile path for user '{username}' is: {profilePathBuffer}");
        }
        else
        {
            // Failed to create profile
            Console.WriteLine($"Failed to create profile for user '{username}'.");
        }

        return result;
    }

    /// <summary>
    /// Delete profile of a user
    /// </summary>
    /// <param name="username"></param>
    /// <returns>true on success</returns>
    public static bool DeleteProfile(string username)
    {
        string userSid = GetSidString(Environment.MachineName, username);
        bool result = WinUser.DeleteProfileW(userSid, null, null);

        if (result)
        {
            Console.WriteLine($"Profile for user '{username}' deleted");
        }
        else
        {
            Console.WriteLine($"Failed to delete profile for user '{username}'.");
        }
        return result;
    }
}
