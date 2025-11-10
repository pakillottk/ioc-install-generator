using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using IoC.InstallGenerator.Abstractions;

namespace Demo.Host
{
    /// <summary>
    /// Benchmark para el método con Source Generator.
    /// Se ejecuta en un proceso separado para evitar sesgo por ensamblados ya cargados.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net50)]
    public class GeneratorBenchmark
    {
        private SimpleContainerAdapter _container = null!;

        [GlobalSetup]
        public void Setup()
        {
            _container = new SimpleContainerAdapter();
        }

        /// <summary>
        /// Método que usa el código generado por el source generator.
        /// Este método no usa reflexión y todos los tipos son conocidos en tiempo de compilación.
        /// </summary>
        [Benchmark(Baseline = true)]
        public void LoadWithGenerator()
        {
            Loader.LoadAll(_container);
        }
    }

    /// <summary>
    /// Benchmark para el método con Reflexión.
    /// Se ejecuta en un proceso separado para evitar sesgo por ensamblados ya cargados.
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(BenchmarkDotNet.Jobs.RuntimeMoniker.Net50)]
    public class ReflectionBenchmark
    {
        private SimpleContainerAdapter _container = null!;

        [GlobalSetup]
        public void Setup()
        {
            _container = new SimpleContainerAdapter();
        }

        /// <summary>
        /// Método que usa reflexión para descubrir y cargar los installers.
        /// Este es el enfoque tradicional que el source generator reemplaza.
        /// </summary>
        [Benchmark]
        public void LoadWithReflection()
        {
            LoadInstallersWithReflection(_container);
        }

        /// <summary>
        /// Implementación basada en reflexión para comparación.
        /// Este método busca todos los tipos que implementan IIoCInstaller
        /// en los ensamblados referenciados y los instancia usando reflexión.
        /// </summary>
        private static void LoadInstallersWithReflection(IIoCContainer container)
        {
            var errors = new List<Exception>();

            try
            {
                // Obtener el ensamblado actual
                var currentAssembly = Assembly.GetExecutingAssembly();
                
                // Obtener todos los ensamblados referenciados
                var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
                
                // También incluir el ensamblado actual
                var assembliesToCheck = new List<Assembly> { currentAssembly };
                
                foreach (var assemblyName in referencedAssemblies)
                {
                    try
                    {
                        var assembly = Assembly.Load(assemblyName);
                        assembliesToCheck.Add(assembly);
                    }
                    catch
                    {
                        // Ignorar ensamblados que no se pueden cargar
                    }
                }

                // Buscar todos los tipos que implementan IIoCInstaller
                var installerTypes = new List<Type>();
                foreach (var assembly in assembliesToCheck)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => typeof(IIoCInstaller).IsAssignableFrom(t)
                                     && !t.IsAbstract
                                     && !t.IsInterface
                                     && t.IsPublic);

                        installerTypes.AddRange(types);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        // Algunos tipos pueden no cargarse, continuar con los que sí
                        var loadedTypes = ex.Types.Where(t => t != null);
                        var validTypes = loadedTypes
                            .Where(t => typeof(IIoCInstaller).IsAssignableFrom(t)
                                     && !t.IsAbstract
                                     && !t.IsInterface
                                     && t.IsPublic);
                        installerTypes.AddRange(validTypes!);
                    }
                }

                // Ordenar por nombre para mantener consistencia con el orden del generador
                installerTypes = installerTypes
                    .OrderBy(t => t.Assembly.GetName().Name)
                    .ThenBy(t => t.FullName)
                    .ToList();

                // Instanciar y ejecutar cada installer
                foreach (var installerType in installerTypes)
                {
                    try
                    {
                        // Usar Activator.CreateInstance con reflexión
                        var installer = (IIoCInstaller)Activator.CreateInstance(installerType)!;
                        installer.Install(container);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(new InstallerException(installerType.FullName ?? installerType.Name, ex));
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }

            if (errors.Count > 0)
            {
                throw new AggregateException("One or more installers failed", errors);
            }
        }
    }

    /// <summary>
    /// Clase principal para ejecutar los benchmarks.
    /// Ejecuta cada benchmark en procesos separados para evitar sesgo por ensamblados ya cargados.
    /// </summary>
    public static class BenchmarkRunner
    {
        public static void Run()
        {
            Console.WriteLine("==========================================");
            Console.WriteLine("Benchmark: Reflexión vs Source Generator");
            Console.WriteLine("==========================================");
            Console.WriteLine();
            Console.WriteLine("NOTA: Cada benchmark se ejecuta en un proceso separado");
            Console.WriteLine("      para evitar sesgo por ensamblados ya cargados.");
            Console.WriteLine();

            // Ejecutar ambos benchmarks en procesos separados
            // BenchmarkDotNet ejecuta cada clase en su propio proceso por defecto
            var config = BenchmarkDotNet.Configs.DefaultConfig.Instance;
            
            Console.WriteLine("Ejecutando benchmark con Source Generator...");
            var generatorSummary = BenchmarkDotNet.Running.BenchmarkRunner.Run<GeneratorBenchmark>(config);
            
            Console.WriteLine();
            Console.WriteLine("Ejecutando benchmark con Reflexión...");
            var reflectionSummary = BenchmarkDotNet.Running.BenchmarkRunner.Run<ReflectionBenchmark>(config);

            Console.WriteLine();
            Console.WriteLine("==========================================");
            Console.WriteLine("Resumen del Benchmark");
            Console.WriteLine("==========================================");
            Console.WriteLine();
            Console.WriteLine("Cada método se ejecutó en un proceso separado para evitar sesgo.");
            Console.WriteLine();
            Console.WriteLine("Métricas clave:");
            Console.WriteLine("- Mean: Tiempo promedio de ejecución");
            Console.WriteLine("- Error: Margen de error estadístico");
            Console.WriteLine("- StdDev: Desviación estándar");
            Console.WriteLine();
            Console.WriteLine("Compara los resultados de ambos benchmarks para ver la diferencia.");
        }
    }
}

