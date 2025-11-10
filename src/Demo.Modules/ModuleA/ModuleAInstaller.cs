using IoC.InstallGenerator.Abstractions;

namespace Demo.Modules.ModuleA
{
    // T055: ModuleAInstaller for demonstration
    public class ModuleAInstaller : IIoCInstaller
    {
        public void Install(IIoCContainer container)
        {
            // Register ModuleA services
            container.Register<IModuleAService, ModuleAService>();
        }
    }

    public interface IModuleAService
    {
        string GetName();
    }

    public class ModuleAService : IModuleAService
    {
        public string GetName() => "ModuleA";
    }
}

