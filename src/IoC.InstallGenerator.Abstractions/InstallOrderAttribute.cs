using System;

namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Atributo para configurar el orden de ejecución de instaladores por ensamblado
    /// en la aplicación host.
    /// </summary>
    /// <remarks>
    /// Este atributo debe aplicarse a la clase principal de la aplicación host.
    /// Si no se especifica, los instaladores se ejecutarán en orden de descubrimiento (arbitrario).
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false)]
    public class InstallOrderAttribute : Attribute
    {
        /// <summary>
        /// Obtiene los nombres de los ensamblados en el orden deseado de ejecución.
        /// </summary>
        public string[] AssemblyNames { get; }
        
        /// <summary>
        /// Inicializa una nueva instancia del atributo con los nombres de ensamblados.
        /// </summary>
        /// <param name="assemblyNames">Nombres de los ensamblados en el orden deseado.</param>
        /// <exception cref="ArgumentNullException">Si assemblyNames es null.</exception>
        public InstallOrderAttribute(params string[] assemblyNames)
        {
            AssemblyNames = assemblyNames ?? throw new ArgumentNullException(nameof(assemblyNames));
        }
    }
}

