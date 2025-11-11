# Research: Actualización a .NET 8 e Incremental Source Generator

**Feature**: Actualización a .NET 8 e Incremental Source Generator  
**Date**: 2025-01-11  
**Phase**: 0 - Outline & Research

## Research Areas

### 1. Versiones de Microsoft.CodeAnalysis para .NET 8 e IIncrementalGenerator

**Decision**: Usar Microsoft.CodeAnalysis.CSharp versión 4.8.0 o superior (preferiblemente 4.9.0+) que incluye soporte completo para IIncrementalGenerator y .NET 8.

**Rationale**: 
- IIncrementalGenerator fue introducido en Microsoft.CodeAnalysis.CSharp 4.0.0
- Las versiones 4.8.0+ incluyen mejoras de rendimiento y estabilidad para .NET 8
- Versión 4.9.0+ incluye optimizaciones específicas para .NET 8
- Compatible con proyectos que usan .NET 8 como TargetFramework

**Alternatives considered**:
- Mantener versión 3.8.0: No soporta IIncrementalGenerator, requiere ISourceGenerator
- Usar versión 4.0.0-4.7.0: Soporta IIncrementalGenerator pero puede tener problemas de compatibilidad con .NET 8
- Usar versión 5.0.0+: Versiones futuras, pueden no estar disponibles aún

**Implementation approach**:
- Actualizar Microsoft.CodeAnalysis.CSharp a versión 4.9.0 o superior
- Actualizar Microsoft.CodeAnalysis a la misma versión
- Actualizar Microsoft.CodeAnalysis.Analyzers a versión compatible
- Verificar que todas las referencias usen la misma versión para evitar conflictos

### 2. Migración de ISourceGenerator a IIncrementalGenerator

**Decision**: Refactorizar la implementación para usar IIncrementalGenerator con IncrementalGeneratorInitializationContext y crear un pipeline incremental usando SyntaxValueProvider y otros transformadores incrementales.

**Rationale**:
- IIncrementalGenerator proporciona mejor rendimiento con caching automático
- Solo procesa archivos modificados en compilaciones incrementales
- Reduce uso de memoria al no mantener estado completo del árbol de sintaxis
- Mejor integración con el pipeline de compilación de Roslyn
- Elimina la necesidad de ISyntaxReceiver

**Alternatives considered**:
- Mantener ISourceGenerator: Funciona pero no aprovecha mejoras de rendimiento
- Usar ambos (ISourceGenerator + IIncrementalGenerator): Añade complejidad innecesaria
- Crear wrapper que use ambos: Más complejo, no necesario

**Implementation approach**:
- Reemplazar `ISourceGenerator` con `IIncrementalGenerator`
- Eliminar `SyntaxReceiver` (ya no necesario)
- Usar `context.SyntaxProvider.CreateSyntaxProvider()` para encontrar clases con atributo `[IoCInstallerLoader]`
- Usar `context.CompilationProvider` para análisis de compilación
- Usar `context.AdditionalTextsProvider` si es necesario para archivos adicionales
- Combinar providers usando `Combine()` y `SelectMany()` para crear el pipeline incremental
- Generar código usando `context.RegisterSourceOutput()` con el pipeline

### 3. Compatibilidad netstandard2.0 vs net8.0 para Abstractions

**Decision**: Actualizar IoC.InstallGenerator.Abstractions a net8.0, pero mantener compatibilidad con netstandard2.0 usando multi-targeting si es necesario para compatibilidad hacia atrás.

**Rationale**:
- net8.0 proporciona mejor rendimiento y características modernas
- Si se requiere compatibilidad con proyectos que usan versiones anteriores de .NET, multi-targeting permite soportar ambos
- netstandard2.0 es compatible con .NET Framework 4.6.1+, .NET Core 2.0+, .NET 5.0+, .NET 6.0+, .NET 7.0+, .NET 8.0+
- Para un proyecto nuevo enfocado en .NET 8, net8.0 es suficiente

**Alternatives considered**:
- Solo netstandard2.0: Mantiene compatibilidad máxima pero no aprovecha características de .NET 8
- Solo net8.0: Más simple pero puede limitar compatibilidad si hay usuarios con versiones anteriores
- Multi-targeting (netstandard2.0;net8.0): Máxima compatibilidad pero añade complejidad de compilación

**Implementation approach**:
- Evaluar si hay dependencias externas que requieran netstandard2.0
- Si no hay dependencias externas críticas, usar solo net8.0
- Si hay dependencias que requieren netstandard2.0, usar multi-targeting: `<TargetFrameworks>netstandard2.0;net8.0</TargetFrameworks>`
- Para este proyecto, comenzar con net8.0 y evaluar necesidad de multi-targeting según feedback

### 4. Actualización de Proyectos de Tests

**Decision**: Actualizar IoC.InstallGenerator.Tests a .NET 8 y actualizar Microsoft.CodeAnalysis.Testing a versión compatible con .NET 8.

**Rationale**:
- Microsoft.CodeAnalysis.Testing requiere versiones compatibles con el generador
- .NET 8 proporciona mejor rendimiento para ejecución de tests
- Compatibilidad con nuevas características de testing en .NET 8
- Alineado con el resto de la solución

**Alternatives considered**:
- Mantener .NET 5.0: Funciona pero no aprovecha mejoras
- Usar .NET 6.0 o 7.0: Funciona pero no es la versión objetivo

