[![CI](https://github.com/xboxoneresearch/Interop/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/xboxoneresearch/Interop/actions/workflows/build.yml)
![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/xboxoneresearch/Interop/latest/total)
[![GitHub latest Tag](https://img.shields.io/github/v/tag/xboxoneresearch/Interop)](https://github.com/xboxoneresearch/Interop/releases/latest)

# Durango Interop Dotnet

This repo serves two purposes

- Full C# classes to interop
- MSBuild tasks that can be ran via dotnet

[Download latest binaries](https://github.com/xboxoneresearch/Interop/releases/latest)

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

Build this package (via msbuild) to generate self-contained C# code snippets in the build outputs `pwsh` directory + compiled .NET assembly.

There are two ways to utilize it, by source code or by loading the .NET assembly (from bytes!).

~~NOTE: This currently does not work on 2024 Retail OS exploited consoles.~~

For OS ~2024 use SharpShell, linked above!

#### Example usage by loading the compiled .NET Assembly

The following snippet is executed via Powershell.
Given `DurangoInteropDotnet.dll` is located in the root of Volume `D:\`.

```powershell
$bytes = [System.IO.File]::ReadAllBytes("D:\\DurangoInteropDotnet.dll")
$assembly = [System.Reflection.Assembly]::Load($bytes)

[LicenseManager]::LoadLicenseFile("O:\\Licenses\\License0.xml")
```

#### Example usage with powershell by loading CSharp source files

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

# Read VBI
$xcrd.ReadVbi("R:\A\system.xvd", "T:\system.vbi")
# Alternatively, try these paths:
# $xcrd.ReadVbi("R:\B\system.xvd", "T:\system.vbi")
# $xcrd.ReadVbi("F:\system.xvd", "T:\system.vbi")
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

