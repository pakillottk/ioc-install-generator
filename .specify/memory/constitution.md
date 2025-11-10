<!--
Sync Impact Report:
Version change: N/A → 1.0.0 (initial creation)
Modified principles: N/A (new constitution)
Added sections: Core Principles, Technology Stack, Development Workflow, Governance
Removed sections: N/A
Templates requiring updates:
  ✅ .specify/templates/plan-template.md (Constitution Check section compatible)
  ✅ .specify/templates/spec-template.md (no changes needed)
  ✅ .specify/templates/tasks-template.md (no changes needed)
  ✅ .specify/templates/commands/speckit.constitution.md (created)
Follow-up TODOs: None
-->

# IoC Install Generator Constitution

## Core Principles

### I. Source Generator First (NON-NEGOTIABLE)

El sistema DEBE usar Source Generators de C# para descubrir y cargar instaladores de IoC en tiempo de compilación. La reflexión en tiempo de ejecución está PROHIBIDA para el descubrimiento de instaladores. El generador analiza las referencias del proyecto host, identifica todos los ensamblados que implementan la interfaz de instalador, y genera código estático que puede ser invocado directamente. Esta aproximación elimina el overhead de reflexión y mejora significativamente el rendimiento de inicialización.

**Rationale**: El objetivo principal del proyecto es demostrar que el uso de source generators es más eficiente que la reflexión tradicional para el descubrimiento de instaladores en sistemas multiproyecto.

### II. Performance Optimization

Todas las decisiones de diseño DEBEN priorizar el rendimiento y la eficiencia. El código generado DEBE ser optimizado para minimizar el tiempo de inicialización. Las operaciones costosas DEBEN realizarse en tiempo de compilación cuando sea posible. El sistema DEBE ser capaz de inicializar el contenedor IoC y cargar todos los instaladores con overhead mínimo en tiempo de ejecución.

**Rationale**: La justificación principal del proyecto es demostrar mejoras de rendimiento sobre soluciones basadas en reflexión.

### III. Multi-Assembly Architecture

El sistema DEBE soportar una arquitectura multiproyecto donde una aplicación host central depende de N ensamblados independientes. Cada ensamblado puede implementar su propio instalador de IoC. El source generator DEBE analizar todas las referencias transitivas del proyecto host para descubrir todos los instaladores disponibles, sin requerir configuración manual adicional.

**Rationale**: El escenario de uso principal es una aplicación modular con múltiples ensamblados que necesitan registrar sus dependencias en un contenedor IoC centralizado.

### IV. Interface-Based Design

Todos los instaladores DEBEN implementar una interfaz común de instalador de IoC definida por el sistema. La interfaz DEBE ser simple y clara, aceptando el contenedor IoC como parámetro. El source generator DEBE buscar implementaciones de esta interfaz específica. No se permiten convenciones alternativas o atributos personalizados para identificar instaladores.

**Rationale**: El uso de interfaces proporciona un contrato claro y permite verificación en tiempo de compilación, facilitando el análisis estático del source generator.

### V. Compile-Time Discovery

El descubrimiento de instaladores DEBE ocurrir completamente en tiempo de compilación. El source generator DEBE generar código estático que liste todos los instaladores encontrados. Este código generado DEBE ser verificable y debuggable. No se permiten técnicas de descubrimiento dinámico o basadas en convenciones que requieran análisis en tiempo de ejecución.

**Rationale**: El descubrimiento en tiempo de compilación es fundamental para lograr las mejoras de rendimiento que justifican el uso de source generators sobre reflexión.

## Technology Stack

**Language**: C# (.NET 6.0 o superior)  
**Source Generator Framework**: Microsoft.CodeAnalysis (Roslyn)  
**IoC Container**: Agnostico - el sistema DEBE ser compatible con cualquier contenedor IoC que implemente una interfaz mínima común (por ejemplo, IServiceCollection de Microsoft.Extensions.DependencyInjection o equivalente)  
**Testing**: xUnit o NUnit para pruebas unitarias e integración  
**Build System**: MSBuild / dotnet CLI

**Constraints**:
- El source generator DEBE funcionar con proyectos que usan PackageReference (SDK-style projects)
- DEBE soportar referencias de proyecto y referencias de paquetes NuGet
- El código generado DEBE ser compatible con trimming y AOT cuando sea posible

## Development Workflow

### Source Generator Development

1. El source generator DEBE estar en un proyecto separado del proyecto de demostración
2. Las pruebas del source generator DEBEN usar Microsoft.CodeAnalysis.Testing para validar la generación de código
3. Cada cambio en el generador DEBE incluir pruebas que verifiquen el código generado
4. El código generado DEBE seguir las convenciones de estilo del proyecto

### Testing Requirements

- Pruebas unitarias para el source generator (análisis de sintaxis, generación de código)
- Pruebas de integración que demuestren el uso completo del sistema con múltiples ensamblados
- Benchmarks comparativos con soluciones basadas en reflexión (opcional pero recomendado)
- Validación de que todos los instaladores son descubiertos correctamente

### Code Quality

- El código generado DEBE ser legible y bien formateado
- El source generator DEBE manejar casos edge (ensamblados sin instaladores, instaladores duplicados, etc.)
- Errores del generador DEBEN ser claros y ayudar a diagnosticar problemas

## Governance

Esta constitución rige todas las decisiones de diseño y desarrollo del proyecto. Cualquier desviación de estos principios DEBE ser justificada documentalmente y requiere revisión.

**Amendment Procedure**: Las modificaciones a esta constitución requieren:
1. Documentación del cambio propuesto y su justificación
2. Actualización de la versión según semántica (MAJOR.MINOR.PATCH)
3. Actualización de todos los templates y documentos relacionados
4. Revisión y aprobación antes de implementar cambios que violen principios existentes

**Compliance Review**: Todos los PRs y cambios de código DEBEN verificar cumplimiento con los principios establecidos. Las violaciones no justificadas DEBEN ser rechazadas.

**Versioning Policy**: 
- MAJOR: Cambios incompatibles en principios o eliminación de principios
- MINOR: Adición de nuevos principios o expansión material de guías
- PATCH: Clarificaciones, correcciones de redacción, refinamientos no semánticos

**Version**: 1.0.0 | **Ratified**: 2025-01-27 | **Last Amended**: 2025-01-27
