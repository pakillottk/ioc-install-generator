# Implementation Plan: Actualización a .NET 8 e Incremental Source Generator

**Branch**: `001-net8-incremental-generator` | **Date**: 2025-01-11 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-net8-incremental-generator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Actualización completa del proyecto IoC Install Generator a .NET 8 y conversión del source generator de ISourceGenerator a IIncrementalGenerator. La migración incluye actualizar todos los proyectos del repositorio (generador, abstracciones, tests, demos), actualizar dependencias de paquetes NuGet a versiones compatibles con .NET 8, y refactorizar la implementación del generador para usar la API incremental que proporciona mejor rendimiento y caching automático.

## Technical Context

**Language/Version**: C# (.NET 8.0)  
**Primary Dependencies**: 
- Microsoft.CodeAnalysis.CSharp (versión compatible con .NET 8 e IIncrementalGenerator)
- Microsoft.CodeAnalysis (versión compatible con .NET 8)
- Microsoft.CodeAnalysis.Analyzers (versión compatible con .NET 8)
- Microsoft.NET.Test.Sdk (versión compatible con .NET 8)
- xUnit (versión compatible con .NET 8)
- BenchmarkDotNet (versión compatible con .NET 8)

**Storage**: N/A (source generator, no requiere persistencia)  
**Testing**: 
- xUnit para pruebas unitarias
- Microsoft.CodeAnalysis.Testing para validar generación de código
- BenchmarkDotNet para benchmarks de rendimiento

**Target Platform**: .NET 8.0 (Windows, Linux, macOS)  
**Project Type**: Single solution con múltiples proyectos (source generator, abstracciones, tests, demos)  
**Performance Goals**: 
- Compilación de todos los proyectos en menos de 30 segundos
- Tiempos de compilación mantenidos o mejorados con generador incremental
- Caching incremental funcional para reducir reprocesamiento

**Constraints**: 
- Mantener compatibilidad con funcionalidad existente
- Código generado debe ser idéntico al anterior
- Todos los tests existentes deben pasar
- Compatible con proyectos SDK-style (PackageReference)
- Soporte para hasta 50 ensamblados referenciados

**Scale/Scope**: 
- 7 proyectos en la solución (generador, abstracciones, tests, host demo, 3 módulos demo)
- Migración de .NET 5.0/netstandard2.0 a .NET 8.0
- Conversión de ISourceGenerator a IIncrementalGenerator

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **I. Source Generator First (NON-NEGOTIABLE)**: 
- La migración mantiene el uso de Source Generators
- La conversión a IIncrementalGenerator es una mejora, no un cambio de principio
- No se introduce reflexión en tiempo de ejecución
- **VERIFIED**: Cumple con el principio fundamental

✅ **II. Performance Optimization**: 
- IIncrementalGenerator mejora el rendimiento con caching automático
- El código generado sigue siendo estático con invocaciones directas
- Los tiempos de compilación se mantienen o mejoran
- **VERIFIED**: La migración mejora el rendimiento

✅ **III. Multi-Assembly Architecture**: 
- La arquitectura multiproyecto se mantiene
- El generador incremental sigue analizando referencias transitivas
- No se cambia la funcionalidad de descubrimiento
- **VERIFIED**: Arquitectura multiproyecto preservada

✅ **IV. Interface-Based Design**: 
- La interfaz IIoCInstaller se mantiene sin cambios
- No se introducen convenciones alternativas
- El generador sigue buscando implementaciones de la interfaz
- **VERIFIED**: Diseño basado en interfaces preservado

✅ **V. Compile-Time Discovery**: 
- El descubrimiento sigue ocurriendo en tiempo de compilación
- El código generado sigue siendo verificable y debuggable
- No se introducen técnicas de descubrimiento dinámico
- **VERIFIED**: Descubrimiento en tiempo de compilación preservado

**Status**: ✅ ALL GATES PASSED - La migración a .NET 8 e IIncrementalGenerator cumple con todos los principios constitucionales

## Project Structure

### Documentation (this feature)

```text
specs/001-net8-incremental-generator/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
src/
├── IoC.InstallGenerator/              # Source Generator project (net8.0)
│   ├── IoCInstallGenerator.cs         # Main generator (IIncrementalGenerator)
│   ├── SyntaxReceiver.cs              # [DEPRECATED - replaced by incremental pipeline]
│   └── CodeGenerator.cs               # Code generation logic
│
├── IoC.InstallGenerator.Abstractions/ # Shared abstractions (net8.0 o netstandard2.0)
│   ├── IIoCInstaller.cs               # Interface for installers
│   ├── IIoCContainer.cs               # Generic container abstraction
│   ├── InstallOrderAttribute.cs      # Attribute for ordering
│   └── IoCInstallerLoaderAttribute.cs # Attribute for loader classes
│
├── IoC.InstallGenerator.Tests/       # Generator tests (net8.0)
│   ├── GeneratorTests.cs              # Tests using Microsoft.CodeAnalysis.Testing
│   ├── InstallerLoadingTests.cs      # Integration tests
│   └── PerformanceTests.cs           # Benchmark tests
│
├── Demo.Host/                         # Demo application host (net8.0)
│   ├── Program.cs                     # Application entry point
│   ├── SimpleContainerAdapter.cs      # Container adapter implementation
│   ├── Verifier.cs                    # Service verification
│   ├── Benchmark.cs                   # Benchmark runner
│   └── Demo.Host.csproj
│
└── Demo.Modules/                      # Demo modules with installers (net8.0)
    ├── ModuleA/
    │   ├── ModuleAInstaller.cs
    │   └── ModuleA.csproj
    ├── ModuleB/
    │   ├── ModuleBInstaller.cs
    │   └── ModuleB.csproj
    └── ModuleC/
        ├── ModuleCInstaller.cs
        └── ModuleC.csproj
```

**Structure Decision**: Se mantiene la estructura existente de proyecto único con múltiples proyectos dentro de la solución. Todos los proyectos se actualizan a .NET 8.0, excepto posiblemente IoC.InstallGenerator.Abstractions que puede mantener netstandard2.0 para compatibilidad hacia atrás si es necesario. El generador se refactoriza para usar IIncrementalGenerator en lugar de ISourceGenerator, eliminando la necesidad de SyntaxReceiver.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations detected. La migración a .NET 8 e IIncrementalGenerator es una mejora técnica que no viola ningún principio constitucional. La estructura del proyecto se mantiene igual, solo se actualizan las versiones y se refactoriza el generador para usar la API incremental.
