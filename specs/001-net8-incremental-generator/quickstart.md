# Quickstart Guide: Actualización a .NET 8 e Incremental Source Generator

**Feature**: Actualización a .NET 8 e Incremental Source Generator  
**Date**: 2025-01-11

## Overview

Esta guía te ayudará a actualizar tu proyecto a .NET 8 y aprovechar las mejoras del Incremental Source Generator. El generador ahora usa IIncrementalGenerator que proporciona mejor rendimiento y caching automático.

## Prerequisites

- .NET 8 SDK instalado
- Visual Studio 2022 17.8+ o VS Code con C# Dev Kit
- Proyecto existente usando IoC Install Generator (opcional, para migración)
- O proyecto nuevo que desea usar el generador

## Step 1: Actualizar a .NET 8

### Para proyectos existentes:

1. **Actualizar el archivo .csproj**:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <!-- Otras propiedades... -->
  </PropertyGroup>
</Project>
```

2. **Actualizar paquetes NuGet**:

```xml
<ItemGroup>
  <PackageReference Include="IoC.InstallGenerator" Version="2.0.0" />
</ItemGroup>
```

O usando dotnet CLI:

```bash
dotnet add package IoC.InstallGenerator --version 2.0.0
```

3. **Compilar y verificar**:

```bash
dotnet build
dotnet test
```

### Para proyectos nuevos:

Crea un nuevo proyecto .NET 8:

```bash
dotnet new console -n MyApp -f net8.0
cd MyApp
dotnet add package IoC.InstallGenerator
```

## Step 2: Implementar la Interfaz de Instalador en tus Módulos

En cada módulo que desees registrar dependencias, crea una clase que implemente `IIoCInstaller`:

```csharp
using IoC.InstallGenerator.Abstractions;

namespace MyModule
{
    public class MyModuleInstaller : IIoCInstaller
    {
        public void Install(IIoCContainer container)
        {
            // Registrar dependencias del módulo
            container.Register<IMyService, MyService>();
            container.Register<IOtherService>(new OtherService());
        }
    }
}
```

**Nota**: Esta parte no cambia con .NET 8. La API se mantiene igual para preservar compatibilidad.

## Step 3: Marcar la Clase para Generación

En tu proyecto host, crea una clase parcial y marca con el atributo `[IoCInstallerLoader]`:

```csharp
using IoC.InstallGenerator.Abstractions;

namespace MyApp
{
    [IoCInstallerLoader]
    public static partial class Loader
    {
        // El método LoadAll será generado automáticamente aquí
    }
}
```

## Step 4: Configurar el Orden de Ejecución (Opcional)

Si necesitas controlar el orden de ejecución de los instaladores, agrega el atributo `[InstallOrder]` a tu clase principal:

```csharp
using IoC.InstallGenerator.Abstractions;

[InstallOrder("ModuleA", "ModuleB", "ModuleC")]
public class Program
{
    // ...
}
```

Si no especificas el orden, los instaladores se ejecutarán en orden de descubrimiento (arbitrario).

**Nota**: Esta parte no cambia con .NET 8. La API se mantiene igual.

## Step 5: Crear un Adaptador para tu Contenedor IoC

Crea un adaptador que implemente `IIoCContainer` para tu contenedor específico. Por ejemplo, para Autofac:

```csharp
using IoC.InstallGenerator.Abstractions;
using Autofac;

public class AutofacContainerAdapter : IIoCContainer
{
    private readonly ContainerBuilder _builder;
    
    public AutofacContainerAdapter(ContainerBuilder builder)
    {
        _builder = builder;
    }
    
    public void Register<TService, TImplementation>() 
        where TImplementation : TService
    {
        _builder.RegisterType<TImplementation>().As<TService>();
    }
    
    public void Register<TService>(TService instance)
    {
        _builder.RegisterInstance(instance).As<TService>();
    }
    
    public void Register<TService>(Func<IIoCContainer, TService> factory)
    {
        _builder.Register(c => factory(this)).As<TService>();
    }
}
```

**Nota**: Esta parte no cambia con .NET 8. La API se mantiene igual.

## Step 6: Inicializar el Sistema

En tu método `Main` o punto de entrada de la aplicación, crea el contenedor y carga los instaladores:

```csharp
using IoC.InstallGenerator.Abstractions;

