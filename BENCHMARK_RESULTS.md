# Benchmark: Reflexión vs Source Generator Incremental

## Resumen Ejecutivo

Este documento presenta los resultados del benchmark comparando el rendimiento entre el método basado en **reflexión** y el método generado por el **Source Generator Incremental (IIncrementalGenerator)** para cargar instaladores de IoC.

### Resultados Principales (.NET 8.0)

| Método | Tiempo Promedio | Ratio | Memoria Allocada | Ratio Memoria |
|--------|----------------|------|------------------|---------------|
| **Source Generator Incremental** | **89.06 ns** | **1.00** (baseline) | **104 B** | **1.00** (baseline) |
| **Reflexión** | **60.81 µs** (60,809 ns) | **682.6x** | **19.67 KB** (20,142 B) | **193.7x** |

**Conclusión**: El método con Source Generator Incremental es aproximadamente **683 veces más rápido** y utiliza **194 veces menos memoria** que el método basado en reflexión.

> **Nota importante**: Cada benchmark se ejecutó en un proceso separado para evitar sesgo por ensamblados ya cargados en el AppDomain. Esto asegura que los resultados sean precisos y representativos del rendimiento real en condiciones de uso normales.

---

## Detalles del Benchmark

### Configuración

- **Runtime**: .NET 8.0.22 (8.0.2225.52707)
- **JIT**: X64 RyuJIT AVX2
- **GC**: Concurrent Workstation
- **Hardware**: AMD Ryzen 5 3600, 1 CPU, 12 logical cores
- **OS**: Windows 10 (10.0.19045.6456/22H2/2022Update)
- **Herramienta**: BenchmarkDotNet v0.13.12
- **Generador**: IIncrementalGenerator (Microsoft.CodeAnalysis 4.9.2)

### Métodos Comparados

#### 1. LoadWithGenerator (Baseline) - Source Generator Incremental

Método que utiliza el código generado por el Source Generator Incremental en tiempo de compilación.

**Características**:
- ✅ Sin reflexión en tiempo de ejecución
- ✅ Todos los tipos conocidos en tiempo de compilación
- ✅ Compatible con AOT (Ahead-of-Time compilation)
- ✅ Compatible con trimming
- ✅ Llamadas directas a métodos (sin overhead de reflexión)
- ✅ Caching automático con IIncrementalGenerator
- ✅ Mejor rendimiento en compilaciones incrementales

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

#### Source Generator Incremental (LoadWithGenerator)
- **Mean**: 89.06 ns
- **Error**: ±0.473 ns (0.53%)
- **StdDev**: 0.419 ns
- **Min**: 88.249 ns
- **Max**: 89.854 ns
- **Median**: 89.174 ns
- **Q1**: 88.760 ns
- **Q3**: 89.251 ns
- **Confidence Interval (99.9%)**: [88.583 ns; 89.529 ns]

#### Reflexión (LoadWithReflection)
- **Mean**: 60.81 µs (60,809 ns)
- **Error**: ±0.976 µs (1.60%)
- **StdDev**: 0.913 µs
- **Min**: 59.873 µs
- **Max**: 63.200 µs
- **Median**: 60.505 µs
- **Q1**: 60.026 µs
- **Q3**: 61.394 µs
- **Confidence Interval (99.9%)**: [59.833 µs; 61.784 µs]

**Ratio de velocidad**: 60,809 ns / 89.06 ns = **682.6x más rápido**

### Métricas de Memoria

#### Source Generator Incremental (LoadWithGenerator)
- **Allocated**: 104 B por operación
- **Gen0 Collections**: 0.0124 por 1000 operaciones

#### Reflexión (LoadWithReflection)
- **Allocated**: 19.67 KB (20,142 B) por operación
- **Gen0 Collections**: 2.3804 por 1000 operaciones
- **Ratio**: 193.7x más memoria que el generador

**Ratio de memoria**: 20,142 B / 104 B = **193.7x menos memoria**

---

## Análisis de Rendimiento

### Ventajas del Source Generator Incremental

1. **Velocidad**: 683x más rápido
   - Eliminación completa del overhead de reflexión
   - Llamadas directas a métodos conocidos en tiempo de compilación
   - Sin necesidad de descubrir tipos en tiempo de ejecución
   - Caching automático con IIncrementalGenerator mejora aún más el rendimiento

2. **Memoria**: 194x menos memoria
   - No necesita cargar metadatos de tipos
   - No necesita crear objetos de reflexión (Type, MethodInfo, etc.)
   - Menos presión sobre el Garbage Collector
   - Menos colecciones de GC (0.0124 vs 2.3804 por 1000 ops)

3. **Compatibilidad**:
   - ✅ Compatible con AOT (Ahead-of-Time compilation)
   - ✅ Compatible con trimming (eliminación de código no usado)
   - ✅ Mejor para aplicaciones móviles y embebidas

4. **Seguridad de Tipos**:
   - Errores detectados en tiempo de compilación
   - Mejor experiencia de desarrollo con IntelliSense
   - Refactoring más seguro

5. **Rendimiento de Compilación**:
   - IIncrementalGenerator proporciona caching automático
   - Solo reprocesa archivos modificados en compilaciones incrementales
   - Mejores tiempos de compilación en proyectos grandes

### Desventajas del Método con Reflexión

1. **Rendimiento**: Significativamente más lento
   - Overhead de reflexión en cada ejecución
   - Descubrimiento de tipos en tiempo de ejecución
   - Creación dinámica de instancias
   - 683 veces más lento que el generador