**Implementation approach**:
- Actualizar TargetFramework a net8.0
- Actualizar Microsoft.NET.Test.Sdk a versión compatible con .NET 8
- Actualizar Microsoft.CodeAnalysis.Testing a versión compatible con .NET 8 y Microsoft.CodeAnalysis 4.9.0+
- Actualizar xUnit a versión compatible con .NET 8
- Verificar que todos los tests pasen después de la actualización

### 5. Actualización de Proyectos Demo

**Decision**: Actualizar Demo.Host y todos los proyectos Demo.Modules a .NET 8.

**Rationale**:
- Demostración debe usar la misma versión que el generador
- .NET 8 proporciona mejor rendimiento para benchmarks
- Simplifica la solución al tener una sola versión objetivo
- Alineado con el objetivo de la migración

**Alternatives considered**:
- Mantener .NET 5.0: Funciona pero no demuestra uso con .NET 8
- Multi-targeting: Añade complejidad innecesaria para proyectos de demostración

**Implementation approach**:
- Actualizar TargetFramework a net8.0 en todos los proyectos demo
- Actualizar BenchmarkDotNet a versión compatible con .NET 8
- Verificar que los benchmarks funcionen correctamente
- Actualizar cualquier código que use características específicas de .NET 5.0

### 6. Cambios en la API del Generador

**Decision**: Mantener la misma API pública (atributos, interfaces) sin cambios, solo cambiar la implementación interna del generador.

**Rationale**:
- No rompe compatibilidad con código existente que usa el generador
- Los usuarios no necesitan cambiar su código
- Solo se mejora la implementación interna
- El código generado debe ser idéntico al anterior

**Alternatives considered**:
- Cambiar la API: Rompería compatibilidad, requiere cambios en código de usuarios
- Añadir nuevas APIs: Añade complejidad, no necesario para esta migración

**Implementation approach**:
- Mantener `[IoCInstallerLoader]` atributo sin cambios
- Mantener `IIoCInstaller` interfaz sin cambios
- Mantener `IIoCContainer` interfaz sin cambios
- Mantener `[InstallOrder]` atributo sin cambios
- Solo cambiar la implementación interna de `IoCInstallGenerator` de `ISourceGenerator` a `IIncrementalGenerator`
- Verificar que el código generado sea idéntico al anterior

### 7. Pipeline Incremental del Generador

**Decision**: Crear un pipeline incremental que:
1. Encuentre clases con `[IoCInstallerLoader]` usando `SyntaxProvider`
2. Analice la compilación para encontrar tipos que implementan `IIoCInstaller`
3. Combine ambos para generar código para cada clase loader encontrada

**Rationale**:
- Pipeline incremental permite caching automático
- Solo reprocesa cuando cambian los inputs relevantes
- Mejor rendimiento en compilaciones incrementales
- Alineado con mejores prácticas de IIncrementalGenerator

**Alternatives considered**:
- Procesar todo en cada compilación: Funciona pero no aprovecha caching
- Pipeline más complejo con múltiples etapas: Añade complejidad innecesaria

**Implementation approach**:
- Usar `context.SyntaxProvider.CreateSyntaxProvider()` para encontrar `ClassDeclarationSyntax` con `[IoCInstallerLoader]`
- Usar `context.CompilationProvider` para análisis de tipos que implementan `IIoCInstaller`
- Usar `Combine()` para combinar ambos providers
- Usar `SelectMany()` si hay múltiples clases loader
- Usar `RegisterSourceOutput()` para generar código
- Mantener la misma lógica de descubrimiento de instaladores que en la versión anterior

## Technology Decisions Summary

| Aspecto | Decisión | Justificación |
|---------|----------|---------------|
| Versión Microsoft.CodeAnalysis | 4.9.0+ | Soporte completo para IIncrementalGenerator y .NET 8 |
| API del Generador | IIncrementalGenerator | Mejor rendimiento y caching automático |
| TargetFramework Generador | net8.0 | Alineado con objetivo de migración |
| TargetFramework Abstractions | net8.0 (o multi-targeting) | Simplicidad o compatibilidad según necesidad |
| TargetFramework Tests | net8.0 | Alineado con generador |
| TargetFramework Demos | net8.0 | Demostración con .NET 8 |
| API Pública | Sin cambios | Mantiene compatibilidad |
| Pipeline | Incremental con SyntaxProvider + CompilationProvider | Mejor rendimiento y caching |

## Open Questions Resolved

✅ **Q1**: ¿Qué versión de Microsoft.CodeAnalysis usar? → 4.9.0+ para soporte completo de IIncrementalGenerator y .NET 8  
✅ **Q2**: ¿Cómo migrar de ISourceGenerator a IIncrementalGenerator? → Usar pipeline incremental con SyntaxProvider y CompilationProvider  
✅ **Q3**: ¿Qué TargetFramework usar para Abstractions? → net8.0 (evaluar multi-targeting si es necesario)  
✅ **Q4**: ¿Cómo mantener compatibilidad de API? → Mantener atributos e interfaces sin cambios  
✅ **Q5**: ¿Cómo estructurar el pipeline incremental? → SyntaxProvider para clases loader + CompilationProvider para análisis de tipos  
✅ **Q6**: ¿Qué hacer con SyntaxReceiver? → Eliminar, reemplazado por pipeline incremental

## Next Steps

Proceed to Phase 1: Design & Contracts
- Actualizar modelo de datos si es necesario (probablemente no, ya que la API no cambia)
- Verificar contratos existentes (atributos, interfaces)
- Actualizar quickstart guide para reflejar .NET 8

