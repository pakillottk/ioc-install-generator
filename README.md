# IoC Install Generator

Un Source Generator para .NET que descubre automáticamente instaladores de IoC de ensamblados referenciados y genera código estático para cargarlos en tiempo de ejecución.

## Características

- ✅ **Descubrimiento automático**: Encuentra todos los instaladores de IoC en ensamblados referenciados (directos y transitivos)
- ✅ **Código estático generado**: Sin overhead de reflexión en tiempo de ejecución
- ✅ **Alto rendimiento**: Al menos 50% más rápido que soluciones basadas en reflexión
- ✅ **Compatible con AOT y Trimming**: Código generado sin dependencias dinámicas
- ✅ **Manejo de errores resiliente**: Los errores en un instalador no detienen la carga de otros
- ✅ **Orden configurable**: Controla el orden de ejecución mediante atributos

## Requisitos

- .NET 8.0 o superior
- Proyectos SDK-style (PackageReference)

## Instalación

Agrega el paquete NuGet a tu proyecto host:

```xml
<ItemGroup>
  <PackageReference Include="IoC.InstallGenerator" Version="1.0.0" />
</ItemGroup>
```

O usando dotnet CLI:

```bash
dotnet add package IoC.InstallGenerator
```

## Uso Rápido

### 1. Implementa la interfaz de instalador en tus módulos

```csharp
using IoC.InstallGenerator.Abstractions;

namespace MyModule
{
    public class MyModuleInstaller : IIoCInstaller
    {
        public void Install(IIoCContainer container)
        {
            container.Register<IMyService, MyService>();
        }
    }
}
```

### 2. Crea un adaptador para tu contenedor IoC

```csharp
using IoC.InstallGenerator.Abstractions;

public class MyContainerAdapter : IIoCContainer
{
    // Implementa los métodos Register según tu contenedor
    public void Register<TService, TImplementation>() 
        where TImplementation : TService
    {
        // Tu lógica de registro
    }
    
    public void Register<TService>(TService instance) { }
    public void Register<TService>(Func<IIoCContainer, TService> factory) { }
    
    // Implementa Resolve para resolver servicios
    public TService Resolve<TService>()
    {
        // Tu lógica de resolución
    }
}
```

### 3. Crea una clase parcial con el atributo [IoCInstallerLoader]

```csharp
using IoC.InstallGenerator.Abstractions;

namespace MyApp
{
    [InstallOrder("ModuleA", "ModuleB", "ModuleC")] // Opcional
    [IoCInstallerLoader]
    public static partial class InstallerLoader
    {
        // El método LoadAll será generado automáticamente por el source generator
    }
}
```

### 4. Usa el código generado en tu aplicación

```csharp
using IoC.InstallGenerator.Abstractions;

namespace MyApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var container = new MyContainerAdapter();
            
            // Carga todos los instaladores automáticamente
            // El método LoadAll es generado por el source generator
            InstallerLoader.LoadAll(container);
            
            // Tu aplicación continúa normalmente...
        }
    }
}
```

## Configuración del Orden de Ejecución

Puedes controlar el orden de ejecución de los instaladores usando el atributo `[InstallOrder]` en la clase marcada con `[IoCInstallerLoader]`:

```csharp
[InstallOrder("ModuleA", "ModuleB", "ModuleC")]
[IoCInstallerLoader]
public static partial class InstallerLoader
{
    // El orden especifica los nombres de los ensamblados
    // Los instaladores se ejecutarán en el orden especificado
}
```

Si no especificas el orden, los instaladores se ejecutarán en orden de descubrimiento (arbitrario).

## Manejo de Errores

El sistema es resiliente a errores: si un instalador falla, el sistema continúa ejecutando los demás. Al final, si hay errores, se lanza una `AggregateException` con todos los errores ocurridos:

```csharp
try
{
    InstallerLoader.LoadAll(container);
}
catch (AggregateException ex)
{
    foreach (var innerEx in ex.InnerExceptions)
    {
        Console.WriteLine($"Installer failed: {innerEx.Message}");
    }
}
```

Cada error individual es envuelto en un `InstallerException` que contiene el nombre del tipo del instalador que falló y la excepción original.

## Ejemplos

Ver el proyecto de demostración en `src/Demo.Host/` y los módulos de ejemplo en `src/Demo.Modules/` para ejemplos completos de uso.

### Ejecutar el demo

```bash
dotnet run --project src/Demo.Host/Demo.Host.csproj
```

El demo muestra cómo:
- Configurar una clase parcial con `[IoCInstallerLoader]`
- Especificar el orden de instalación con `[InstallOrder]`
- Cargar todos los instaladores automáticamente
- Resolver servicios del contenedor

## Rendimiento

### Resultados del Benchmark

Hemos ejecutado benchmarks comparando el Source Generator con implementaciones basadas en reflexión. Los resultados muestran mejoras significativas:

