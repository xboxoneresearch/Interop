[Guid("CC31D32A-A46C-4EB0-A395-66B6D8AB1CC4")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IAppPackageMountManager
{
    void MountForFileIO(string packageFullName, uint target, int type);
    void UnmountForFileIO(string packageFullName, uint target);
    string MountTitleDeveloperScratch();
    void UnmountTitleDeveloperScratch();
    void PrepareForActivation(string packageFullName, string psmKey, int packageType);
    void CleanupAfterDeactivation(string packageFullName, string psmKey);
    void CleanupAfterUninstall(string packageFullName);
    void GetPathForTarget(string packageFullName, uint target, [MarshalAs(UnmanagedType.LPWStr)] out string path);
}

[Guid("b54e912f-c1f4-470d-b841-c59db24be3b7")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IAppPackageMountManagerInternal
{
    bool IsXCrdMountable(string xcrd);
    void UnmountForUninstall(string packageFullName);
    void AttachMount(string packageFullName, string crdPath, int type);
}

public class AppPackageMountManager
{
    static readonly Guid CLSID = new Guid("8D2C2449-D1F1-4EB5-835A-D0DD0E5CAD36");
    static readonly IAppPackageMountManager _manager;
    static readonly IAppPackageMountManagerInternal _managerInternal;

    static AppPackageMountManager()
    {
        COM.CoInitializeEx(IntPtr.Zero, 0);

        _manager = COM.ActivateClass<IAppPackageMountManager>(CLSID);
        _managerInternal = COM.ActivateClass<IAppPackageMountManagerInternal>(CLSID);
    }

    public string GetMountPath(string packageFullName)
    {
        _manager.GetPathForTarget(packageFullName, 0x00000001, out string mountPath);
        if (string.IsNullOrEmpty(mountPath))
        {
            throw new InvalidDataException("Invalid mount path!");
        }
        return mountPath;
    }

    public static void AttachMount(string packageFullName, string crdPath)
    {
        _managerInternal.AttachMount(packageFullName, crdPath, 1);
    }

    public static void MountPackage(string packageFullName)
    {
        _manager.MountForFileIO(packageFullName, 0x00000001, 1);
    }

    public static void UnmountPackage(string packageFullName)
    {
        _manager.UnmountForFileIO(packageFullName, 0x00000001);
    }
}
