# Research: IoC Install Generator

**Feature**: IoC Install Generator  
**Date**: 2025-01-27  
**Phase**: 0 - Outline & Research

## Research Areas

### 1. Source Generators en C# - Análisis de Referencias

**Decision**: Usar `ISourceGenerator` con `ISyntaxReceiver` para analizar referencias de proyecto y descubrir tipos que implementan la interfaz de instalador.

**Rationale**: 
- `ISyntaxReceiver` permite analizar el código fuente durante la compilación sin cargar ensamblados
- Permite acceso a información de referencias del proyecto mediante `Compilation.References`
- Compatible con análisis estático en tiempo de compilación
- No requiere reflexión en tiempo de ejecución

**Alternatives considered**:
- Análisis de metadatos de ensamblados: Requeriría cargar ensamblados en tiempo de compilación, más complejo
- Análisis de archivos .csproj: Menos preciso, no captura referencias transitivas automáticamente
- Uso de MSBuild tasks: Más complejo, menos integrado con el pipeline de compilación

**Implementation approach**:
- Usar `Compilation.References` para obtener todas las referencias (directas y transitivas)
- Para cada referencia, obtener el `Compilation` del ensamblado referenciado
- Buscar tipos que implementen `IIoCInstaller` en cada compilación
- Generar código estático con referencias directas a los tipos encontrados

### 2. Generación de Código Estático para Instaladores

**Decision**: Generar una clase estática con un método que acepte el contenedor IoC y ejecute todos los instaladores en el orden configurado.

**Rationale**:
- Código estático es más eficiente que descubrimiento dinámico
- Permite verificación en tiempo de compilación
- Fácil de depurar (código visible en el proyecto generado)
- Compatible con trimming y AOT

**Alternatives considered**:
- Generar código que use reflexión: Violaría el principio de Source Generator First
- Generar código que use DI para resolver instaladores: Añadiría complejidad innecesaria
- Generar código que use expresiones lambda: Más complejo, menos legible

**Implementation approach**:
- Generar clase `IoCInstallerLoader` con método estático `LoadAll(IContainer container)`
- Incluir try-catch para cada instalador para capturar excepciones
- Acumular excepciones y lanzar `AggregateException` al final si hay fallos
- Respetar el orden especificado en el atributo de configuración

### 3. Abstracción Genérica para Contenedores IoC

**Decision**: Crear interfaz genérica `IIoCContainer` con método `Register<T>(...)` que pueda ser implementada por adaptadores para diferentes contenedores.

**Rationale**:
- Balance entre compatibilidad y simplicidad
- Permite soportar múltiples contenedores (Autofac, Unity, SimpleInjector, etc.)
- Mantiene el código generado simple y genérico
- Facilita testing con contenedores mock

**Alternatives considered**:
- Interfaz específica del sistema: Requeriría adaptadores para cada contenedor, más trabajo inicial
- Solo IServiceCollection: Más simple pero menos flexible, limita casos de uso
- Duck typing: Máxima flexibilidad pero sin verificación en compilación, más propenso a errores

**Implementation approach**:
- Definir `IIoCContainer` con método genérico `Register<TService, TImplementation>()`
- Crear adaptadores para contenedores comunes (opcional, para demostración)
- Los instaladores usan esta interfaz genérica en lugar de APIs específicas del contenedor

### 4. Configuración de Orden mediante Atributo

**Decision**: Usar atributo `[InstallOrder]` en la clase principal de la aplicación host que especifique la lista ordenada de nombres de ensamblados.

**Rationale**:
- Resuelto en tiempo de compilación, compatible con source generators
- No requiere archivos de configuración adicionales
- Declarativo y fácil de entender
- Permite al source generator leer la configuración durante la generación

**Alternatives considered**:
- Atributos en clases instaladoras: Más granular pero más complejo de mantener
- Archivo de configuración: Flexible pero requiere archivo adicional y parsing
- Parámetro en método de carga: Simple pero debe especificarse en cada llamada

