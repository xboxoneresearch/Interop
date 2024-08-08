@{
    # Script module or binary module file associated with this manifest
    RootModule = 'xcrdutil.psm1'

    # Version number of this module.
    ModuleVersion = '1.0.0'

    # Supported PSEditions
    CompatiblePSEditions = @('Desktop', 'Core', 'Core-7.2.3')

    # ID used to uniquely identify this module
    GUID = 'c56a4180-65aa-42ec-a945-5fd21dec0538'

    # Author of this module
    Author = 'Shadow LAG'

    # Company or vendor of this module
    CompanyName = 'XOS Team'

    # Description of the functionality provided by this module
    Description = 'Utility module for XCRD operations'

    # Minimum version of the Windows PowerShell engine required by this module
    PowerShellVersion = '5.1'

    # Modules that must be imported into the global environment prior to importing this module
    RequiredModules = @()

    # Assemblies that must be loaded prior to importing this module
    RequiredAssemblies = @('.\xcrd.dll')

    # Script files (.ps1) that are run in the caller's environment prior to importing this module
    ScriptsToProcess = @()

    # Type files (.ps1xml) to be loaded when importing this module
    TypesToProcess = @()

    # Format files (.ps1xml) to be loaded when importing this module
    FormatsToProcess = @()

    # Modules to import as nested modules of the module specified in RootModule/ModuleToProcess
    NestedModules = @()

    # Functions to export from this module
    FunctionsToExport = @('xcrdutil')

    # Cmdlets to export from this module
    CmdletsToExport = @()

    # Variables to export from this module
    VariablesToExport = @()

    # Aliases to export from this module
    AliasesToExport = @()

    # List of all modules packaged with this module
    FileList = @('xcrdutil.psm1', 'xcrd.dll')

    # Private data to pass to the module specified in RootModule/ModuleToProcess
    PrivateData = @{

    }

    # HelpInfo URI of this module
    HelpInfoURI = ''

    # Default prefix for commands exported from this module. Override the default prefix using Import-Module -Prefix
    DefaultCommandPrefix = 'xcrd'
}
