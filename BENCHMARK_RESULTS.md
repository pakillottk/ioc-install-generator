# Benchmark: Reflexión vs Source Generator

## Resumen Ejecutivo

Este documento presenta los resultados del benchmark comparando el rendimiento entre el método basado en **reflexión** y el método generado por el **Source Generator** para cargar instaladores de IoC.

### Resultados Principales

| Método | Tiempo Promedio | Ratio | Memoria Allocada | Ratio Memoria |
|--------|----------------|------|------------------|---------------|
| **Source Generator** | **89.35 ns** | **1.00** (baseline) | **104 B** | **1.00** (baseline) |
| **Reflexión** | **79,934 ns** (79.93 µs) | **895.5x** | **19,630 B** | **188.75x** |

**Conclusión**: El método con Source Generator es aproximadamente **896 veces más rápido** y utiliza **189 veces menos memoria** que el método basado en reflexión.

> **Nota importante**: Cada benchmark se ejecutó en un proceso separado para evitar sesgo por ensamblados ya cargados en el AppDomain. Esto asegura que los resultados sean precisos y representativos del rendimiento real en condiciones de uso normales.

---

## Detalles del Benchmark

### Configuración

- **Runtime**: .NET 5.0.1 (5.0.120.57516)
- **JIT**: X64 RyuJIT AVX2
- **GC**: Concurrent Workstation
- **Hardware**: AMD Ryzen 5 3600, 1 CPU, 12 logical cores
- **OS**: Windows 10 (10.0.19045.6456/22H2/2022Update)
- **Herramienta**: BenchmarkDotNet v0.13.5

### Métodos Comparados

#### 1. LoadWithGenerator (Baseline)

Método que utiliza el código generado por el Source Generator en tiempo de compilación.

**Características**:
- ✅ Sin reflexión en tiempo de ejecución
- ✅ Todos los tipos conocidos en tiempo de compilación
- ✅ Compatible con AOT (Ahead-of-Time compilation)
- ✅ Compatible con trimming
- ✅ Llamadas directas a métodos (sin overhead de reflexión)

**Código generado**:
```csharp
public static void LoadAll(IIoCContainer container)
{
    var errors = new List<Exception>();
    
    try
    {
        new global::Demo.Modules.ModuleA.ModuleAInstaller().Install(container);
    }
    catch (Exception ex)
    {
        errors.Add(new InstallerException("...", ex));
    }
    // ... más installers ...
}
```

#### 2. LoadWithReflection

Método tradicional que utiliza reflexión para descubrir y cargar los instaladores.

**Características**:
- ❌ Usa reflexión en tiempo de ejecución
- ❌ Descubrimiento dinámico de tipos
- ❌ No compatible con AOT
- ❌ No compatible con trimming
- ❌ Overhead significativo de reflexión

**Implementación**:
```csharp
private void LoadInstallersWithReflection(IIoCContainer container)
{
    var currentAssembly = Assembly.GetExecutingAssembly();
    var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
    
    foreach (var assemblyName in referencedAssemblies)
    {
        var assembly = Assembly.Load(assemblyName);
        var installerTypes = assembly.GetTypes()
            .Where(t => typeof(IIoCInstaller).IsAssignableFrom(t)
                     && !t.IsAbstract
                     && !t.IsInterface
                     && t.IsPublic);
        
        foreach (var installerType in installerTypes)
        {
            var installer = (IIoCInstaller)Activator.CreateInstance(installerType)!;
            installer.Install(container);
        }
    }
}
```

---

## Resultados Detallados

### Métricas de Tiempo

#### Source Generator (LoadWithGenerator)
- **Mean**: 89.35 ns
- **Error**: ±0.673 ns (0.75%)
- **StdDev**: 0.562 ns
- **Min**: 88.712 ns
- **Max**: 90.504 ns
- **Median**: 89.154 ns
- **Confidence Interval (99.9%)**: [88.676 ns; 90.021 ns]

#### Reflexión (LoadWithReflection)
- **Mean**: 79,934 ns (79.93 µs)
- **Error**: ±1.165 µs (1.46%)
- **StdDev**: 1.033 µs
- **Min**: 79.094 µs
- **Max**: 82.127 µs
- **Median**: 79.445 µs
- **Confidence Interval (99.9%)**: [78.769 µs; 81.099 µs]

