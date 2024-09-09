#!/bin/sh
cat src/*.cs > aio.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/XCrd.cs > xcrd.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/LicenseManager.cs > license.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/UserManager.cs > usermanager.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/AppPackageMountManager.cs > packagemount.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/WinUserManager.cs > winuser.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/Shellcode.cs > sc.cs
cat src/modules/xcrdutil.psm1 > xcrdutil.psm1
