[Guid("2769c3a8-d8e3-41ba-b38b-01d05dd2374e")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public interface IClipLicenseManager
{
    void StoreLicense(string licenseBuffer, uint flags, out IntPtr clipLicense);
}

public class LicenseManager
{
    static readonly Guid CLSID = new Guid("7e480b22-b679-4542-9b30-d5a52da92ce5");
    static readonly IClipLicenseManager _manager;

    static LicenseManager()
    {
        COM.CoInitializeEx(IntPtr.Zero, 0);

        _manager = COM.ActivateClass<IClipLicenseManager>(CLSID);
    }

    /// <summary>
    /// Loads a full license (base64 representation of the license.xml)
    /// </summary>
    /// <param name="licenseBuffer"></param>
    /// <returns>true on success, false otherwise</returns>
    public static bool LoadLicenseBase64(string licenseBuffer)
    {
        _manager.StoreLicense(licenseBuffer, 0x1, out IntPtr license);
        return license != IntPtr.Zero;
    }

    public static bool LoadLicenseFile(string filePath)
    {
        //
        // Read and convert license contents to base64
        // Just assume the license is valid, it'll fail anyway if it's not
        //
        string content = File.ReadAllText(filePath);
        if (string.IsNullOrEmpty(content))
        {
            throw new InvalidDataException("Provided file is empty");
        }

        var licenseBlob = Encoding.UTF8.GetBytes(content);
        var base64Blob = Convert.ToBase64String(licenseBlob);
        return LoadLicenseBase64(base64Blob);
    }
}
