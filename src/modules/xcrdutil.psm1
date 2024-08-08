# Usage information text
$global:usageText = @"
Usage  : xcrdutil <operation> [parameters/options]

Operations & parameters:
    -h - Usage information
    -m <xvd file> - mounts a xvd file as a virtual drive
    -um <xvd file> - unmounts a xvd drive for the specified xvd file
    -del <xvd file> - deletes a xvd file using a XCRD path
    -enum <xcrd_id> <enum_rel_path> <flags> - enumerate blobs. path = * - enum root; flags = 1 - detect blob.

    XCRD ID List:
    HDD Temp              : 0
    HDD User Content      : 1
    HDD System Support    : 2
    HDD System Image      : 3
    HDD Future Growth     : 5
    XTF Remote Storage    : 7
    XBL XBox Live Storage : 8
    XODD (System VM)      : 9
    USB Transfer Storage  : 11 - 18
    USB External Storage  : 19 - 26
    XBOX (Copy-Over-Lan)  : 27
    SRA (local or SMB)    : 28
"@

# Function to display usage information
function Show-Usage {
    Write-Host $global:usageText
}

# Main function
function xcrdutil {
    [CmdletBinding(DefaultParameterSetName="Help")]
    param (
        [Parameter(ParameterSetName="Help")]
        [switch]$h,

        [Parameter(ParameterSetName="Mount")]
        [string]$m,

        [Parameter(ParameterSetName="Unmount")]
        [string]$um,

        [Parameter(ParameterSetName="Delete")]
        [string]$del,

        [Parameter(ParameterSetName="Enum")]
        [switch]$enum,

        [Parameter(ParameterSetName="Enum", Position=1)]
        [string]$xcrd_id,

        [Parameter(ParameterSetName="Enum", Position=2)]
        [string]$enum_rel_path,

        [Parameter(ParameterSetName="Enum", Position=3)]
        [string]$flags
    )

    # Check if no parameters are provided
    if (-not $PSCmdlet.MyInvocation.BoundParameters.Count) {
        Show-Usage
        return
    }

    if ($h) {
        Show-Usage
        return
    }

    Add-Type -Path .\xcrd.cs
    $xcrd = [XCrdManager]::new()

    if ($m) {
        Write-Host "-m: $m"
        $xcrd.Mount($m)
    }
    elseif ($um) {
        Write-Host "-um: $um"
        $xcrd.Unmount($um)
    }
    elseif ($del) {
        Write-Host "-del: $del"
        $xcrd.DeleteXVD($del)
    }
    elseif ($enum) {
        $xcrdIdUint = [uint32]::Parse($xcrd_id)
        $flagsUint = [uint32]::Parse($flags)
        Write-Host "-enum: $xcrdIdUint $enum_rel_path $flagsUint"
        $xcrd.EnumBlobs($xcrdIdUint, $enum_rel_path, $flagsUint)
    }
}

Export-ModuleMember -Function xcrdutil
