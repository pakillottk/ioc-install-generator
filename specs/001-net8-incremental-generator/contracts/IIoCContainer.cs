namespace IoC.InstallGenerator.Abstractions
{
    /// <summary>
    /// Abstracción genérica para contenedores IoC que permite compatibilidad
    /// con múltiples implementaciones.
    /// </summary>
    public interface IIoCContainer
    {
        /// <summary>
        /// Registra un tipo de servicio con su implementación.
        /// </summary>
        /// <typeparam name="TService">Tipo del servicio.</typeparam>
        /// <typeparam name="TImplementation">Tipo de la implementación.</typeparam>
        void Register<TService, TImplementation>() 
            where TImplementation : TService;
        
        /// <summary>
        /// Registra una instancia singleton del servicio.
        /// </summary>
        /// <typeparam name="TService">Tipo del servicio.</typeparam>
        /// <param name="instance">Instancia a registrar.</param>
        void Register<TService>(TService instance);
        
        /// <summary>
        /// Registra un factory para crear instancias del servicio.
        /// </summary>
        /// <typeparam name="TService">Tipo del servicio.</typeparam>
        /// <param name="factory">Factory que crea instancias del servicio.</param>
        void Register<TService>(Func<IIoCContainer, TService> factory);
    }
}

