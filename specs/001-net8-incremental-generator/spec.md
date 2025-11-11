# Feature Specification: Actualización a .NET 8 e Incremental Source Generator

**Feature Branch**: `001-net8-incremental-generator`  
**Created**: 2025-01-11  
**Status**: Draft  
**Input**: User description: "actualizar todo a .NET 8 y hacer un Incremental source generator."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Actualizar todos los proyectos a .NET 8 (Priority: P1)

Los desarrolladores necesitan que todos los proyectos del repositorio se actualicen a .NET 8 para aprovechar las mejoras de rendimiento, nuevas características del lenguaje y soporte a largo plazo. Esto incluye actualizar los archivos de proyecto, dependencias y asegurar que la compilación funcione correctamente.

**Why this priority**: Es el requisito fundamental que habilita todas las demás mejoras. Sin esta actualización, no se pueden aprovechar las características modernas de .NET 8 ni convertir el generador a incremental.

**Independent Test**: Se puede verificar ejecutando `dotnet build` en todos los proyectos y confirmando que compilan exitosamente con .NET 8. Los tests unitarios deben ejecutarse sin errores.

**Acceptance Scenarios**:

1. **Given** un proyecto con .NET 5.0 o netstandard2.0, **When** se actualiza el TargetFramework a net8.0, **Then** el proyecto compila exitosamente
2. **Given** todos los proyectos actualizados a .NET 8, **When** se ejecuta `dotnet build`, **Then** todos los proyectos compilan sin errores
3. **Given** proyectos con dependencias de paquetes NuGet, **When** se actualizan a versiones compatibles con .NET 8, **Then** las referencias se resuelven correctamente
4. **Given** la solución completa actualizada, **When** se ejecuta `dotnet test`, **Then** todos los tests pasan exitosamente

---

### User Story 2 - Convertir Source Generator a Incremental (Priority: P2)

Los desarrolladores necesitan que el source generator actual use la API de Incremental Source Generator para mejorar el rendimiento de compilación, reducir el uso de memoria y aprovechar el caching automático que proporciona .NET 8.

**Why this priority**: Los Incremental Source Generators ofrecen mejor rendimiento y eficiencia que los generadores tradicionales, especialmente en proyectos grandes. Esta mejora es crítica para la experiencia del desarrollador.

**Independent Test**: Se puede verificar que el generador funciona correctamente generando el mismo código que antes, pero usando la API incremental. Los tiempos de compilación deben mantenerse o mejorar.

**Acceptance Scenarios**:

1. **Given** un proyecto que usa el source generator, **When** se convierte a incremental generator, **Then** el código generado es idéntico al anterior
2. **Given** un proyecto con múltiples módulos referenciados, **When** se ejecuta el generador incremental, **Then** todos los instaladores se descubren y generan correctamente
3. **Given** cambios en archivos fuente, **When** se recompila el proyecto, **Then** el generador incremental solo procesa archivos modificados (caching)
4. **Given** el generador incremental funcionando, **When** se ejecutan los tests del generador, **Then** todos los tests pasan exitosamente

---

### User Story 3 - Actualizar dependencias y paquetes NuGet (Priority: P3)

Los desarrolladores necesitan que todas las dependencias de paquetes NuGet se actualicen a versiones compatibles con .NET 8 y que aprovechen las últimas versiones estables de las bibliotecas utilizadas.

**Why this priority**: Asegura compatibilidad y aprovecha mejoras de seguridad y rendimiento de las dependencias actualizadas. Es importante pero no bloquea la funcionalidad principal.

**Independent Test**: Se puede verificar que todas las referencias de paquetes se resuelven correctamente y que no hay conflictos de versiones. Los tests deben ejecutarse sin problemas.

**Acceptance Scenarios**:

1. **Given** paquetes NuGet con versiones antiguas, **When** se actualizan a versiones compatibles con .NET 8, **Then** todas las referencias se resuelven sin conflictos
2. **Given** Microsoft.CodeAnalysis con versión 3.8.0, **When** se actualiza a versión compatible con .NET 8, **Then** el generador funciona correctamente
3. **Given** todas las dependencias actualizadas, **When** se ejecuta `dotnet restore`, **Then** todos los paquetes se descargan y resuelven correctamente

