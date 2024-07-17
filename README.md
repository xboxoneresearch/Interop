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

#### General

Use script `genAio.sh` (Linux) or `genAIO.bat` (Windows) to generate self-contained c# code snippets.

Then, on console in powershell, do `Add-Type -Path filename.cs`

**NOTE**: This currently does not work on 2024 Retail OS exploited consoles.

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

