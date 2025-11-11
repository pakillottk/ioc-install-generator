using System;

namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Atributo que marca una clase parcial donde el generador debe generar
    /// el método LoadAll para cargar instaladores.
    /// </summary>
    /// <remarks>
    /// Este atributo debe aplicarse a una clase parcial pública o interna.
    /// El generador incremental buscará clases con este atributo y generará
    /// un método estático LoadAll en la clase parcial.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IoCInstallerLoaderAttribute : Attribute
    {
    }
}

