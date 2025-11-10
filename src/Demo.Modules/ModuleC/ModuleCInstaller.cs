using IoC.InstallGenerator.Abstractions;

namespace Demo.Modules.ModuleC
{
    // T057: ModuleCInstaller for demonstration
    public class ModuleCInstaller : IIoCInstaller
    {
        public void Install(IIoCContainer container)
        {
            // Register ModuleC services
            container.Register<IModuleCService, ModuleCService>();
        }
    }

    public interface IModuleCService
    {
        string GetName();
    }

    public class ModuleCService : IModuleCService
    {
        public string GetName() => "ModuleC";
    }
}

