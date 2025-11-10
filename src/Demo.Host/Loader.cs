using System;
using IoC.InstallGenerator.Abstractions;

namespace Demo.Host
{
    // T061: Add InstallOrderAttribute example
    [InstallOrder("ModuleA", "ModuleB", "ModuleC")]
    // Mark this class for source generator - it will generate the LoadAll method in a partial class
    [IoCInstallerLoader]
    public static partial class Loader
    {
    }
}
