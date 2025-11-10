using System;

namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Atributo que marca una clase parcial para que el source generator
    /// genere el método LoadAll que carga todos los instaladores.
    /// </summary>
    /// <remarks>
    /// Aplica este atributo a una clase parcial en tu proyecto host.
    /// El generador creará automáticamente el método estático LoadAll.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IoCInstallerLoaderAttribute : Attribute
    {
    }
}