public class Program
{
    public static void Main(string[] args)
    {
        // Crear tu contenedor IoC
        var containerBuilder = new ContainerBuilder(); // Ejemplo con Autofac
        var containerAdapter = new AutofacContainerAdapter(containerBuilder);
        
        // Cargar todos los instaladores automáticamente
        // El método LoadAll es generado automáticamente por el generador incremental
        Loader.LoadAll(containerAdapter);
        
        // Construir el contenedor final
        var container = containerBuilder.Build();
        
        // Tu aplicación continúa normalmente...
    }
}
```

**Nota**: El código generado es idéntico al anterior. Solo cambia la implementación interna del generador (ahora usa IIncrementalGenerator).

## Step 7: Compilar y Verificar

1. **Compila tu proyecto**:

```bash
dotnet build
```

El generador incremental analizará automáticamente las referencias y generará el código. Con IIncrementalGenerator, solo reprocesará archivos modificados en compilaciones incrementales, mejorando los tiempos de compilación.

2. **Verifica el código generado**:

El código generado aparece en:
- Visual Studio: `Dependencies` → `Analyzers` → `IoC.InstallGenerator` → `Loader.g.cs`
- VS Code: En la carpeta `obj/Debug/net8.0/generated/`

3. **Ejecuta tu aplicación**:

```bash
dotnet run
```

Los instaladores se cargarán automáticamente en el orden especificado.

## Beneficios de .NET 8 e IIncrementalGenerator

### Mejoras de Rendimiento

- **Caching automático**: El generador incremental solo reprocesa archivos modificados
- **Menor uso de memoria**: No mantiene el árbol de sintaxis completo en memoria
- **Compilaciones más rápidas**: Especialmente en proyectos grandes con múltiples módulos

### Compatibilidad

- **API sin cambios**: Todas las interfaces y atributos se mantienen igual
- **Código generado idéntico**: El código generado es el mismo que antes
- **Migración sencilla**: Solo necesitas actualizar el TargetFramework y el paquete NuGet

## Troubleshooting

### El código generado no aparece

- Verifica que el paquete NuGet está instalado correctamente
- Asegúrate de que estás usando .NET 8.0
- Revisa que el proyecto usa SDK-style (PackageReference)
- Verifica que la clase está marcada con `[IoCInstallerLoader]` y es `partial`

### Algunos instaladores no se descubren

- Verifica que las clases instaladoras son públicas
- Asegúrate de que las clases implementan `IIoCInstaller` correctamente
- Verifica que los ensamblados están referenciados (directa o transitivamente)
- Compila el proyecto completo para que el generador analice todas las referencias

### Errores durante la carga

- Si un instalador falla, el sistema continuará con los demás
- Al final, se lanzará una `AggregateException` con todos los errores
- Revisa los mensajes de error para identificar qué instalador falló

### El orden de ejecución no se respeta

- Verifica que el atributo `[InstallOrder]` está aplicado correctamente
- Asegúrate de que los nombres de ensamblados en el atributo coinciden exactamente con los nombres reales
- Si no se especifica el atributo, el orden será arbitrario

### Problemas de compilación después de actualizar

- Limpia y reconstruye: `dotnet clean && dotnet build`
- Elimina carpetas `bin/` y `obj/` y vuelve a compilar
- Verifica que todas las referencias de paquetes están actualizadas
- Asegúrate de que el SDK de .NET 8 está instalado: `dotnet --version`

## Ejemplo Completo

Ver el proyecto de demostración en `src/Demo.Host/` y `src/Demo.Modules/` para un ejemplo completo de integración con .NET 8.

## Migración desde Versión Anterior

Si estás migrando desde una versión anterior:

1. **Actualiza el TargetFramework** a `net8.0` en todos los proyectos
2. **Actualiza el paquete NuGet** a la versión 2.0.0+
3. **Marca tu clase con `[IoCInstallerLoader]`** si aún no lo has hecho
4. **Compila y verifica** que todo funciona correctamente

No necesitas cambiar tu código existente. La API se mantiene igual.

## Next Steps

- Revisa la documentación completa en `spec.md`
- Explora los contratos en `contracts/`
- Consulta el modelo de datos en `data-model.md`
- Revisa la investigación sobre IIncrementalGenerator en `research.md`

