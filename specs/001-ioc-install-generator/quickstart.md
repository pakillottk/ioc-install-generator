# Quickstart Guide: IoC Install Generator

**Feature**: IoC Install Generator  
**Date**: 2025-01-27

## Overview

Este guía te ayudará a integrar el IoC Install Generator en tu aplicación multiproyecto en menos de 10 minutos.

## Prerequisites

- .NET 6.0 o superior
- Proyecto host (aplicación principal)
- Módulos referenciados que implementan instaladores IoC

## Step 1: Instalar el Source Generator

Agrega el paquete NuGet del Source Generator a tu proyecto host:

```xml
<ItemGroup>
  <PackageReference Include="IoC.InstallGenerator" Version="1.0.0" />
</ItemGroup>
```

O usando dotnet CLI:

```bash
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

## Step 3: Configurar el Orden de Ejecución (Opcional)

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

## Step 4: Crear un Adaptador para tu Contenedor IoC

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

## Step 5: Inicializar el Sistema

En tu método `Main` o punto de entrada de la aplicación, crea el contenedor y carga los instaladores:

```csharp
using IoC.InstallGenerator.Generated;
using IoC.InstallGenerator.Abstractions;

public class Program
{
    public static void Main(string[] args)
    {
        // Crear tu contenedor IoC
        var containerBuilder = new ContainerBuilder(); // Ejemplo con Autofac
        var containerAdapter = new AutofacContainerAdapter(containerBuilder);
        
        // Cargar todos los instaladores automáticamente
        IoCInstallerLoader.LoadAll(containerAdapter);
        
        // Construir el contenedor final
        var container = containerBuilder.Build();
        
        // Tu aplicación continúa normalmente...
    }
}
```

## Step 6: Compilar y Verificar

1. Compila tu proyecto. El source generator analizará automáticamente las referencias y generará el código.
2. Verifica que el archivo generado aparece en tu proyecto (bajo "Dependencies" → "Analyzers" → "IoC.InstallGenerator").
3. Ejecuta tu aplicación. Los instaladores se cargarán automáticamente en el orden especificado.

## Troubleshooting

### El código generado no aparece

- Verifica que el paquete NuGet está instalado correctamente
- Asegúrate de que estás usando .NET 6.0 o superior
- Revisa que el proyecto usa SDK-style (PackageReference)

### Algunos instaladores no se descubren

- Verifica que las clases instaladoras son públicas
- Asegúrate de que las clases implementan `IIoCInstaller` correctamente
- Verifica que los ensamblados están referenciados (directa o transitivamente)

### Errores durante la carga

- Si un instalador falla, el sistema continuará con los demás
- Al final, se lanzará una `AggregateException` con todos los errores
- Revisa los mensajes de error para identificar qué instalador falló

### El orden de ejecución no se respeta

- Verifica que el atributo `[InstallOrder]` está aplicado correctamente
- Asegúrate de que los nombres de ensamblados en el atributo coinciden exactamente con los nombres reales
- Si no se especifica el atributo, el orden será arbitrario

## Ejemplo Completo

Ver el proyecto de demostración en `src/Demo.Host/` y `src/Demo.Modules/` para un ejemplo completo de integración.

## Next Steps

- Revisa la documentación completa en `spec.md`
- Explora los contratos en `contracts/`
- Consulta el modelo de datos en `data-model.md`