### Métricas de Memoria

#### Source Generator (LoadWithGenerator)
- **Allocated**: 104 B por operación
- **Gen0 Collections**: 0.0124 por 1000 operaciones

#### Reflexión (LoadWithReflection)
- **Allocated**: 19,630 B (19.17 KB) por operación
- **Gen0 Collections**: 2.3193 por 1000 operaciones
- **Ratio**: 188.75x más memoria que el generador

---

## Análisis de Rendimiento

### Ventajas del Source Generator

1. **Velocidad**: 896x más rápido
   - Eliminación completa del overhead de reflexión
   - Llamadas directas a métodos conocidos en tiempo de compilación
   - Sin necesidad de descubrir tipos en tiempo de ejecución

2. **Memoria**: 189x menos memoria
   - No necesita cargar metadatos de tipos
   - No necesita crear objetos de reflexión (Type, MethodInfo, etc.)
   - Menos presión sobre el Garbage Collector

3. **Compatibilidad**:
   - ✅ Compatible con AOT (Ahead-of-Time compilation)
   - ✅ Compatible con trimming (eliminación de código no usado)
   - ✅ Mejor para aplicaciones móviles y embebidas

4. **Seguridad de Tipos**:
   - Errores detectados en tiempo de compilación
   - Mejor experiencia de desarrollo con IntelliSense
   - Refactoring más seguro

### Desventajas del Método con Reflexión

1. **Rendimiento**: Significativamente más lento
   - Overhead de reflexión en cada ejecución
   - Descubrimiento de tipos en tiempo de ejecución
   - Creación dinámica de instancias

2. **Memoria**: Mayor consumo
   - Metadatos de tipos cargados en memoria
   - Objetos de reflexión mantenidos en memoria
   - Mayor presión sobre el GC

3. **Compatibilidad**:
   - ❌ No compatible con AOT
   - ❌ No compatible con trimming
   - ❌ Problemas en aplicaciones móviles y embebidas

---

## Escenarios de Uso

### Cuándo Usar Source Generator

✅ **Recomendado para**:
- Aplicaciones que requieren máximo rendimiento
- Aplicaciones móviles (iOS, Android)
- Aplicaciones embebidas
- Aplicaciones que usan AOT compilation
- Aplicaciones que requieren trimming
- Inicialización frecuente de instaladores

### Cuándo Considerar Reflexión

⚠️ **Considerar solo si**:
- Necesitas descubrir tipos dinámicamente en tiempo de ejecución
- Los instaladores se agregan sin recompilar
- El rendimiento de inicialización no es crítico
- No usas AOT o trimming

---

## Conclusiones

1. **Rendimiento**: El Source Generator es **896 veces más rápido** que la reflexión, reduciendo el tiempo de carga de instaladores de ~79.93 µs a ~89.35 ns.

2. **Memoria**: El Source Generator utiliza **189 veces menos memoria**, reduciendo la presión sobre el Garbage Collector.

3. **Compatibilidad**: El Source Generator es compatible con tecnologías modernas como AOT y trimming, mientras que la reflexión no lo es.

4. **Desarrollo**: El Source Generator proporciona mejor seguridad de tipos y experiencia de desarrollo, con errores detectados en tiempo de compilación.

5. **Recomendación**: Para la mayoría de los casos de uso, el Source Generator es la opción superior, especialmente en aplicaciones que requieren alto rendimiento o compatibilidad con AOT/trimming.

---

## Ejecutar el Benchmark

Para ejecutar el benchmark localmente:

```bash
cd src/Demo.Host
dotnet run -c Release -- benchmark
```

Los resultados se guardarán en:
- `BenchmarkDotNet.Artifacts/results/Demo.Host.InstallerLoadingBenchmark-report.html`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.InstallerLoadingBenchmark-report.csv`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.InstallerLoadingBenchmark-report-github.md`

---

## Referencias

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Source Generators in C#](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [AOT Compilation in .NET](https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Trimming in .NET](https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)

---

**Fecha del Benchmark**: 2025-11-10  
**Versión del Generador**: 1.0.0  
**Número de Instaladores**: 3 (ModuleA, ModuleB, ModuleC)

