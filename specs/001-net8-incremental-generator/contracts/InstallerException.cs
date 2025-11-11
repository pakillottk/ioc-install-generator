using System;

namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Excepción que encapsula un error ocurrido durante la ejecución de un instalador.
    /// </summary>
    public class InstallerException : Exception
    {
        /// <summary>
        /// Obtiene el nombre del instalador que falló.
        /// </summary>
        public string InstallerName { get; }
        
        /// <summary>
        /// Inicializa una nueva instancia de la excepción.
        /// </summary>
        /// <param name="installerName">Nombre del instalador que falló.</param>
        /// <param name="innerException">Excepción original que causó el fallo.</param>
        public InstallerException(string installerName, Exception innerException)
            : base($"Installer '{installerName}' failed during execution.", innerException)
        {
            InstallerName = installerName ?? throw new ArgumentNullException(nameof(installerName));
        }
    }
}

