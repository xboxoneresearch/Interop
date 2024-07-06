# Durango Interop Dotnet

## Requirements

- Powershell Core (x64 binaries)

## General

Use script `genAio.sh` (Linux) or `genAIO.bat` (Windows) to generate self-contained c# code snippets.

Then, on console in powershell, do `Add-Type -Path filename.cs`

## Example usage with powershell

XCrdAPi

```powershell
Add-Type -Path xcrd.cs
$xcrd = [XCrdManager]::new()

# Mount savegame
$xcrd.Unmount("[XTE:]\ConnectedStorage-retail")
$xcrd.Mount("[XTE:]\ConnectedStorage-retail")
# [+] Opened adapter...
# [+] Successfully mounted XVD!
# [+] Mount Path: \\?\GLOBALROOT\Device\Harddisk15\Partition1
cmd /c mklink /j T:\connectedStorage "\\?\GLOBALROOT\Device\Harddisk15\Partition1\"
```

License

```powershell
Add-Type -Path license.cs
[LicenseManager]::LoadLicenseFile("S:\Clip\g-u-i-d")
```

