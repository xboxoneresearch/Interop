# Durango Interop Dotnet

This repo serves two purposes

- Full C# classes to interop
- MSBuild tasks that can be ran via dotnet


### Dotnet MSBuild tasks

#### Requirements

- [Dotnet SDK, x64, Windows, zip](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

#### Usage

- Extract dotnet sdk zip to flash drive
- Copy the tasks to a flash drive
- Execute them via `dotnet.exe msbuild taskname.xml`

**NOTE**: Some scripts require modifications before being used, f.e. to adjust paths or addresses.

#### Running in background / as another user

As SYSTEM

```
schtasks /create /f /tn "MySYSTEMBackgroundTask" /ru SYSTEM /sc ONSTART /tr "D:\dotnet\dotnet.exe msbuild D:\msbuild_tasks\taskname.xml"
schtasks /run /tn "MySYSTEMBackgroundTask"
```

As DefaultAccount

```
schtasks /create /f /tn "MyDABackgroundTask" /ru DefaultAccount /sc ONSTART /tr "D:\dotnet\dotnet.exe msbuild D:\msbuild_tasks\taskname.xml"
schtasks /run /tn "MyDABackgroundTask"
```

### C# Interop

#### Requirement

- Powershell Core (x64 binaries)

OR

- [SharpShell](https://github.com/xboxoneresearch/SharpShell)

#### General

Build this package (via msbuild) to generate self-contained C# code snippets in the build outputs `pwsh` directory.

Then, on console in powershell, do `Add-Type -Path filename.cs`

~~NOTE: This currently does not work on 2024 Retail OS exploited consoles.~~

For OS ~2024 use SharpShell, linked above!

## Example usage with powershell

Important: Do the "compiling" genAio step first, to yield the standalone/ready to use *.cs files!

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

XcrdUtil
```
#Powershell
import-module xcrdutil.psm1
xcrdutil -m [XUC:]\connectedStorage-retail
```

License

```powershell
Add-Type -Path license.cs
[LicenseManager]::LoadLicenseFile("S:\Clip\g-u-i-d")
```

