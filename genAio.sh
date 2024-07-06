#!/bin/sh
cat src/*.cs > aio.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/XCrd.cs > xcrd.cs
cat src/00_GlobalUsings.cs src/01_Interop.cs src/LicenseManager.cs > license.cs
