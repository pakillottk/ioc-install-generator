# Implementation Plan: IoC Install Generator

**Branch**: `001-ioc-install-generator` | **Date**: 2025-01-27 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/001-ioc-install-generator/spec.md`

**Note**: This template is filled in by the `/speckit.plan` command. See `.specify/templates/commands/plan.md` for the execution workflow.

## Summary

Proof of Concept de un sistema de gestión de IoC multiproyecto en C# que utiliza Source Generators para descubrir e inicializar automáticamente instaladores de IoC en tiempo de compilación. El sistema analiza las referencias del proyecto host (directas y transitivas), identifica todos los ensamblados que implementan la interfaz de instalador, y genera código estático que puede ser invocado directamente durante la inicialización de la aplicación, eliminando el overhead de reflexión y mejorando significativamente el rendimiento.

## Technical Context

**Language/Version**: C# (.NET 6.0 o superior)  
**Primary Dependencies**: 
- Microsoft.CodeAnalysis (Roslyn) para Source Generators
- Microsoft.CodeAnalysis.Testing para pruebas del generador
- xUnit o NUnit para pruebas unitarias e integración
- Abstracción genérica para contenedores IoC (método `Register<T>(...)`)

**Storage**: N/A (sistema de generación de código, no requiere persistencia)  
**Testing**: 
- Microsoft.CodeAnalysis.Testing para validar generación de código
- xUnit o NUnit para pruebas unitarias e integración
- Benchmarks comparativos con soluciones basadas en reflexión (opcional)

**Target Platform**: .NET 6.0+ (Windows, Linux, macOS)  
**Project Type**: Single project (librería de Source Generator + proyecto de demostración)  
**Performance Goals**: 
- Inicialización de hasta 50 ensamblados en <100ms
- Al menos 50% más rápido que implementación basada en reflexión
- Código generado sin overhead de descubrimiento dinámico en tiempo de ejecución

**Constraints**: 
- Source generator debe funcionar con proyectos SDK-style (PackageReference)
- Debe soportar referencias de proyecto y referencias de paquetes NuGet
- Código generado debe ser compatible con trimming y AOT cuando sea posible
- Sin uso de reflexión en tiempo de ejecución para descubrimiento de instaladores

**Scale/Scope**: 
- Soporte para hasta 50 ensamblados referenciados
- Referencias transitivas de hasta 3 niveles de profundidad
- Múltiples instaladores por ensamblado
- Orden de ejecución configurable mediante atributo en aplicación host

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### Pre-Design Check

✅ **I. Source Generator First (NON-NEGOTIABLE)**: El plan utiliza Source Generators de C# como mecanismo principal. No se propone uso de reflexión en tiempo de ejecución.

✅ **II. Performance Optimization**: Los objetivos de rendimiento están definidos (<100ms, 50% más rápido que reflexión). El código generado será estático y optimizado.

✅ **III. Multi-Assembly Architecture**: El sistema está diseñado para soportar arquitectura multiproyecto con análisis de referencias directas y transitivas.

✅ **IV. Interface-Based Design**: Se utiliza una interfaz común (`Install(IContainer container)`) para todos los instaladores. No se proponen convenciones alternativas.

✅ **V. Compile-Time Discovery**: Todo el descubrimiento ocurre en tiempo de compilación mediante el source generator. No hay descubrimiento dinámico.

**Status**: ✅ ALL GATES PASSED - Proceed to Phase 0

### Post-Design Check

✅ **I. Source Generator First (NON-NEGOTIABLE)**: 
- Diseño utiliza `ISourceGenerator` con `ISyntaxReceiver` para análisis estático
- Código generado contiene invocaciones directas, sin reflexión
- **VERIFIED**: Cumple completamente con el principio

✅ **II. Performance Optimization**: 
- Código generado es estático con invocaciones directas
- Manejo de errores eficiente (try-catch por instalador, excepción agregada al final)
- Sin overhead de descubrimiento dinámico
- **VERIFIED**: Cumple con objetivos de rendimiento

✅ **III. Multi-Assembly Architecture**: 
- Análisis de `Compilation.References` incluye referencias transitivas
- Soporte para hasta 3 niveles de profundidad
- **VERIFIED**: Arquitectura multiproyecto completamente soportada

✅ **IV. Interface-Based Design**: 
- Interfaz `IIoCInstaller` con método único `Install(IContainer container)`
- No se proponen convenciones alternativas
- Source generator busca implementaciones de la interfaz específica
- **VERIFIED**: Diseño basado en interfaces, sin convenciones alternativas

✅ **V. Compile-Time Discovery**: 
- Todo el análisis ocurre durante la compilación
- Código generado es verificable y debuggable
- No hay técnicas de descubrimiento dinámico
- **VERIFIED**: Descubrimiento completamente en tiempo de compilación

**Status**: ✅ ALL GATES PASSED - Design complies with all constitutional principles

## Project Structure

### Documentation (this feature)

```text
specs/001-ioc-install-generator/
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
├── IoC.InstallGenerator/          # Source Generator project
│   ├── IoCInstallGenerator.cs    # Main generator implementation
│   ├── SyntaxReceiver.cs          # Syntax receiver for analysis
│   └── CodeGenerator.cs           # Code generation logic
│
├── IoC.InstallGenerator.Abstractions/  # Shared abstractions
│   ├── IIoCInstaller.cs           # Interface for installers
│   ├── IIoCContainer.cs           # Generic container abstraction
│   └── InstallOrderAttribute.cs  # Attribute for ordering
│
├── IoC.InstallGenerator.Tests/    # Generator tests
│   ├── GeneratorTests.cs         # Tests using Microsoft.CodeAnalysis.Testing
│   └── TestCases/                 # Test scenarios
│
├── Demo.Host/                     # Demo application host
│   ├── Program.cs                 # Application entry point
│   └── Demo.Host.csproj
│
└── Demo.Modules/                  # Demo modules with installers
    ├── ModuleA/
    │   ├── ModuleAInstaller.cs
    │   └── ModuleA.csproj
    ├── ModuleB/
    │   ├── ModuleBInstaller.cs
    │   └── ModuleB.csproj
    └── ...

tests/
├── integration/                   # Integration tests
│   ├── MultiAssemblyTests.cs     # Tests with multiple assemblies
│   └── PerformanceTests.cs       # Benchmark comparisons
│
└── unit/                          # Unit tests
    └── ...
```

**Structure Decision**: Se utiliza una estructura de proyecto único con múltiples proyectos dentro de la solución:
- Proyecto del Source Generator (separado según constitución)
- Proyecto de abstracciones compartidas
- Proyecto de pruebas del generador
- Proyecto de demostración (host + módulos de ejemplo)

Esta estructura permite mantener el generador separado del código de demostración, facilita las pruebas, y proporciona ejemplos claros de uso.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

No violations detected. The proposed structure aligns with all constitutional principles.
