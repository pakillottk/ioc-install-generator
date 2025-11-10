using IoC.InstallGenerator.Abstractions;

namespace Demo.Modules.ModuleB
{
    // T056: ModuleBInstaller for demonstration
    public class ModuleBInstaller : IIoCInstaller
    {
        public void Install(IIoCContainer container)
        {
            // Register ModuleB services
            container.Register<IModuleBService, ModuleBService>();
        }
    }

    public interface IModuleBService
    {
        string GetName();
    }

    public class ModuleBService : IModuleBService
    {
        public string GetName() => "ModuleB";
    }
}

