# Feature Specification: IoC Install Generator

**Feature Branch**: `001-ioc-install-generator`  
**Created**: 2025-01-27  
**Status**: Draft  
**Input**: User description: "Proof of Concept de un sistema de gestión de IoC multiproyecto en C#. El objetivo es: Tenemos una app, un host central, que depende de N ensamblados. En la inicialización de la App, crea un contenedor IoC y debe pasarlo a todos los ensamblados que implementen una interfaz de 'instalador' de IoC. Para conseguirlo en lugar de reflexion, se propone usar un source generator que analice las referencias de la App para encontrar todos los 'instaladores' y monte un cargador que podamos llamar directamente, lo cual sería mucho más eficiente."

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Descubrimiento Automático de Instaladores (Priority: P1)

Como desarrollador de una aplicación multiproyecto, quiero que el sistema descubra automáticamente todos los instaladores de IoC de los ensamblados referenciados, para que no tenga que registrar manualmente cada instalador y el proceso sea transparente.

**Why this priority**: Esta es la funcionalidad core del sistema. Sin el descubrimiento automático, el sistema no cumple su propósito principal de simplificar la gestión de IoC en arquitecturas multiproyecto.

**Independent Test**: Puede ser probado independientemente creando una aplicación host con referencias a múltiples ensamblados que implementan instaladores, ejecutando el source generator, y verificando que el código generado contiene referencias a todos los instaladores encontrados. El valor entregado es la automatización del descubrimiento sin intervención manual.

**Acceptance Scenarios**:

1. **Given** una aplicación host con referencias a 3 ensamblados, **When** el source generator analiza las referencias del proyecto, **Then** el código generado identifica todos los tipos que implementan la interfaz de instalador en los 3 ensamblados
2. **Given** un ensamblado que no implementa ningún instalador, **When** el source generator analiza las referencias, **Then** el código generado no incluye referencias a ese ensamblado pero continúa procesando otros ensamblados
3. **Given** un ensamblado con múltiples clases que implementan la interfaz de instalador, **When** el source generator analiza las referencias, **Then** el código generado incluye referencias a todas las clases instaladoras encontradas

---

### User Story 2 - Carga de Instaladores en Contenedor IoC (Priority: P2)

Como desarrollador de una aplicación host, quiero que el sistema cargue automáticamente todos los instaladores descubiertos en el contenedor IoC durante la inicialización, para que todas las dependencias de los ensamblados referenciados estén disponibles sin configuración manual adicional.

**Why this priority**: Una vez que los instaladores son descubiertos, deben ser invocados para registrar las dependencias. Esta funcionalidad completa el flujo principal del sistema.

**Independent Test**: Puede ser probado independientemente creando un contenedor IoC, llamando al código generado que carga los instaladores, y verificando que todas las dependencias esperadas están registradas en el contenedor. El valor entregado es la inicialización automática completa del sistema IoC.

**Acceptance Scenarios**:

1. **Given** un contenedor IoC vacío y código generado con referencias a 3 instaladores, **When** se invoca el método generado para cargar instaladores pasando el contenedor, **Then** todos los instaladores son ejecutados y sus dependencias quedan registradas en el contenedor
2. **Given** un instalador que falla durante el registro, **When** se ejecuta la carga de instaladores, **Then** el sistema continúa cargando los demás instaladores y al finalizar lanza una excepción agregada que contiene información sobre todos los fallos ocurridos
3. **Given** múltiples instaladores que registran el mismo tipo de servicio, **When** se ejecuta la carga de instaladores, **Then** el sistema invoca todos los instaladores en orden y permite que el contenedor IoC maneje las políticas de registro según su implementación (último gana, error, etc.). El sistema no impone políticas de registro, solo garantiza la ejecución ordenada de instaladores.

---

### User Story 3 - Mejora de Rendimiento sobre Reflexión (Priority: P3)

Como desarrollador, quiero que el sistema sea más eficiente que soluciones basadas en reflexión, para que la inicialización de la aplicación sea más rápida y el overhead en tiempo de ejecución sea mínimo.

**Why this priority**: La justificación principal del proyecto es demostrar mejoras de rendimiento. Aunque no es crítico para el MVP funcional, es esencial para validar el valor del enfoque.

**Independent Test**: Puede ser probado independientemente comparando el tiempo de inicialización del sistema con source generators versus una implementación equivalente usando reflexión, ejecutando benchmarks con múltiples ensamblados. El valor entregado es la validación de que el enfoque cumple su objetivo de eficiencia.

**Acceptance Scenarios**:

1. **Given** una aplicación con 10 ensamblados referenciados, **When** se mide el tiempo de inicialización usando source generators versus reflexión, **Then** el sistema con source generators completa la inicialización en menos tiempo
2. **Given** el código generado por el source generator, **When** se analiza el código, **Then** no contiene llamadas a reflexión para descubrir instaladores, solo invocaciones directas a métodos conocidos en tiempo de compilación

---

### Edge Cases

