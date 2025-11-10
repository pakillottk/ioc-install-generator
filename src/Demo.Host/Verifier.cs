using System;
using IoC.InstallGenerator.Abstractions;

namespace Demo.Host
{
    public static partial class Verifier
    {        
        public static void Verify(IIoCContainer container)
        {
            // Verify that services were registered
            Console.WriteLine("Verifying registered services...");
            var moduleA = container.Resolve<Demo.Modules.ModuleA.IModuleAService>();
            var moduleB = container.Resolve<Demo.Modules.ModuleB.IModuleBService>();
            var moduleC = container.Resolve<Demo.Modules.ModuleC.IModuleCService>();

            Console.WriteLine($"ModuleA: {moduleA.GetName()}");
            Console.WriteLine($"ModuleB: {moduleB.GetName()}");
            Console.WriteLine($"ModuleC: {moduleC.GetName()}");
            Console.WriteLine();

            Console.WriteLine("Services verified successfully!");
        }
    }
}