**Implementation approach**:
- Definir `[InstallOrder(params string[] assemblyNames)]` atributo
- El source generator busca este atributo en la clase principal del proyecto host
- Ordena los instaladores según el orden de los ensamblados en el atributo
- Si no se encuentra el atributo, usa orden de descubrimiento (arbitrario)

### 5. Manejo de Errores durante Carga

**Decision**: Continuar ejecutando todos los instaladores incluso si algunos fallan, y lanzar `AggregateException` al final con todos los errores ocurridos.

**Rationale**:
- Balance entre resiliencia y visibilidad de errores
- Permite que la aplicación inicie parcialmente si algunos módulos fallan
- Proporciona información completa sobre todos los fallos
- Alineado con patrones comunes de inicialización de aplicaciones

**Alternatives considered**:
- Detener en primer error: Más simple pero menos resiliente
- Solo registrar errores sin lanzar: Resiliente pero puede ocultar problemas críticos
- Configurable: Más flexible pero añade complejidad innecesaria para el PoC

**Implementation approach**:
- Envolver cada llamada a `Install()` en try-catch
- Acumular excepciones en una lista
- Si hay excepciones al finalizar, lanzar `AggregateException` con todas las excepciones
- Incluir información del instalador que falló en cada excepción

### 6. Análisis de Referencias Transitivas

**Decision**: Usar `Compilation.References` que incluye referencias transitivas automáticamente, y analizar cada una recursivamente hasta 3 niveles de profundidad.

**Rationale**:
- `Compilation.References` ya incluye referencias transitivas
- Límite de 3 niveles previene análisis infinito en casos de dependencias circulares
- Alineado con el requisito SC-003 de la especificación
- Suficiente para la mayoría de escenarios reales

**Alternatives considered**:
- Solo referencias directas: Más simple pero no cumple con el requisito de referencias transitivas
- Sin límite de profundidad: Riesgo de análisis infinito o muy lento
- Análisis manual de .csproj: Más complejo y menos confiable

**Implementation approach**:
- Iterar sobre `Compilation.References`
- Para cada referencia, obtener su `Compilation` y buscar instaladores
- Mantener contador de profundidad y limitar a 3 niveles
- Evitar análisis duplicado de ensamblados ya procesados

## Technology Decisions Summary

| Aspecto | Decisión | Justificación |
|---------|----------|---------------|
| Análisis de código | `ISyntaxReceiver` + `Compilation.References` | Análisis estático en compilación, sin reflexión |
| Generación de código | Clase estática con método `LoadAll()` | Eficiente, verificable, debuggable |
| Abstracción IoC | `IIoCContainer` con `Register<T>()` | Balance compatibilidad/simplicidad |
| Configuración orden | Atributo `[InstallOrder]` en host | Resuelto en compilación, declarativo |
| Manejo errores | Continuar + `AggregateException` | Resiliente con visibilidad completa |
| Referencias transitivas | Análisis recursivo hasta 3 niveles | Cumple requisitos, previene loops |

## Open Questions Resolved

✅ **Q1**: ¿Cómo analizar referencias de proyecto? → `Compilation.References` con análisis recursivo  
✅ **Q2**: ¿Cómo generar código eficiente? → Clase estática con invocaciones directas  
✅ **Q3**: ¿Qué abstracción usar para contenedores? → `IIoCContainer` genérico  
✅ **Q4**: ¿Cómo configurar orden? → Atributo `[InstallOrder]` en host  
✅ **Q5**: ¿Cómo manejar errores? → Continuar + excepción agregada  
✅ **Q6**: ¿Cómo manejar referencias transitivas? → Análisis recursivo limitado a 3 niveles

## Next Steps

Proceed to Phase 1: Design & Contracts
- Definir modelo de datos (entidades, interfaces)
- Generar contratos (interfaces, atributos)
- Crear quickstart guide