- ¿Qué sucede cuando un ensamblado referenciado no está disponible en tiempo de compilación pero sí en tiempo de ejecución?
- ¿Cómo maneja el sistema instaladores en ensamblados que son referencias transitivas (no referencias directas)?
- ¿Qué ocurre si un instalador tiene dependencias circulares con otros instaladores?
- ¿Cómo se comporta el sistema cuando un ensamblado tiene múltiples versiones de la misma interfaz de instalador?
- ¿Qué sucede si el source generator no puede analizar correctamente un ensamblado (formato corrupto, versión incompatible)?

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: El sistema DEBE analizar todas las referencias del proyecto host (directas y transitivas) para descubrir instaladores de IoC
- **FR-002**: El sistema DEBE generar código estático en tiempo de compilación que liste todos los instaladores encontrados
- **FR-003**: El código generado DEBE proporcionar un método invocable que acepte un contenedor IoC y cargue todos los instaladores descubiertos
- **FR-004**: El sistema DEBE identificar clases que implementan la interfaz de instalador definida por el sistema
- **FR-005**: El sistema DEBE manejar casos donde un ensamblado no contiene instaladores sin generar errores
- **FR-006**: El código generado DEBE ser legible y debuggable para facilitar el diagnóstico de problemas. Específicamente: (a) código formateado consistentemente, (b) nombres descriptivos para clases y variables generadas, (c) comentarios XML cuando sea apropiado, (d) estructura clara que permita identificar fácilmente cada instalador invocado.
- **FR-007**: El sistema DEBE funcionar con proyectos que usan PackageReference (SDK-style projects)
- **FR-008**: El sistema DEBE soportar referencias de proyecto y referencias de paquetes NuGet
- **FR-009**: El sistema DEBE proporcionar mensajes de error claros cuando el source generator encuentra problemas durante el análisis. Los mensajes DEBEN incluir: (a) nombre del ensamblado/proyecto donde ocurrió el error, (b) tipo específico de error (ensamblado no encontrado, versión incompatible, etc.), (c) sugerencia de solución cuando sea posible, (d) ubicación del problema (archivo, línea si aplica).
- **FR-010**: El sistema DEBE permitir la invocación de todos los instaladores en una sola llamada durante la inicialización de la aplicación
- **FR-011**: Cuando un instalador falla durante el registro, el sistema DEBE continuar ejecutando los demás instaladores y al finalizar lanzar una excepción agregada que contenga información sobre todos los fallos ocurridos
- **FR-012**: El sistema DEBE permitir al desarrollador configurar el orden de ejecución de los instaladores mediante un atributo en la aplicación host que especifique la lista ordenada de ensamblados a cargar

### Key Entities

- **Instalador de IoC**: Interfaz común que deben implementar todos los módulos que desean registrar dependencias en el contenedor IoC. La interfaz define un método único `Install(IContainer container)` que acepta el contenedor IoC como parámetro y registra las dependencias del módulo.

- **Contenedor IoC**: Contenedor de inversión de control que gestiona el ciclo de vida y resolución de dependencias. El sistema es compatible con cualquier contenedor que implemente una abstracción genérica con método `Register<T>(...)` para registrar dependencias.

- **Código Generado**: Código estático generado por el source generator que contiene referencias a todos los instaladores descubiertos y un método para invocarlos. El orden de ejecución se configura mediante un atributo en la aplicación host que especifica la lista ordenada de ensamblados.

- **Referencia de Ensamblado**: Referencia de proyecto o paquete NuGet que conecta la aplicación host con otros ensamblados que pueden contener instaladores.

## Assumptions

- Los ensamblados referenciados están disponibles en tiempo de compilación para análisis estático
- Todos los instaladores implementan la misma interfaz común definida por el sistema
- El contenedor IoC proporciona una abstracción genérica con método `Register<T>(...)` para registrar dependencias
- Los proyectos usan el formato SDK-style (PackageReference) como estándar
- El sistema operará en un entorno de desarrollo .NET estándar con acceso a herramientas de compilación

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: El sistema puede descubrir e inicializar instaladores de hasta 50 ensamblados referenciados en menos de 100 milisegundos desde el inicio de la aplicación
- **SC-002**: El código generado contiene referencias directas a todos los instaladores encontrados sin requerir descubrimiento dinámico en tiempo de ejecución
- **SC-003**: El sistema puede procesar correctamente proyectos con referencias transitivas de hasta 3 niveles de profundidad
- **SC-004**: El tiempo de inicialización del sistema es al menos 50% más rápido que una implementación equivalente que descubre instaladores dinámicamente en tiempo de ejecución
- **SC-005**: El sistema genera código sin errores de compilación para el 100% de los proyectos que cumplen con los requisitos de la interfaz de instalador
- **SC-006**: Los desarrolladores pueden integrar el sistema en una nueva aplicación multiproyecto en menos de 10 minutos sin documentación adicional

## Clarifications

### Session 2025-01-27

- Q: ¿Cuál es la firma exacta de la interfaz de instalador que deben implementar los módulos? → A: Método único `Install(IContainer container)` - patrón más simple y común
- Q: ¿Qué debe hacer el sistema cuando un instalador lanza una excepción durante el registro? → A: Continuar con otros instaladores y lanzar excepción agregada al final - balance entre resiliencia y visibilidad
- Q: ¿En qué orden deben ejecutarse los instaladores cuando se cargan? → A: Orden configurable por el desarrollador - máximo control pero requiere configuración
- Q: ¿Cómo debe configurarse el orden de ejecución de los instaladores? → A: Atributo en el host con la lista ordenada de ensamblados a cargar
- Q: ¿Qué interfaz mínima común debe implementar el contenedor IoC para ser compatible con el sistema? → A: Abstracción genérica con método `Register<T>(...)` - balance entre compatibilidad y simplicidad