2. **Memoria**: Mayor consumo
   - Metadatos de tipos cargados en memoria
   - Objetos de reflexión mantenidos en memoria
   - Mayor presión sobre el GC
   - 194 veces más memoria que el generador

3. **Compatibilidad**:
   - ❌ No compatible con AOT
   - ❌ No compatible con trimming
   - ❌ Problemas en aplicaciones móviles y embebidas

---

## Comparación con Versión Anterior (.NET 5.0)

### Mejoras con .NET 8.0 e IIncrementalGenerator

| Métrica | .NET 5.0 (ISourceGenerator) | .NET 8.0 (IIncrementalGenerator) | Mejora |
|---------|------------------------------|-----------------------------------|--------|
| **Source Generator Mean** | 89.35 ns | 89.06 ns | **0.3% más rápido** |
| **Reflexión Mean** | 79.93 µs | 60.81 µs | **24% más rápido** |
| **Ratio de velocidad** | 895.5x | 682.6x | Reflexión mejoró más |
| **Memoria (Generator)** | 104 B | 104 B | Sin cambios |
| **Memoria (Reflexión)** | 19.63 KB | 19.67 KB | Similar |

**Nota**: Los resultados muestran que:
- El generador incremental mantiene el mismo rendimiento excelente
- La reflexión mejoró significativamente en .NET 8.0 (24% más rápida)
- El generador sigue siendo 683 veces más rápido que la reflexión
- La ventaja del generador se mantiene abrumadora

---

## Escenarios de Uso

### Cuándo Usar Source Generator Incremental

✅ **Recomendado para**:
- Aplicaciones que requieren máximo rendimiento
- Aplicaciones móviles (iOS, Android)
- Aplicaciones embebidas
- Aplicaciones que usan AOT compilation
- Aplicaciones que requieren trimming
- Inicialización frecuente de instaladores
- Proyectos grandes con múltiples módulos (aprovecha caching incremental)

### Cuándo Considerar Reflexión

⚠️ **Considerar solo si**:
- Necesitas descubrir tipos dinámicamente en tiempo de ejecución
- Los instaladores se agregan sin recompilar
- El rendimiento de inicialización no es crítico
- No usas AOT o trimming

---

## Conclusiones

1. **Rendimiento Excepcional**: El Source Generator Incremental es **683 veces más rápido** que la reflexión, reduciendo el tiempo de carga de instaladores de ~60.81 µs a ~89.06 ns. Esta mejora es crítica en aplicaciones que requieren inicialización rápida.

2. **Eficiencia de Memoria**: El Source Generator Incremental utiliza **194 veces menos memoria** (104 B vs 20,142 B), reduciendo significativamente la presión sobre el Garbage Collector y mejorando el rendimiento general de la aplicación.

3. **Compatibilidad Moderna**: A diferencia de la reflexión, el Source Generator Incremental es completamente compatible con tecnologías modernas como:
   - **AOT (Ahead-of-Time compilation)**: Necesario para aplicaciones móviles y embebidas
   - **Trimming**: Reduce el tamaño de las aplicaciones eliminando código no usado
   - **Native AOT**: Permite compilación nativa sin runtime de .NET

4. **Rendimiento de Compilación**: IIncrementalGenerator proporciona caching automático, mejorando los tiempos de compilación en proyectos grandes al solo reprocesar archivos modificados.

5. **Seguridad de Tipos**: El código generado proporciona mejor seguridad de tipos y experiencia de desarrollo:
   - Errores detectados en tiempo de compilación, no en tiempo de ejecución
   - Mejor soporte de IntelliSense y refactoring
   - Sin sorpresas en producción por tipos no encontrados

6. **Recomendación**: Para la mayoría de los casos de uso, especialmente en aplicaciones que requieren:
   - Alto rendimiento
   - Bajo consumo de memoria
   - Compatibilidad con AOT/trimming
   - Inicialización rápida
   - Mejores tiempos de compilación
   
   El Source Generator Incremental es la opción superior y recomendada sobre soluciones basadas en reflexión.

---

## Ejecutar el Benchmark

Para ejecutar el benchmark localmente:

```bash
cd src/Demo.Host
dotnet run -c Release -- benchmark
```

Los resultados se guardarán en:
- `BenchmarkDotNet.Artifacts/results/Demo.Host.GeneratorBenchmark-report.html`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.GeneratorBenchmark-report.csv`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.GeneratorBenchmark-report-github.md`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.ReflectionBenchmark-report.html`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.ReflectionBenchmark-report.csv`
- `BenchmarkDotNet.Artifacts/results/Demo.Host.ReflectionBenchmark-report-github.md`

---

## Referencias

- [BenchmarkDotNet Documentation](https://benchmarkdotnet.org/)
- [Source Generators in C#](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/source-generators-overview)
- [Incremental Source Generators](https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.md)
- [AOT Compilation in .NET](https://docs.microsoft.com/en-us/dotnet/core/deploying/native-aot/)
- [Trimming in .NET](https://docs.microsoft.com/en-us/dotnet/core/deploying/trimming/trim-self-contained)

---

**Fecha del Benchmark**: 2025-11-11  
**Versión del Generador**: 2.0.0 (IIncrementalGenerator)  
**Runtime**: .NET 8.0.22  
**Número de Instaladores**: 3 (ModuleA, ModuleB, ModuleC)