---

### Edge Cases

- ¿Qué sucede cuando un proyecto tiene dependencias que no son compatibles con .NET 8?
- ¿Cómo maneja el sistema la migración de proyectos que usan características específicas de .NET 5.0?
- ¿Qué ocurre si el generador incremental encuentra errores durante el análisis incremental?
- ¿Cómo se comporta el generador cuando hay cambios en referencias transitivas?
- ¿Qué sucede si hay proyectos de ejemplo o demos que también necesitan actualización?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Sistema DEBE actualizar todos los archivos .csproj del repositorio para usar net8.0 como TargetFramework
- **FR-002**: Sistema DEBE mantener la compatibilidad con la funcionalidad existente del source generator después de la migración
- **FR-003**: Sistema DEBE convertir la implementación de ISourceGenerator a IIncrementalGenerator
- **FR-004**: Sistema DEBE actualizar los paquetes Microsoft.CodeAnalysis a versiones compatibles con .NET 8 e Incremental Generators
- **FR-005**: Sistema DEBE actualizar todos los proyectos de ejemplo y demos a .NET 8
- **FR-006**: Sistema DEBE actualizar el proyecto de abstracciones (IoC.InstallGenerator.Abstractions) a .NET 8 manteniendo compatibilidad con netstandard2.0 si es necesario para compatibilidad hacia atrás
- **FR-007**: Sistema DEBE actualizar el proyecto de tests a .NET 8 y actualizar las dependencias de testing
- **FR-008**: Sistema DEBE mantener la misma funcionalidad de descubrimiento de instaladores después de la conversión a incremental
- **FR-009**: Sistema DEBE generar el mismo código que el generador anterior para garantizar compatibilidad
- **FR-010**: Sistema DEBE manejar correctamente el caching incremental para mejorar tiempos de compilación

### Key Entities

- **Proyecto de Generador**: El proyecto IoC.InstallGenerator que contiene la implementación del source generator
- **Proyecto de Abstracciones**: El proyecto IoC.InstallGenerator.Abstractions que contiene las interfaces y atributos
- **Proyectos de Ejemplo**: Los proyectos Demo.Host y Demo.Modules que demuestran el uso del generador
- **Proyecto de Tests**: El proyecto IoC.InstallGenerator.Tests que contiene las pruebas unitarias

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Todos los proyectos del repositorio compilan exitosamente con .NET 8 en menos de 30 segundos
- **SC-002**: El 100% de los tests unitarios existentes pasan después de la migración
- **SC-003**: El generador incremental genera código idéntico al generador anterior para los mismos inputs
- **SC-004**: Los tiempos de compilación se mantienen o mejoran comparados con la versión anterior
- **SC-005**: El generador incremental procesa correctamente proyectos con hasta 50 ensamblados referenciados
- **SC-006**: La funcionalidad de descubrimiento de instaladores funciona correctamente en el 100% de los casos de prueba
- **SC-007**: Todas las dependencias de paquetes NuGet se resuelven sin conflictos después de la actualización

## Assumptions

- Los proyectos de ejemplo (Demo.Host, Demo.Modules) pueden actualizarse a .NET 8 sin problemas de compatibilidad
- Las versiones actuales de Microsoft.CodeAnalysis tienen soporte completo para IIncrementalGenerator en .NET 8
- No se requiere mantener compatibilidad con versiones anteriores de .NET para los proyectos de ejemplo
- El proyecto de abstracciones puede mantener netstandard2.0 o actualizarse a net8.0 según sea necesario para compatibilidad
- Los benchmarks existentes pueden actualizarse para funcionar con .NET 8

## Dependencies

- .NET 8 SDK instalado en el entorno de desarrollo
- Acceso a NuGet para descargar paquetes actualizados
- Proyectos existentes funcionando correctamente antes de la migración

## Out of Scope

- Cambios en la funcionalidad del generador más allá de la conversión a incremental
- Mejoras de rendimiento adicionales no relacionadas con la migración
- Cambios en la API pública del generador
- Actualización de documentación más allá de lo necesario para reflejar .NET 8
