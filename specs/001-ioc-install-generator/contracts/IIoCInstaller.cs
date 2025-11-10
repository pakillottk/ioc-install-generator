namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Interfaz común que deben implementar todos los módulos que desean registrar
    /// dependencias en el contenedor IoC.
    /// </summary>
    public interface IIoCInstaller
    {
        /// <summary>
        /// Registra las dependencias del módulo en el contenedor IoC proporcionado.
        /// </summary>
        /// <param name="container">Contenedor IoC donde registrar las dependencias.</param>
        void Install(IIoCContainer container);
    }
}