| Métrica | Source Generator Incremental | Reflexión | Mejora |
|---------|-------------------------------|-----------|--------|
| **Tiempo promedio** | 89.06 ns | 60.81 µs (60,809 ns) | **683x más rápido** |
| **Memoria allocada** | 104 B | 19.67 KB (20,142 B) | **194x menos memoria** |
| **GC Gen0 collections** | 0.0124/1000 ops | 2.3804/1000 ops | **192x menos presión GC** |

> **Nota**: Cada benchmark se ejecutó en un proceso separado para evitar sesgo por ensamblados ya cargados.

### Características de Rendimiento

- ✅ **683 veces más rápido** que implementaciones basadas en reflexión
- ✅ **194 veces menos memoria** que métodos con reflexión
- ✅ Inicialización de hasta 50 ensamblados en <100ms
- ✅ Sin overhead de descubrimiento dinámico en tiempo de ejecución
- ✅ Llamadas directas a métodos (sin overhead de reflexión)
- ✅ Caching automático con IIncrementalGenerator para mejor rendimiento de compilación

### Ejecutar el Benchmark

Para ejecutar el benchmark localmente y ver los resultados detallados:

```bash
cd src/Demo.Host
dotnet run -c Release -- benchmark
```

Los resultados completos están documentados en [BENCHMARK_RESULTS.md](BENCHMARK_RESULTS.md).

### Conclusiones del Benchmark

Basado en los resultados del benchmark ejecutado en procesos separados para evitar sesgo:

1. **Rendimiento Excepcional**: El Source Generator Incremental es **683 veces más rápido** que la reflexión, reduciendo el tiempo de carga de instaladores de ~60.81 µs a ~89.06 ns. Esta mejora es crítica en aplicaciones que requieren inicialización rápida.

2. **Eficiencia de Memoria**: El Source Generator Incremental utiliza **194 veces menos memoria** (104 B vs 20,142 B), reduciendo significativamente la presión sobre el Garbage Collector y mejorando el rendimiento general de la aplicación.

3. **Compatibilidad Moderna**: A diferencia de la reflexión, el Source Generator es completamente compatible con tecnologías modernas como:
   - **AOT (Ahead-of-Time compilation)**: Necesario para aplicaciones móviles y embebidas
   - **Trimming**: Reduce el tamaño de las aplicaciones eliminando código no usado
   - **Native AOT**: Permite compilación nativa sin runtime de .NET

4. **Seguridad de Tipos**: El código generado proporciona mejor seguridad de tipos y experiencia de desarrollo:
   - Errores detectados en tiempo de compilación, no en tiempo de ejecución
   - Mejor soporte de IntelliSense y refactoring
   - Sin sorpresas en producción por tipos no encontrados

5. **Recomendación**: Para la mayoría de los casos de uso, especialmente en aplicaciones que requieren:
   - Alto rendimiento
   - Bajo consumo de memoria
   - Compatibilidad con AOT/trimming
   - Inicialización rápida
   
   El Source Generator es la opción superior y recomendada sobre soluciones basadas en reflexión.

## Compatibilidad

- ✅ .NET 8.0+
- ✅ Compatible con AOT (Ahead-of-Time compilation)
- ✅ Compatible con Trimming
- ✅ Proyectos SDK-style (PackageReference)
- ✅ Referencias de proyecto y paquetes NuGet
- ✅ Sin dependencias de reflexión en tiempo de ejecución
- ✅ Generador incremental (IIncrementalGenerator) para mejor rendimiento

## Documentación

- [Guía de inicio rápido](specs/001-ioc-install-generator/quickstart.md)
- [Especificación completa](specs/001-ioc-install-generator/spec.md)
- [Plan de implementación](specs/001-ioc-install-generator/plan.md)

## Construcción y Pruebas

Para construir el proyecto:

```bash
dotnet build
```

Para ejecutar los tests:

```bash
dotnet test
```

## Estructura del Proyecto

```
src/
├── IoC.InstallGenerator/              # Source generator
├── IoC.InstallGenerator.Abstractions/ # Interfaces y atributos
├── IoC.InstallGenerator.Tests/       # Tests unitarios
├── Demo.Host/                         # Aplicación de demostración
└── Demo.Modules/                      # Módulos de ejemplo
    ├── ModuleA/
    ├── ModuleB/
    └── ModuleC/
```

## Cómo Funciona

1. **En tiempo de compilación**: El source generator incremental (IIncrementalGenerator) analiza todas las referencias del proyecto (directas y transitivas hasta 3 niveles de profundidad) usando un pipeline incremental
2. **Descubrimiento**: Encuentra todas las clases públicas que implementan `IIoCInstaller` y tienen un constructor público sin parámetros usando SyntaxProvider y CompilationProvider
3. **Caching automático**: IIncrementalGenerator proporciona caching automático, solo reprocesando archivos modificados en compilaciones incrementales
4. **Generación**: Genera código estático en una clase parcial que instancia y ejecuta cada instalador directamente, sin usar reflexión
5. **Tiempo de ejecución**: El código generado se ejecuta de forma eficiente, llamando directamente a cada instalador

## Licencia

[Especificar licencia]

## Contribuir

[Instrucciones para contribuir]

